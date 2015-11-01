using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Unbrickable.ViewModels
{
    public class BoardsViewModel
    {
        public int? page { get; set; }
        public IEnumerable<Unbrickable.ViewModels.BoardPostViewModel> Posts { get; set; }
        public IEnumerable<Unbrickable.ViewModels.SearchCriteria> search { get; set; }
    }
}