//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IVISS
{
    using System;
    using System.Collections.Generic;
    
    public partial class Organization
    {
        public Organization()
        {
            this.Guards = new HashSet<Guard>();
            this.Facilities = new HashSet<Facility>();
            this.Visitors = new HashSet<Visitor>();
        }
    
        public int organization_id { get; set; }
        public string organization_name { get; set; }
    
        public virtual ICollection<Guard> Guards { get; set; }
        public virtual ICollection<Facility> Facilities { get; set; }
        public virtual ICollection<Visitor> Visitors { get; set; }
    }
}
