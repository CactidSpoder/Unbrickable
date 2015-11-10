using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Unbrickable.ViewModels
{
    public class NewPostViewModel
    {
        public int id { get; set; }
        public string entry { get; set; }
        public IEnumerable<LinkedItemViewModel> linked_items { get; set; }
    }
}