using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Unbrickable.ViewModels
{
    public class PostSearchFilter : SearchCriteria
    {
        public string search_contents { get; set; }
        override public Expression<Func<Unbrickable.Models.Post, bool>> filterPosts(Expression<Func<Unbrickable.Models.Post, bool>> predicate)
        {
            this.search_contents = this.search_contents ?? "";
            if (this.isOr)
            {
                Debug.WriteLine("Or " + this.search_contents);
                return predicate.Or(x => x.entry.Contains(this.search_contents));
            }
            else
            {
                Debug.WriteLine("And " + this.search_contents);
                return predicate.And(x => x.entry.Contains(this.search_contents));
            }
        }

        public override Boolean verify()
        {
            return true;
        }

    }
}