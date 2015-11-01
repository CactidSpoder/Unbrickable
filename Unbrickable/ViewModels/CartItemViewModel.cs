using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Unbrickable.ViewModels
{
    public class CartItemViewModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string image { get; set; }
        public int quantity { get; set; }
        public decimal price { get; set; }
    }
}