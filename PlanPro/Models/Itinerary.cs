//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PlanPro.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Itinerary
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Itinerary()
        {
            this.DayPlans = new HashSet<DayPlan>();
        }
    
        public int Id { get; set; }
        public int TravelPlanId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DayPlan> DayPlans { get; set; }
        public virtual TravelPlan TravelPlan { get; set; }
    }
}
