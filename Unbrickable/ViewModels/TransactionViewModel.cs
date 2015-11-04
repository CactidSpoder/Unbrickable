using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Unbrickable.ViewModels
{
    public class TransactionViewModel
    {
        public int id { get; set; }
        public string username { get; set; }
        public string name { get; set; }
        public string transaction_status { get; set; }
        public decimal total { get; set; }
    }
}