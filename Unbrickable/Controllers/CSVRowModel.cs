using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Unbrickable.Controllers
{
    public class CSVRowModel : List<String>
    {
        public string LineText { get; set; }
    }
}