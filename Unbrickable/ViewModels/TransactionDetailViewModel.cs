using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Unbrickable.ViewModels
{
    public class TransactionDetailViewModel
    {
        public string username { get; set; }
        public IEnumerable<TransactionItemDetailViewModel> transaction_items{ get; set; }
    }
}