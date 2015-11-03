using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Unbrickable.ViewModels
{
    public class TestViewModel
    {
        [Required]
        public string item_name { get; set; }
        [Required]
        public string item_price { get; set; }
        [Required]
        public string quantity { get; set; }
    }
}