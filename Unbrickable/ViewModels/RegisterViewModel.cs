using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Unbrickable.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [ServerSideValidations.NameFormat(ErrorMessage = "Invalid First Name!")]
        public string first_name { get; set; }
        [Required]
        [ServerSideValidations.NameFormat(ErrorMessage = "Invalid Last Name!")]
        public string last_name { get; set; }
        [Required]
        public int gender_id { get; set; }
        [Required]
        public int salutation_id { get; set; }
        [Required]
        public int birth_day { get; set; }
        [Required]
        public int birth_month { get; set; }
        [Required]
        [ServerSideValidations.Over18(ErrorMessage = "Must be at least 18 years old.")]
        public int birth_year { get; set; }
        public string about_me { get; set; }

        [Required]
        [ServerSideValidations.UsernameFormat(ErrorMessage = "Invalid username.")]
        public string username { get; set; }
        [Required]
        public string password { get; set; }
        [Required]
        public string confirm_password { get; set; }

        public int access_level_id { get; set; }

        public IEnumerable<SelectListItem> Salutations { get; set; }
        public IEnumerable<SelectListItem> access_levels { get; set; }
        public IEnumerable<SelectListItem> days { get; set; }
        public IEnumerable<SelectListItem> months { get; set; }
        public IEnumerable<SelectListItem> years { get; set; }

    }
}