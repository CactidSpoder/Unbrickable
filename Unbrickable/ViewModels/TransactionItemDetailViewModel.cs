using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Unbrickable.ViewModels
{
    public class TransactionItemDetailViewModel
    {
        public string item_name { get; set; }
        public int item_quantity { get; set; }
        public decimal item_price { get; set; }
        public decimal total_price { get; set; }
    }
}