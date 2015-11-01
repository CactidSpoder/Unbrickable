using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace Unbrickable.ViewModels
{
    public class DateSearchFilter : SearchCriteria
    {
        public DateSearchFilter()
        {
            List<SelectListItem> operations = new List<SelectListItem>();
            operations.Add(new SelectListItem()
            {
                Text = "After",
                Value = "1"
            });
            operations.Add(new SelectListItem()
            {
                Text = "Before",
                Value = "2"
            });
            operations.Add(new SelectListItem()
            {
                Text = "On",
                Value = "3"
            });
            this.operations = operations;
        }
        public int operation { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime date { get; set; }

        public List<SelectListItem> operations { get; set; }
        override public Expression<Func<Unbrickable.Models.Post, bool>> filterPosts(Expression<Func<Unbrickable.Models.Post, bool>> predicate)
        {
            if (operation != 1 && operation != 2 && operation != 3)
            {
                return predicate;
            }
            if (this.isOr)
            {
                Debug.WriteLine("Or " + this.operation + " on " + this.date.Date);
                switch (operation)
                {
                    case 1: return predicate.Or(x => DbFunctions.TruncateTime(x.date_posted) >= this.date);
                    case 2: return predicate.Or(x => DbFunctions.TruncateTime(x.date_posted) <= this.date);
                    case 3: return predicate.Or(x => DbFunctions.TruncateTime(x.date_posted) == this.date);
                    default: return predicate;
                } 
            }
            else
            {
                Debug.WriteLine("And " + this.operation + " on " + this.date.Date);
                switch (operation)
                {
                    case 1: return predicate.And(x => DbFunctions.TruncateTime(x.date_posted) >= this.date);
                    case 2: return predicate.And(x => DbFunctions.TruncateTime(x.date_posted) <= this.date);
                    case 3: return predicate.And(x => DbFunctions.TruncateTime(x.date_posted) == this.date);
                    default: return predicate;
                }
            }
        }

        public override Boolean verify()
        {
            return Unbrickable.Controllers.ApplicationController.verifyDate(this.date.Year, this.date.Month, this.date.Day);
        }
    }
}