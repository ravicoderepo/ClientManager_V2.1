﻿using System;

namespace ClientManager.Models
{
    public class DespatchItemData
    {
        public int Id { get; set; }        
        public int DespatchId { get; set; }
        public int MaterialId { get; set; }
        public string MaterialName { get; set; }
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public string Remarks { get; set; }
    }   
}
