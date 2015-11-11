using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Unbrickable.ViewModels
{
    public class LinkedItemViewModel
    {
        public int num { get; set; }
        public int id { get; set; }
        public string image_src { get; set; }
        public string item_name { get; set; }
        public Boolean isChecked { get; set; }
    }
}