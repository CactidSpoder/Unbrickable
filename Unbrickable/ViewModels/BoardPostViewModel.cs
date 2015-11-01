using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Unbrickable.ViewModels
{
    public class BoardPostViewModel
    {
        public int id { get; set; }
        public int account_id { get; set; }
        public int access_level_id { get; set; }
        public string username { get; set; }
        public string date_posted_text { get; set; }
        public string entry { get; set; }
        public string name { get; set; }
        public string joined_date_text { get; set; }
        public string date_edited_text { get; set; }

    }
}