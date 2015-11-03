using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Unbrickable.ViewModels
{
    public class TransactionViewModel
    {
        public int account_id { get; set; }
        public int transaction_status_id { get; set; }
        public string paypal_transaction_id { get; set; }
    }
}