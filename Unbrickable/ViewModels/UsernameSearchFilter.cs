using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Unbrickable.ViewModels
{
    public class UsernameSearchFilter : SearchCriteria
    {
        public string username { get; set; }
        override public Expression<Func<Unbrickable.Models.Post, bool>> filterPosts(Expression<Func<Unbrickable.Models.Post, bool>> predicate)
        {
            this.username = this.username ?? "";
            if (this.isOr)
            {
                Debug.WriteLine("Or " + this.username);
                return predicate.Or(x => x.Account.username.Equals(this.username));
            }
            else
            {
                Debug.WriteLine("And " + this.username);
                return predicate.And(x => x.Account.username.Equals(this.username));
            }
        }

        public override Boolean verify()
        {
            return true;
        }
    }
}