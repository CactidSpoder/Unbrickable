//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Unbrickable.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class TransactionItem
    {
        public int id { get; set; }
        public int transaction_id { get; set; }
        public string item_name { get; set; }
        public decimal item_price { get; set; }
        public int quantity { get; set; }
    
        public virtual Transaction Transaction { get; set; }
    }
}
