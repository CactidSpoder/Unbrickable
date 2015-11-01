using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Unbrickable.ViewModels
{
    public abstract class SearchCriteria
    {
        public SearchCriteria() { }
        public int num { get; set; }
        public Boolean isOr { get; set; }
        public abstract Expression<Func<Unbrickable.Models.Post, bool>> filterPosts(Expression<Func<Unbrickable.Models.Post, bool>> predicate);
        public abstract Boolean verify();
    }
}