using Ganss.XSS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Unbrickable.Models;
using Unbrickable.ViewModels;

namespace Unbrickable.Controllers
{
    [ValidateInput(false)]
    public class AJAXController : Controller
    {
        private UnbrickableDatabase db = new UnbrickableDatabase();

        public ActionResult GenerateDayList(int? month, int? year)
        {
            if (month == null || year == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int month_val = (int)month;
            int year_val = (int)year;
            List<SelectListItem> days = new List<SelectListItem>();
            for (int i = 1; i <= DateTime.DaysInMonth(year_val, month_val); i++)
            {
                days.Add(new SelectListItem() { Text = i.ToString(), Value = i.ToString() });
            }
            return Json(days, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSalutationList(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Gender g = db.Genders.Find(id);
            if (g == null)
            {
                return HttpNotFound();
            }
            List<SelectListItem> Salutations = new List<SelectListItem>();
            foreach (Salutation s in g.Salutations)
            {
                SelectListItem sli = new SelectListItem();
                sli.Text = s.value.ToString();
                sli.Value = s.id.ToString();
                Salutations.Add(sli);
            }
            return Json(Salutations, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditPost(int? id, int? account_id, string val)
        {
            if (Session["User"] == null || id == null || account_id == null || account_id != Convert.ToInt32(Session["User"]))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                Account a = db.Accounts.Find(Session["User"]);
                Post p = db.Posts.Find(id);
                if (a != null && (Session["Elevation"].Equals("Administrator") || p.id == id))
                {
                    if (p != null)
                    {
                        var sanitizer = new HtmlSanitizer();
                        string sanitized = sanitizer.Sanitize(val);
                        p.entry = HttpUtility.HtmlEncode(sanitized);
                        p.editor_id = a.id;
                        db.SaveChanges();
                        return new HttpStatusCodeResult(HttpStatusCode.OK);
                    }
                    else
                    {
                        return HttpNotFound();
                    }
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
        }

        [HttpPost]
        public ActionResult DeletePost(int? id, int? account_id)
        {
            if (Session["User"] == null || id == null || account_id == null || account_id != Convert.ToInt32(Session["User"]))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                Account a = db.Accounts.Find(Session["User"]);
                Post p = db.Posts.Find(id);
                if (p == null || a == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    if (Session["Elevation"].Equals("Administrator") || p.account_id == a.id)
                    {
                        db.Posts.Remove(p);
                        db.SaveChanges();
                        return new HttpStatusCodeResult(HttpStatusCode.OK);
                    }
                    else
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                }                
            }
        }

        public ActionResult AddPostSearchFilter(int? num)
        {
            if (num == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                return PartialView("~/Views/Application/EditorTemplates/PostSearchFilter.cshtml", new PostSearchFilter() { num = (int)num });
            }            
        }

        public ActionResult AddUsernameSearchFilter(int? num)
        {
            if (num == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                return PartialView("~/Views/Application/EditorTemplates/UsernameSearchFilter.cshtml", new UsernameSearchFilter() { num = (int)num });
            }
        }

        public ActionResult AddDateSearchFilter(int? num)
        {
            if (num == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                DateSearchFilter dsf = new DateSearchFilter() { num = (int)num };                
                dsf.date = DateTime.Now;
                return PartialView("~/Views/Application/EditorTemplates/DateSearchFilter.cshtml", dsf);
            }
        }

        public ActionResult AddDateRangeSearchFilter(int? num)
        {
            if (num == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                return PartialView("~/Views/Application/EditorTemplates/DateRangeSearchFilter.cshtml", new DateRangeSearchFilter() { num = (int)num, min_date = DateTime.Now, max_date = DateTime.Now });
            }
        }
        
        [HttpPost]
        public ActionResult DeleteItem(int? id)
        {
            if (Session["User"] == null || id == null || !Session["Elevation"].Equals("Administrator"))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                Item i = db.Items.Find(id);
                if (i == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    db.Items.Remove(i);
                    db.SaveChanges();
                    return new HttpStatusCodeResult(HttpStatusCode.OK);
                }
            }
        }
        
        [HttpPost]
        public ActionResult AddItemToCart(int? id, int? qty)
        {
            if(id == null || qty == null || Session["User"] == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Item i = db.Items.Find(id);
            if (i == null)
            {
                return HttpNotFound();
            }
            else
            {
                CartItemViewModel civm = new CartItemViewModel();

                civm.id = i.id;
                civm.name = i.name;
                civm.price = i.price;
                civm.quantity = (int)qty;

                List<CartItemViewModel> l_civm = (List<CartItemViewModel>)Session["Cart"];

                l_civm.Add(civm);

                Session["Cart"] = l_civm;

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
        }

        [HttpPost]
        public ActionResult EditItemQuantityInCart(int? index, int? qty)
        {
            List<CartItemViewModel> l_civm = (List<CartItemViewModel>)Session["Cart"];
            
            if (index == null || index < 0 || index >= l_civm.Count || qty == null || Session["User"] == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            int indx = (int)index;
            int qnty = (int)qty;
            
            
            if(qnty == 0)
            {
                l_civm.RemoveAt(indx);
            }
            else
            {
                l_civm[indx].quantity = qnty;
            }

            Session["Cart"] = l_civm;
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        public ActionResult DeleteItemQuantityInCart(int? index)
        {
            List<CartItemViewModel> l_civm = (List<CartItemViewModel>)Session["Cart"];
            
            if (index == null || index < 0 || index >= l_civm.Count || Session["User"] == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            l_civm.RemoveAt((int)index);

            Session["Cart"] = l_civm;
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        public ActionResult AddTransaction(int? id)
        {
            if (id == null || Session["User"] == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                Transaction t = db.Transactions.Find(id);
                if (t == null)
                {
                    t = new Transaction();
                    t.id = (int)id;
                    t.account_id = (int)Session["User"];
                    t.transaction_status_id = 0;

                    db.Transactions.Add(t);
                    db.SaveChanges();
                }

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
        }

        [HttpPost]
        public ActionResult EditTransactionStatus(int? id, int? stat)
        {

            if (id == null || Session["User"] == null || stat == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                Transaction t = db.Transactions.Find(id);
                if (t == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    if ((int)stat == 1)
                    {
                        t.transaction_status_id = 1;
                    }
                    else if ((int)stat == 2)
                    {
                        db.Transactions.Remove(t);
                    }

                    db.SaveChanges();
                }
            }
            
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}