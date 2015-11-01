using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Unbrickable.ViewModels
{
    public class ChangePasswordViewModel
    {
        public int id { get; set; }
        [Required]
        public string old_password { get; set; }
        [Required]
        public string new_password { get; set; }
        [Required]
        public string confirm_password { get; set; }
    }
}