using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Unbrickable.ViewModels
{
    public class EditItemViewModel
    {
        [Required]
        public int id { get; set; }
        [Required]
        public string name { get; set; }
        public string description { get; set; }

        public string orig_image { get; set; }

        public HttpPostedFileBase image { get; set; }
        [Required]
        public decimal price { get; set; }
    }
}