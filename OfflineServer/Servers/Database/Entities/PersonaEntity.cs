﻿using System;
using System.Collections.Generic;

namespace OfflineServer.Servers.Database.Entities
{
    public class PersonaEntity
    {
        public virtual Int32 id { get; protected set; }
        public virtual UInt64 timePlayed { get; set; }
        public virtual Int16 iconIndex { get; set; }
        public virtual String name { get; set; }
        public virtual String motto { get; set; }
        public virtual Int32 level { get; set; }
        public virtual Int32 cash { get; set; }
        public virtual Int32 boost { get; set; }
        public virtual Int16 percentageOfLevelCompletion { get; set; }
        public virtual Int32 reputationInLevel { get; set; }
        public virtual Int32 reputationInTotal { get; set; }
        public virtual Int32 currentCarIndex { get; set; }
        public virtual Int32 score { get; set; }
        public virtual Int32 rating { get; set; }
        public virtual IList<CarEntity> garage { get; protected set; }
        public virtual IList<InventoryItemEntity> inventory { get; protected set; }

        public PersonaEntity()
        {
            garage = new List<CarEntity>();
            inventory = new List<InventoryItemEntity>();
        }

        public virtual void addCar(CarEntity car)
        {
            car.ownerPersona = this;
            garage.Add(car);
        }
        public virtual void removeCar(CarEntity car)
        {
            garage.Remove(car);
        }

        public virtual void addInventoryItem(InventoryItemEntity inventoryItem)
        {
            inventoryItem.ownerPersona = this;
            inventory.Add(inventoryItem);
        }
        public virtual void removeInventoryItem(InventoryItemEntity inventoryItem)
        {
            inventory.Remove(inventoryItem);
        }
    }
}