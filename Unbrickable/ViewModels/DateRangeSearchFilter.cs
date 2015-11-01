using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Unbrickable.ViewModels
{
    public class DateRangeSearchFilter : SearchCriteria
    {
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime min_date { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime max_date { get; set; }
        override public Expression<Func<Unbrickable.Models.Post, bool>> filterPosts(Expression<Func<Unbrickable.Models.Post, bool>> predicate)
        {
            
            if (this.isOr)
            {
                Debug.WriteLine("Or " + this.min_date.Date + " to " + this.max_date.Date);
                return predicate.Or(x => DbFunctions.TruncateTime(x.date_posted) >= min_date && DbFunctions.TruncateTime(x.date_posted) <= max_date);
            }
            else
            {
                Debug.WriteLine("And " + this.min_date.Date + " to " + this.max_date.Date);
                return predicate.And(x => DbFunctions.TruncateTime(x.date_posted) >= min_date && DbFunctions.TruncateTime(x.date_posted) <= max_date);
            }
        }

        public override Boolean verify()
        {
            return Unbrickable.Controllers.ApplicationController.verifyDate(this.min_date.Year, this.min_date.Month, this.min_date.Day)
                && Unbrickable.Controllers.ApplicationController.verifyDate(this.max_date.Year, this.max_date.Month, this.max_date.Day)
                && this.max_date >= this.min_date;
        }

    }
}