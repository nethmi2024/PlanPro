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
    
    public partial class DayPlan
    {
        public int Id { get; set; }
        public int ItineraryId { get; set; }
        public System.DateTime Date { get; set; }
        public string Activities { get; set; }
    
        public virtual Itinerary Itinerary { get; set; }
    }
}
