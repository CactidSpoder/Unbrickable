using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Unbrickable.ViewModels
{
    public class TransactionStatusViewModel
    {
        [Required]
        public int id { get; set; }

        [Required]
        public int a_id { get; set; }

        [Required]
        public int status { get; set; }
    }
}