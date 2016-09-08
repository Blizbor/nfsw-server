﻿using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using OfflineServer.Servers.Database;
using OfflineServer.Servers.Database.Entities;
using System;
using System.ComponentModel;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace OfflineServer
{
    public partial class MainWindow : MetroWindow
    {
        private DispatcherTimer RandomPersonaInfo = new DispatcherTimer();
        public Access Access { get; set; }
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MainWindow()
        {
            Logger.Setup();
            log.Info("Application started.");

            #region Culture Independency
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-GB");
            XmlLanguage xMarkup = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name);
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(xMarkup));
            FrameworkContentElement.LanguageProperty.OverrideMetadata(typeof(System.Windows.Documents.TextElement), new FrameworkPropertyMetadata(xMarkup));
            log.Info("Culture independency achieved.");
            #endregion

            vCreateDb();

            Access = new Access();

            log.Info("Starting session.");
            Access.CurrentSession.startSession();
            InitializeComponent();
            SetupComponents();
        }

        private void vCreateDb()
        {
            if (!File.Exists("ServerData\\Personas.db"))
            {
                log.Warn("Database doesn't exist!");
                if (!Directory.Exists("ServerData")) Directory.CreateDirectory("ServerData");

                log.Info("Creating database.");
                var sessionFactory = SessionManager.createDatabase();

                log.Info("Setting minimum pkPersonaId to 100.");
                using (var sqliteConnection = new SQLiteConnection("Data Source=\"ServerData\\Personas.db\";Version=3;"))
                {
                    sqliteConnection.Open();
                    SQLiteCommand insertSQL = new SQLiteCommand("INSERT INTO sqlite_sequence (name, seq) VALUES ('Personas', 99)", sqliteConnection);
                    insertSQL.ExecuteNonQuery();
                    sqliteConnection.Close();
                }


                log.Info("Inserting filler entries.");
                using (var session = sessionFactory.OpenSession())
                using (var transaction = session.BeginTransaction())
                {
                    UserEntity userEntity = new UserEntity();
                    userEntity.defaultPersonaIdx = 0;

                    for (int i = 0; i < 20; i++)
                    {
                        PersonaEntity personaEntity = new PersonaEntity();
                        personaEntity.boost = 7331;
                        personaEntity.cash = 1337;
                        personaEntity.currentCarIndex = 0;
                        personaEntity.iconIndex = 27;
                        personaEntity.level = 60;
                        personaEntity.motto = "test";
                        personaEntity.name = "DEBUG Id" + (i + 100);
                        personaEntity.percentageOfLevelCompletion = 100;
                        personaEntity.rating = 8752;
                        personaEntity.reputationInLevel = 0;
                        personaEntity.reputationInTotal = 99999999;
                        personaEntity.score = 2578;

                        CarEntity carEntity = new CarEntity();
                        carEntity.baseCarId = 1816139026L;
                        carEntity.durability = 100;
                        carEntity.heatLevel = 6;
                        carEntity.carId = 1;
                        carEntity.paints = "<Paints/>";
                        carEntity.performanceParts = "<PerformanceParts/>";
                        carEntity.physicsProfileHash = 4123572107L;
                        carEntity.raceClass = CarClass.A;
                        carEntity.rating = 750;
                        carEntity.resalePrice = 123456789;
                        carEntity.skillModParts = "<SkillModParts/>";
                        carEntity.vinyls = "<Vinyls/>";
                        carEntity.visualParts = "<VisualParts/>";

                        personaEntity.addCar(carEntity);
                        session.Save(personaEntity);
                    }
                    session.Save(userEntity);
                    transaction.Commit();
                    log.Info("Database actions finalized.");
                }
            }
            else { log.Info("Database already exists, skipping creation."); }
        }

        private void SetupComponents()
        {
            #region FlipViewPersona
            FlipViewPersonaImage.HideControlButtons();

            Grid[] aFlipViewAvatarArray = new Grid[28];
            for (int i = 0; i < 28; i++)
            {
                Grid Grid_FlipViewDummy;
                Image Image_FlipViewDummy;
                Image_FlipViewDummy = new Image() { Margin = new Thickness(7.4d), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Stretch = Stretch.Uniform, Source = (ImageSource)BitmapFrame.Create(new Uri("pack://application:,,,/OfflineServer;component/images/NFSW_Avatars/Avatar_" + i.ToString() + ".png", UriKind.Absolute)) };
                Grid_FlipViewDummy = new Grid() { Margin = new Thickness(-5d) };
                Grid_FlipViewDummy.Children.Add(Image_FlipViewDummy);
                Image t1 = new Image() { Source = Image_FlipViewDummy.Source };
                t1.Effect = new BlurEffect() { Radius = 6.55d, RenderingBias = RenderingBias.Performance, KernelType = KernelType.Box };
                Grid_FlipViewDummy.Background = new VisualBrush((Visual)t1);
                aFlipViewAvatarArray[i] = Grid_FlipViewDummy;
            }
            FlipViewPersonaImage.ItemsSource = aFlipViewAvatarArray;

            Binding indexBind = new Binding()
            {
                Path = new PropertyPath("ActivePersona.IconIndex"),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.TwoWay,
                Source = Access.CurrentSession
            };
            BindingOperations.SetBinding(FlipViewPersonaImage, FlipView.SelectedIndexProperty, indexBind);
            #endregion

            #region MetroTile -> Random Persona Info
            tRandomPersonaInfo_Tick(null, null);
            RandomPersonaInfo.Tick += new EventHandler(tRandomPersonaInfo_Tick);
            RandomPersonaInfo.Interval = new TimeSpan(0, 0, 10);
            RandomPersonaInfo.Start();
            #endregion
        }

        private void tRandomPersonaInfo_Tick(object sender, EventArgs e)
        {
            DockPanel dNewInfoContent = Access.CurrentSession.Engine.Achievements.generateNewAchievement();
            metrotileRandomPersonaInfo.Content = dNewInfoContent;
        }

        private async void buttonStartServer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Access.sHttp = new Servers.Http.HttpServer();
            Access.sXmpp = new Servers.Xmpp.BasicXmppServer();

            MetroDialogSettings messageBoxStyle = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Right away!",
                ColorScheme = MetroDialogOptions.ColorScheme
            };

            // gonna keep this until I add nfs:w launching support
            await this.ShowMessageAsync("Servers are up and running!", "Go ahead and launch NFS: World now.", MessageDialogStyle.Affirmative, messageBoxStyle);

            /*test

            MessageDialogResult result = await this.ShowMessageAsync("Hello!", "It seems like this is your first time running this build of the Offline Server! Would you like some help?", MessageDialogStyle.AffirmativeAndNegative, mySettings);

            if (result != MessageDialogResult.FirstAuxiliary)
            {

            }*/
        }

        private void Button_ClickHandler(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case "buttonOpenBasicPersonaInfo":
                    flyoutBasicPersonaInfo.IsOpen = !flyoutBasicPersonaInfo.IsOpen;
                    break;
                case "buttonOpenDetailedPersonaInfo":
                    flyoutDetailedPersonaInfo.IsOpen = !flyoutDetailedPersonaInfo.IsOpen;
                    break;
                case "buttonOpenPersonaList":
                    flyoutPersonaList.IsOpen = !flyoutPersonaList.IsOpen;
                    break;
                case "buttonUpdatePersonaInfoTile":
                    RandomPersonaInfo.Stop();
                    tRandomPersonaInfo_Tick(null, null);
                    RandomPersonaInfo.Start();
                    break;
                case "buttonPaints":
                case "buttonPerformanceParts":
                case "buttonSkillModParts":
                case "buttonVinyls":
                case "buttonVisualParts":
                    tbGaragePartInfo.SetBinding(MVVMSyntax._TextProperty,
                        new Binding()
                        {
                            Converter = new STEditConverter(this),
                            Path = new PropertyPath("ActivePersona.SelectedCar." + ((sender as Button).Name).Remove(0, 6)),
                            UpdateSourceTrigger = UpdateSourceTrigger.LostFocus,
                            Mode = BindingMode.TwoWay,
                            Source = Access.CurrentSession
                        });
                    flyoutGaragePartInfo.IsOpen = !flyoutGaragePartInfo.IsOpen;
                    break;
                case "buttonAddCar":
                    break;
                case "buttonRemoveCar":
                    break;
            }
        }
        public class STEditConverter : IValueConverter
        {
            private MainWindow _Window;
            public STEditConverter(MainWindow dlWindow)
            {
                _Window = dlWindow;
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value.ToString();
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                try
                {
                    return System.Xml.Linq.XElement.Parse(value.ToString());
                }
                catch (Exception ex)
                {
                    BindingOperations.ClearBinding(_Window.tbGaragePartInfo, MVVMSyntax._TextProperty);
                    _Window.flyoutGaragePartInfo.IsOpen = false;
                    MessageBox.Show(ex.Message, "ERROR: Not valid input", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
        }

        #region PersonaList related events
        private void datagridPersonaList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Persona mSelectedPersona = datagridPersonaList.SelectedItem as Persona;
            Access.CurrentSession.ActivePersona = mSelectedPersona;
        }

        private void flyoutBasicPersonaInfo_IsOpenChanged(object sender, RoutedEventArgs e)
        {
            if (!flyoutBasicPersonaInfo.IsOpen)
            {
                textboxPersonaName.Text = textboxPersonaName.Text.Trim();
                Int32 iPersonaIndex = Access.CurrentSession.PersonaList.IndexOf(Access.CurrentSession.PersonaList.First<Persona>(sPersona => sPersona.Id == Access.CurrentSession.ActivePersona.Id));
                Access.CurrentSession.PersonaList[iPersonaIndex] = Access.CurrentSession.ActivePersona;
            }
        }
        #endregion

        #region FlipViewPersonaImage events
        private void FlipViewPersonaImage_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as FlipView).ShowControlButtons();
        }

        private void FlipViewPersonaImage_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as FlipView).HideControlButtons();
        }
        #endregion

        private void textboxPersonaName_LostFocus(object sender, RoutedEventArgs e)
        {
            textboxPersonaName.Text = textboxPersonaName.Text.Trim();
            if (textboxPersonaName.Text.Length < 1)
            {
                informUser("Sorry, the persona name cannot be empty.");
                if (textboxPersonaName.CanUndo)
                {
                    do
                    {
                        textboxPersonaName.Undo();
                    } while (textboxPersonaName.Text.Trim().Length < 1);
                }
                else
                {
                    textboxPersonaName.Text = "CHANGE ME";
                }
            }
        }

        private void informUser(String infoText)
        {
            this.ShowMessageAsync("Oops!", infoText, MessageDialogStyle.Affirmative);
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            log.Info("Shutting down offline server.");

            if (Access.sHttp != null && Access.sXmpp != null)
            {
                // https://github.com/foxglovesec/Potato/blob/master/source/NHttp/NHttp/HttpServer.cs#L261
                Access.sHttp.nServer.Stop();
                Access.sHttp.nServer.Dispose();
                log.Info("Shutdown of HttpServer has been completed.");

                Access.sXmpp.shutdown();
            }

            NfswSession.dbConnection.Close();
            NfswSession.dbConnection.Dispose();

            SessionManager.getSessionFactory().Close();
            SessionManager.getSessionFactory().Dispose();

            log.Info("Killing main thread.");
        }
    }
}