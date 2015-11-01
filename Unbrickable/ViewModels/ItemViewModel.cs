using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Unbrickable.ViewModels
{
    public class ItemViewModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string image_src { get; set; }
        public decimal price { get; set; }
    }
}