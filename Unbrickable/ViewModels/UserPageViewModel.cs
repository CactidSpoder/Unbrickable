using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Unbrickable.ViewModels
{
    public class UserPageViewModel
    {
        public int id { get; set; }
        public string username { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string Gender { get; set; }
        public DateTime birthday { get; set; }
        public string Salutation { get; set; }
        public string about_me { get; set; }
        public int post_badge_level { get; set; }
        public int donation_badge_level { get; set; }
        public int purchase_badge_level { get; set; }
    }
}