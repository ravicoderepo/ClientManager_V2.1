//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DBOperation
{
    using System;
    using System.Collections.Generic;
    
    public partial class Item
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Item()
        {
            this.DespatchItems = new HashSet<DespatchItem>();
            this.VRM_InwardStock = new HashSet<VRM_InwardStock>();
        }
    
        public int ItemId { get; set; }
        public Nullable<int> ParentId { get; set; }
        public int MaterialId { get; set; }
        public int TypeId { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public Nullable<int> MinimumAvailableQuantity { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DespatchItem> DespatchItems { get; set; }
        public virtual Material Material { get; set; }
        public virtual Type Type { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VRM_InwardStock> VRM_InwardStock { get; set; }
    }
}
