using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Unbrickable.Models;
using Unbrickable.ViewModels;
using PagedList;
using Ganss.XSS;
using LinqKit;
using System.IO;
using System.Configuration;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Data;

namespace Unbrickable.Controllers
{
    [ValidateInput(false)]
    public class ApplicationController : Controller
    {

        private UnbrickableDatabase db = new UnbrickableDatabase();

        public static Boolean verifyDate(int year, int month, int day)
        {
            
            String datestring = month + "/" + day + "/" + year;
            DateTime dt = new DateTime();
            return DateTime.TryParseExact(datestring, "M/d/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
        }

        public ActionResult Index()
        {
            if (Session["User"] != null)
            {
                return RedirectToAction("UserPage", "Application", new { id = Session["User"] });
            }
            else
            {
                return View();
            }
        }
        
        public ActionResult AboutUs()
        {
            return View();
        }

        public ActionResult LoginPage()
        {
            if (Session["User"] != null)
            {
                return RedirectToAction("UserPage", "Application", new { id = Session["User"] });
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LoginPage(LoginViewModel lvm)
        {
            Account a = db.Accounts.Where(x => x.username.Equals(lvm.username)).FirstOrDefault();
            if (a != null)
            {
                if (BCrypt.Net.BCrypt.Verify(lvm.password, Encoding.UTF8.GetString(a.password)))
                {
                    List<CartItemViewModel> civm_list = new List<CartItemViewModel>();
                    
                    Session["User"] = a.id;
                    Session["Elevation"] = a.AccessLevel.value;
                    Session["Name"] = a.username;
                    Session["Cart"] = civm_list;
                    
                    return RedirectToAction("LoggedInProfile", "Application");
                }
                else
                {
                    this.ModelState.AddModelError("username", "Invalid Login.");
                    this.ModelState.AddModelError("password", "Invalid Login.");
                    return View(lvm);
                }
            }
            else
            {
                this.ModelState.AddModelError("username", "Invalid Login.");
                this.ModelState.AddModelError("password", "Invalid Login.");
                return View(lvm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.RemoveAll();
            Session.Abandon();
            return RedirectToAction("Index", "Application");
        }

        private List<SelectListItem> GenerateDayList(int month, int year)
        {
            List<SelectListItem> days = new List<SelectListItem>();
            for (int i = 1; i <= DateTime.DaysInMonth(year, month); i++)
            {
                days.Add(new SelectListItem() { Text = i.ToString(), Value = i.ToString() });
            }
            return days;
        }
        private List<SelectListItem> GenerateMonthList()
        {
            List<SelectListItem> months = new List<SelectListItem>();
            System.Globalization.DateTimeFormatInfo mfi = new System.Globalization.DateTimeFormatInfo();
            for (int i = 1; i <= 12; i++)
            {
                months.Add(new SelectListItem() { Text = mfi.GetMonthName(i).ToString(), Value = i.ToString() });
            }
            return months;
        }

        private List<SelectListItem> GenerateYearList()
        {
            List<SelectListItem> years = new List<SelectListItem>();
            for (int i = DateTime.Now.Year; i >= DateTime.Now.Year - 100; i--)
            {
                years.Add(new SelectListItem() { Text = i.ToString(), Value = i.ToString() });
            }
            return years;
        }

        private RegisterViewModel GenerateRegisterViewModel()
        {
            RegisterViewModel rvm = new RegisterViewModel();
            List<SelectListItem> Salutations = new List<SelectListItem>();
            rvm.Salutations = new SelectList(Salutations, "Value", "Text");
            rvm.birth_day = DateTime.Now.Day;
            rvm.birth_month = DateTime.Now.Month;
            rvm.birth_year = DateTime.Now.Year;
            rvm.days = GenerateDayList(rvm.birth_month, rvm.birth_year);
            rvm.months = GenerateMonthList();
            rvm.years = GenerateYearList();
            rvm.access_levels = new SelectList(db.AccessLevels, "id", "value");
            return rvm;
        }

        public ActionResult RegisterPage()
        {
            return View(GenerateRegisterViewModel());
        }

        private void AddAccount(RegisterViewModel rvm)
        {
            Account a = new Account();
            a.first_name = rvm.first_name;
            a.last_name = rvm.last_name;
            a.username = rvm.username;
            a.password = Encoding.UTF8.GetBytes(BCrypt.Net.BCrypt.HashPassword(rvm.password, BCrypt.Net.BCrypt.GenerateSalt(10)));
            a.salutation_id = rvm.salutation_id;
            a.gender_id = rvm.gender_id;
            if (Session["User"] == null || Session["Elevation"] == null || !Session["Elevation"].Equals("Administrator") || a.access_level_id == 0)
            {
                a.access_level_id = 1;
            }
            else
            {
                a.access_level_id = rvm.access_level_id;
            }
            if (rvm.about_me == null)
            {
                a.about_me = "";
            }
            else
            {
                a.about_me = rvm.about_me;
            }
            String datestring = rvm.birth_month + "/" + rvm.birth_day + "/" + rvm.birth_year;
            DateTime dt = DateTime.ParseExact(datestring, "M/d/yyyy", CultureInfo.InvariantCulture);
            a.birthdate = dt;
            a.date_joined = DateTime.Now;
            db.Accounts.Add(a);
            db.SaveChanges();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterPage(RegisterViewModel rvm)
        {
            if (rvm.password == null || rvm.confirm_password == null || !rvm.password.Equals(rvm.confirm_password))
            {
                this.ModelState.AddModelError("password", "Password Mismatch.");
                this.ModelState.AddModelError("confirm_password", "Password Mismatch.");
            }
            Salutation s = db.Salutations.Find(rvm.salutation_id);
            if (s != null)
            {
                if (s.gender_id != rvm.gender_id)
                {
                    this.ModelState.AddModelError("salutation_id", "Salutation Error.");
                }                
            }
            else
            {
                this.ModelState.AddModelError("salutation_id", "Salutation Error.");
            }
            if ((Session["User"] == null || Session["Elevation"] == null || !Session["Elevation"].Equals("Administrator")) && (rvm.access_level_id != 0))
            {
                this.ModelState.AddModelError("access_level_id", "Creation Error.");
            }
            if (db.Genders.Find(rvm.gender_id) == null)
            {
                this.ModelState.AddModelError("gender_id", "Gender Error.");
            }
            int acc_count = db.Accounts.Where(x => x.username.Equals(rvm.username)).Count();
            if (acc_count > 0)
            {
                this.ModelState.AddModelError("username", "Duplicate Username.");
            }
            if (!verifyDate(rvm.birth_year, rvm.birth_month, rvm.birth_day))
            {
                this.ModelState.AddModelError("birth_month", "Invalid Date.");
                this.ModelState.AddModelError("birth_year", "Invalid Date.");
                this.ModelState.AddModelError("birth_day", "Invalid Date.");
            }
            if (this.ModelState.IsValid)
            {
                AddAccount(rvm);
                return RedirectToAction("LoginPage", "Application");
            }
            else
            {
                List<SelectListItem> Salutations = new List<SelectListItem>();
                Gender g = db.Genders.Find(rvm.gender_id);
                if (g != null)
                {
                    foreach (Salutation s_2 in g.Salutations)
                    {
                        SelectListItem sli = new SelectListItem();
                        sli.Text = s_2.value.ToString();
                        sli.Value = s_2.id.ToString();
                        Salutations.Add(sli);
                    }
                    rvm.Salutations = new SelectList(Salutations, "Value", "Text");
                }
                else
                {
                    rvm.Salutations = new SelectList(Salutations, "Value", "Text");
                }
                rvm.days = GenerateDayList(rvm.birth_month, rvm.birth_year);
                rvm.months = GenerateMonthList();
                rvm.years = GenerateYearList();
                rvm.access_levels = new SelectList(db.AccessLevels, "id", "value");
                this.ModelState.AddModelError("GeneralError", "Error: There are some invalid fields. Please check your input before trying again.");
                return View("RegisterPage", rvm);
            }            
        }

        private UserPageViewModel GenerateUserPageViewModel(Account a)
        {
            UserPageViewModel upvm = new UserPageViewModel();
            upvm.about_me = a.about_me;
            upvm.birthday = a.birthdate;
            upvm.first_name = a.first_name;
            upvm.Gender = a.Gender.value;
            upvm.id = a.id;
            upvm.last_name = a.last_name;
            upvm.Salutation = a.Salutation.value;
            upvm.username = a.username;
            decimal donation_total = a.donation_total;
            int post_count = a.Posts.Count();
            var valid_transactions = a.Transactions.Where(x => x.transaction_status_id == 2);
            decimal purchase_total = 0;
            foreach(Transaction t in valid_transactions)
            {
                foreach(TransactionItem ti in t.TransactionItems)
                {
                    purchase_total += ti.item_price * ti.quantity;
                }
            }
            if(donation_total >= 100)
            {
                upvm.donation_badge_level = 3;
            }
            else if(donation_total >= 20)
            {
                upvm.donation_badge_level = 2;
            }
            else if(donation_total >= 5)
            {
                upvm.donation_badge_level = 1;
            }
            else
            {
                upvm.donation_badge_level = 0;
            }

            if(post_count >= 10)
            {
                upvm.post_badge_level = 3;
            }
            else if (post_count >= 5)
            {
                upvm.post_badge_level = 2;
            }
            else if(post_count >= 3)
            {
                upvm.post_badge_level = 1;
            }
            else
            {
                upvm.post_badge_level = 0;
            }
            
            if(purchase_total >= 100)
            {
                upvm.purchase_badge_level = 3;
            }
            else if(purchase_total >= 20)
            {
                upvm.purchase_badge_level = 2;
            }
            else if(purchase_total >= 5)
            {
                upvm.purchase_badge_level = 1;
            }
            else
            {
                upvm.purchase_badge_level = 0;
            }

            return upvm;
        }

        public ActionResult LoggedInProfile()
        {
            return RedirectToAction("UserPage", "Application", new { id = Session["User"] });
        }

        public ActionResult UserPage(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account a = db.Accounts.Find(id);
            if (a == null)
            {
                return HttpNotFound();
            }
            return View(GenerateUserPageViewModel(a));
        }

        private EditAccountViewModel GenerateEditAccountViewModel(Account a)
        {            
            EditAccountViewModel eavm = new EditAccountViewModel();
            eavm.about_me = a.about_me;
            eavm.access_level_id = a.access_level_id;
            eavm.birth_day = a.birthdate.Day;
            eavm.birth_month = a.birthdate.Month;
            eavm.birth_year = a.birthdate.Year;
            eavm.first_name = a.first_name;
            eavm.gender_id = a.gender_id;
            eavm.id = a.id;
            eavm.last_name = a.last_name;
            eavm.salutation_id = a.salutation_id;
            List<SelectListItem> Salutations = new List<SelectListItem>();
            Gender g = db.Genders.Find(eavm.gender_id);
            if (g != null)
            {
                foreach (Salutation s_2 in g.Salutations)
                {
                    SelectListItem sli = new SelectListItem();
                    sli.Text = s_2.value.ToString();
                    sli.Value = s_2.id.ToString();
                    Salutations.Add(sli);
                }
                eavm.Salutations = new SelectList(Salutations, "Value", "Text");
            }
            else
            {
                eavm.Salutations = new SelectList(Salutations, "Value", "Text");
            }
            eavm.days = GenerateDayList(eavm.birth_month, eavm.birth_year);
            eavm.months = GenerateMonthList();
            eavm.years = GenerateYearList();
            eavm.access_levels = new SelectList(db.AccessLevels, "id", "value");
            return eavm;
        }

        public ActionResult EditPage()
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("LoginPage", "Application");
            }
            else
            {
                Account a = db.Accounts.Find(Session["User"]);
                return View(GenerateEditAccountViewModel(a));
            }
        }

        private void EditAccount(EditAccountViewModel eavm)
        {
            Account a = db.Accounts.Find(Session["User"]);
            a.first_name = eavm.first_name;
            a.last_name = eavm.last_name;
            a.salutation_id = eavm.salutation_id;
            a.gender_id = eavm.gender_id;
            if (Session["User"] != null && Session["Elevation"] != null && Session["Elevation"].Equals("Administrator"))
            {
                a.access_level_id = eavm.access_level_id;
            }
            a.about_me = eavm.about_me;
            String datestring = eavm.birth_month + "/" + eavm.birth_day + "/" + eavm.birth_year;
            DateTime dt = DateTime.ParseExact(datestring, "M/d/yyyy", CultureInfo.InvariantCulture);
            a.birthdate = dt;
            db.SaveChanges();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPage(EditAccountViewModel eavm)
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("LoginPage", "Application");
            }
            else if (eavm.id != Convert.ToInt32(Session["User"]))
            {
                return RedirectToAction("LoggedInProfile");
            }
            else
            {
                Salutation s = db.Salutations.Find(eavm.salutation_id);
                if (s != null)
                {
                    if (s.gender_id != eavm.gender_id)
                    {
                        this.ModelState.AddModelError("salutation_id", "Salutation Error.");
                    }
                }
                else
                {
                    this.ModelState.AddModelError("salutation_id", "Salutation Error.");
                }
                if (db.Genders.Find(eavm.gender_id) == null)
                {
                    this.ModelState.AddModelError("gender_id", "Gender Error.");
                }
                Account a = db.Accounts.Find(Session["User"]);
                if (a == null)
                {
                    this.ModelState.AddModelError("id", "General Error.");
                }
                else if (a.access_level_id != eavm.access_level_id && Session["Elevation"].Equals("Administrator"))
                {
                    this.ModelState.AddModelError("access_level_id", "Access Error.");
                }
                if (this.ModelState.IsValid)
                {
                    EditAccount(eavm);
                    return RedirectToAction("LoggedInProfile", "Application");
                }
                else
                {
                    List<SelectListItem> Salutations = new List<SelectListItem>();
                    Gender g = db.Genders.Find(eavm.gender_id);
                    if (g != null)
                    {
                        foreach (Salutation s_2 in g.Salutations)
                        {
                            SelectListItem sli = new SelectListItem();
                            sli.Text = s_2.value.ToString();
                            sli.Value = s_2.id.ToString();
                            Salutations.Add(sli);
                        }
                        eavm.Salutations = new SelectList(Salutations, "Value", "Text");
                    }
                    else
                    {
                        eavm.Salutations = new SelectList(Salutations, "Value", "Text");
                    }
                    eavm.days = GenerateDayList(eavm.birth_month, eavm.birth_year);
                    eavm.months = GenerateMonthList();
                    eavm.years = GenerateYearList();
                    eavm.access_levels = new SelectList(db.AccessLevels, "id", "value");
                    this.ModelState.AddModelError("GeneralError", "Error: There are some invalid fields. Please check your input before trying again.");
                    return View("EditPage", eavm);
                }
            }            
        }

        private ChangePasswordViewModel GenerateChangePasswordViewModel()
        {
            ChangePasswordViewModel cpvm = new ChangePasswordViewModel();
            cpvm.id = Convert.ToInt32(Session["User"]);
            return cpvm;
        }

        public ActionResult ChangePassword()
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("LoginPage", "Application");
            }
            else
            {
                return View(GenerateChangePasswordViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel cpvm)
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("LoginPage", "Application");
            }
            else if (cpvm.id != Convert.ToInt32(Session["User"]))
            {
                return RedirectToAction("LoggedInProfile");
            }
            else if (!this.ModelState.IsValid)
            {
                return View(cpvm);
            }
            else
            {
                if (!cpvm.new_password.Equals(cpvm.confirm_password))
                {
                    this.ModelState.AddModelError("new_password", "Mismatch.");
                    this.ModelState.AddModelError("confirm_password", "Mismatch.");
                }
                Account a = db.Accounts.Find(Session["User"]);
                if (a == null)
                {
                    this.ModelState.AddModelError("password", "Error");
                }
                if (!BCrypt.Net.BCrypt.Verify(cpvm.old_password, Encoding.UTF8.GetString(a.password)))
                {
                    this.ModelState.AddModelError("old_password", "Error");
                }
                if (this.ModelState.IsValid)
                {
                    a.password = Encoding.UTF8.GetBytes(BCrypt.Net.BCrypt.HashPassword(cpvm.new_password, BCrypt.Net.BCrypt.GenerateSalt(10)));
                    db.SaveChanges();
                    return RedirectToAction("LoggedInProfile", "Application");
                }
                else
                {
                    return View(cpvm);
                }
            }
        }

        public BoardPostViewModel GetBoardPostViewModel(Post p)
        {
            BoardPostViewModel bpvm = new BoardPostViewModel();
            bpvm.access_level_id = p.Account.access_level_id;
            if (p.Editor == null || p.date_edited == null)
            {
                bpvm.date_edited_text = "";
            }
            else
            {
                bpvm.date_edited_text = "Edited by " + p.Editor.username + " on " + p.date_edited.ToString() + ".";
            }
            bpvm.date_posted_text = p.date_posted.ToString();
            bpvm.entry = p.entry;
            bpvm.id = p.id;
            bpvm.account_id = p.account_id;
            bpvm.joined_date_text = "Member since " + p.Account.birthdate.ToString("MMMM d, yyyy");
            bpvm.username = p.Account.username;
            bpvm.name = p.Account.first_name + " " + p.Account.last_name;
            List<LinkedItemViewModel> l_livm = new List<LinkedItemViewModel>();
            foreach(LinkedItem li in p.LinkedItems.ToList())
            {
                LinkedItemViewModel livm = new LinkedItemViewModel();
                livm.id = li.item_id;
                livm.item_name = li.Item.name;
                l_livm.Add(livm);
            }
            bpvm.linked_items = l_livm;
            return bpvm;
        }

        private IPagedList<BoardPostViewModel> GetPosts(int? page)
        {
            var page_number = page ?? 1;
            var item_count = 10;
            List<BoardPostViewModel> l_bpvm = new List<BoardPostViewModel>();
            foreach (Post p in db.Posts.OrderByDescending(x => x.date_posted).ToList())
            {
                l_bpvm.Add(GetBoardPostViewModel(p));
            }
            return l_bpvm.ToPagedList(page_number, item_count);
        }

        public ActionResult Boards(int? page)
        {
            if ((db.Posts.Count()+9)/10 >= (page ?? 1) && (page == null || page > 0))
            {                
                return View(GetPosts(page));
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);               
            }
        }

        private List<LinkedItemViewModel> GetAllItemsAsLinkedItems()
        {
            List<LinkedItemViewModel> l_livm = new List<LinkedItemViewModel>();
            int ix = 0;
            foreach(Item i in db.Items.ToList())
            {
                LinkedItemViewModel livm = new LinkedItemViewModel();
                livm.num = ix;
                livm.id = i.id;
                livm.image_src = GetImageURL(i.name + ".png");
                livm.isChecked = false;
                livm.item_name = i.name;
                l_livm.Add(livm);
                ix++;
            }
            return l_livm;
        }

        public NewPostViewModel GetNewPostViewModel(object id)
        {
            NewPostViewModel npvm = new NewPostViewModel();
            npvm.id = Convert.ToInt32(id);
            npvm.linked_items = GetAllItemsAsLinkedItems();
            return npvm;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Post(NewPostViewModel npvm)
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("LoginPage", "Application");
            }
            else if (npvm.id != Convert.ToInt32(Session["User"]))
            {
                return RedirectToAction("LoggedInProfile");
            }
            else if (this.ModelState.IsValid)
            {
                Post p = new Post();
                Account a = db.Accounts.Find(Session["User"]);

                if (a == null)
                {
                    return RedirectToAction("LoginPage", "Application");
                }
                else
                {
                    p.account_id = a.id;
                    var sanitizer = new HtmlSanitizer();
                    if (npvm.entry == null)
                    {
                        npvm.entry = "";
                    }
                    string sanitized = sanitizer.Sanitize(npvm.entry);
                    p.entry = HttpUtility.HtmlEncode(sanitized);
                    p.date_posted = DateTime.Now;
                    foreach(LinkedItemViewModel livm in npvm.linked_items.Where(x => x.isChecked == true).ToList())
                    {
                        LinkedItem li = new LinkedItem();
                        li.item_id = livm.id;
                        li.post_id = p.id;
                        db.LinkedItems.Add(li);
                    }
                    db.Posts.Add(p);
                    db.SaveChanges();
                    return RedirectToAction("Boards", "Application");
                }
            }
            else
            {
                return RedirectToAction("Boards", "Application");
            }
        }

        public ActionResult PagedSearchResults(string json)
        {
            if (json == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                BoardsViewModel bvm = new BoardsViewModel();
                try
                {
                    bvm = JsonConvert.DeserializeObject<BoardsViewModel>(json, new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.Objects
                    });
                }
                catch (Exception)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                if (bvm != null && bvm.search != null && bvm.search.Count() > 0)
                {
                    Debug.WriteLine("Criteria Count: " + bvm.search.Count());
                    var predicate = PredicateBuilder.True<Post>();
                    foreach (SearchCriteria sc in bvm.search)
                    {
                        if (sc != null)
                        {
                            if (!sc.verify())
                            {
                                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                            }
                            else
                            {
                                if (sc.isOr && predicate.ToString().Equals("f => True"))
                                {
                                    predicate = PredicateBuilder.False<Post>();
                                    Debug.WriteLine("False");
                                }
                                else if (predicate.ToString().Equals("f => True"))
                                {
                                    Debug.WriteLine("True");
                                }
                                predicate = sc.filterPosts(predicate);
                            }
                        }
                        else
                        {
                            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                        }
                    }
                    var Posts = db.Posts.AsExpandable().Where(predicate);
                    Debug.WriteLine("Result Count: " + Posts.Count());
                    foreach (Post p in Posts)
                    {
                        Debug.WriteLine(p.entry);
                    }
                    List<BoardPostViewModel> l_bpvm = new List<BoardPostViewModel>();
                    foreach (Post p in Posts.OrderByDescending(x => x.date_posted).ToList())
                    {
                        l_bpvm.Add(GetBoardPostViewModel(p));
                    }
                    bvm.Posts = l_bpvm.ToPagedList(bvm.page ?? 1, 10);
                    return View("SearchResults", bvm);
                }
                else
                {
                    return RedirectToAction("Boards", "Application");
                }
            }            
        }
        
        public ActionResult SearchResults(BoardsViewModel bvm)
        {            
            int pageNumber = bvm.page ?? 1;

            if (bvm.search != null && bvm.search.Count() > 0)
            {
                Debug.WriteLine("Criteria Count: " + bvm.search.Count());
                var predicate = PredicateBuilder.True<Post>();
                foreach (SearchCriteria sc in bvm.search)
                {
                    if (sc != null)
                    {
                        if (!sc.verify())
                        {
                            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                        }
                        else
                        {
                            if (sc.isOr && predicate.ToString().Equals("f => True"))
                            {
                                predicate = PredicateBuilder.False<Post>();
                                Debug.WriteLine("False");
                            }
                            else if (predicate.ToString().Equals("f => True"))
                            {
                                Debug.WriteLine("True");
                            }
                            predicate = sc.filterPosts(predicate);
                        }                        
                    }
                    else
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                }
                var Posts = db.Posts.AsExpandable().Where(predicate);
                Debug.WriteLine("Result Count: " + Posts.Count());
                foreach (Post p in Posts)
                {
                    Debug.WriteLine(p.entry);
                }
                List<BoardPostViewModel> l_bpvm = new List<BoardPostViewModel>();
                foreach (Post p in Posts.OrderByDescending(x => x.date_posted).ToList())
                {
                    l_bpvm.Add(GetBoardPostViewModel(p));
                }
                bvm.Posts = l_bpvm.ToPagedList(pageNumber, 10);
                bvm.search = bvm.search;
                return View(bvm);
            }
            else
            {
                return RedirectToAction("Boards", "Application");
            }
        }

        private List<BackUpViewModel> GetAllBackUps()
        {
            List<BackUpViewModel> l_buvm = new List<BackUpViewModel>();
            foreach (BackUp bu in db.BackUps.ToList())
            {
                BackUpViewModel buvm = new BackUpViewModel();
                buvm.date = bu.date_uploaded.ToString("MM/dd/yyyy hh:mm:ss tt");
                buvm.num_posts = bu.num_posts;
                buvm.id = bu.id;
                l_buvm.Add(buvm);
            }
            return l_buvm;
        }

        private List<TransactionViewModel> GetAllTransactions()
        {
            List<TransactionViewModel> l_tvm = new List<TransactionViewModel>();           

            foreach(Transaction t in db.Transactions.ToList())
            {
                TransactionViewModel tvm = new TransactionViewModel();
                tvm.username = t.Account.username;
                tvm.name = t.Account.first_name + " " + t.Account.last_name;
                decimal total = 0;
                foreach(TransactionItem ti in t.TransactionItems.ToList())
                {
                    total += (ti.item_price * ti.quantity);
                }
                tvm.total = total;
                tvm.id = t.id;
                tvm.transaction_status = t.TransactionStatus.value;
                l_tvm.Add(tvm);
            }

            return l_tvm;
        }

        private AdminPageViewModel GenerateAdminPageViewModel()
        {
            AdminPageViewModel apvm = new AdminPageViewModel();
            List<SelectListItem> Salutations = new List<SelectListItem>();
            apvm.Salutations = new SelectList(Salutations, "Value", "Text");
            apvm.birth_day = DateTime.Now.Day;
            apvm.birth_month = DateTime.Now.Month;
            apvm.birth_year = DateTime.Now.Year;
            apvm.days = GenerateDayList(apvm.birth_month, apvm.birth_year);
            apvm.months = GenerateMonthList();
            apvm.years = GenerateYearList();
            apvm.access_levels = new SelectList(db.AccessLevels, "id", "value");
            apvm.BackUps = GetAllBackUps();
            apvm.transactions = GetAllTransactions();
            return apvm;
        }

        public ActionResult AdminPage()
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("LoginPage", "Application");
            }
            else if (!Session["Elevation"].Equals("Administrator"))
            {
                return RedirectToAction("LoggedInProfile");
            }
            else
            {
                return View(GenerateAdminPageViewModel());
            }            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdminRegisterPage (RegisterViewModel rvm)
        {
            if (rvm.password == null || rvm.confirm_password == null || !rvm.password.Equals(rvm.confirm_password))
            {
                this.ModelState.AddModelError("password", "Password Mismatch.");
                this.ModelState.AddModelError("confirm_password", "Password Mismatch.");
            }
            Salutation s = db.Salutations.Find(rvm.salutation_id);
            if (s != null)
            {
                if (s.gender_id != rvm.gender_id)
                {
                    this.ModelState.AddModelError("salutation_id", "Salutation Error.");
                }
            }
            else
            {
                this.ModelState.AddModelError("salutation_id", "Salutation Error.");
            }
            if ((Session["User"] == null || Session["Elevation"] == null || !Session["Elevation"].Equals("Administrator")) && (rvm.access_level_id != 0))
            {
                this.ModelState.AddModelError("access_level_id", "Creation Error.");
            }
            if (db.Genders.Find(rvm.gender_id) == null)
            {
                this.ModelState.AddModelError("gender_id", "Gender Error.");
            }
            int acc_count = db.Accounts.Where(x => x.username.Equals(rvm.username)).Count();
            if (acc_count > 0)
            {
                this.ModelState.AddModelError("username", "Duplicate Username.");
            }
            if (!verifyDate(rvm.birth_year, rvm.birth_month, rvm.birth_day))
            {
                this.ModelState.AddModelError("birth_month", "Invalid Date.");
                this.ModelState.AddModelError("birth_year", "Invalid Date.");
                this.ModelState.AddModelError("birth_day", "Invalid Date.");
            }
            if (this.ModelState.IsValid)
            {
                AddAccount(rvm);
                return RedirectToAction("LoginPage", "Application");
            }
            else
            {
                AdminPageViewModel apvm = new AdminPageViewModel();
                apvm.about_me = rvm.about_me;
                apvm.access_level_id = rvm.access_level_id;
                apvm.access_levels = new SelectList(db.AccessLevels, "id", "value");
                apvm.birth_day = rvm.birth_day;
                apvm.birth_month = rvm.birth_month;
                apvm.birth_year = rvm.birth_year;
                apvm.confirm_password = rvm.confirm_password;
                apvm.days = GenerateDayList(apvm.birth_month, apvm.birth_year);
                apvm.first_name = rvm.first_name;
                apvm.gender_id = rvm.gender_id;
                apvm.last_name = rvm.last_name;
                apvm.months = GenerateMonthList();
                apvm.password = rvm.password;
                apvm.salutation_id = rvm.salutation_id;
                List<SelectListItem> Salutations = new List<SelectListItem>();
                Gender g = db.Genders.Find(apvm.gender_id);
                if (g != null)
                {
                    foreach (Salutation s_2 in g.Salutations)
                    {
                        SelectListItem sli = new SelectListItem();
                        sli.Text = s_2.value.ToString();
                        sli.Value = s_2.id.ToString();
                        Salutations.Add(sli);
                    }
                    apvm.Salutations = new SelectList(Salutations, "Value", "Text");
                }
                else
                {
                    apvm.Salutations = new SelectList(Salutations, "Value", "Text");
                }
                apvm.username = rvm.username;
                apvm.years = GenerateYearList();
                apvm.BackUps = GetAllBackUps();
                this.ModelState.AddModelError("GeneralError", "Error: There are some invalid fields. Please check your input before trying again.");
                return View("AdminPage", apvm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BackUpPosts()
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("LoginPage", "Application");
            }
            else if (!Session["Elevation"].Equals("Administrator"))
            {
                return RedirectToAction("LoggedInProfile");
            }
            else
            {
                DateTime currDate = DateTime.Now;
                string fileName = currDate.ToString("yyyy-MM-dd hh-mm-ss-tt") + ".csv";
                CloudStorageAccount csa = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["UnbrickableStorage"].ConnectionString);
                CloudBlobClient cbcl = csa.CreateCloudBlobClient();
                CloudBlobContainer cbcon = cbcl.GetContainerReference("backups");
                cbcon.CreateIfNotExists();
                CloudBlockBlob cbb = cbcon.GetBlockBlobReference(fileName);

                string fileLocation = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/"), fileName);

                if (System.IO.File.Exists(fileLocation))
                {
                    System.IO.File.Delete(fileLocation);
                }
                using (CSVFileWriter writer = new CSVFileWriter(fileLocation))
                {
                    CSVRowModel row = new CSVRowModel();
                    row.Add("username");
                    row.Add("date_posted");
                    row.Add("entry");
                    writer.WriteRow(row);
                    foreach (Post p in db.Posts.ToList())
                    {
                        row = new CSVRowModel();
                        row.Add(p.Account.username);
                        row.Add(p.date_posted.ToString());
                        row.Add(p.entry);

                        writer.WriteRow(row);
                    }

                    BackUp bu = new BackUp();
                    bu.filename = fileName;
                    bu.date_uploaded = currDate;
                    bu.num_posts = db.Posts.Count();
                    db.BackUps.Add(bu);
                    db.SaveChanges();
                    
                }

                using (var fileStream = System.IO.File.OpenRead(fileLocation))
                {
                    cbb.UploadFromStream(fileStream);
                }
                if (System.IO.File.Exists(fileLocation))
                {
                    System.IO.File.Delete(fileLocation);
                }
                return RedirectToAction("AdminPage", "Application");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DownloadBackUp(int? id)
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("LoginPage", "Application");
            }
            else if (!Session["Elevation"].Equals("Administrator"))
            {
                return RedirectToAction("LoggedInProfile");
            }
            else
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                BackUp bu = db.BackUps.Find(id);
                if (bu == null)
                {
                    return HttpNotFound();
                }

                DateTime currDate = DateTime.Now;
                string fileName = bu.filename;
                CloudStorageAccount csa = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["UnbrickableStorage"].ConnectionString);
                CloudBlobClient cbcl = csa.CreateCloudBlobClient();
                CloudBlobContainer cbcon = cbcl.GetContainerReference("backups");
                cbcon.CreateIfNotExists();
                CloudBlockBlob cbb = cbcon.GetBlockBlobReference(fileName);
                if (cbb.Exists())
                {
                    Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
                    cbb.DownloadToStream(Response.OutputStream);
                    return new HttpStatusCodeResult(HttpStatusCode.OK);
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
        }

        private ItemViewModel GetItemViewModel(Item i)
        {
            ItemViewModel ivm = new ItemViewModel();
            ivm.image_src = GetImageURL(i.name + ".png");
            ivm.id = i.id;
            ivm.name = i.name;
            ivm.price = i.price;
            ivm.description = i.description;
            ivm.quantity = 1;
            return ivm;
        }

        private EditItemViewModel GetEditItemViewModel(Item i)
        {
            EditItemViewModel eivm = new EditItemViewModel();
            eivm.id = i.id;
            eivm.name = i.name;
            eivm.price = i.price;            
            eivm.description = i.description;
            eivm.orig_image = GetImageURL(eivm.name + ".png");
            return eivm;
        }

        private IPagedList<ItemViewModel> GetItems(int? page)
        {
            var page_number = page ?? 1;
            var item_count = 12;
            List<ItemViewModel> l_ivm = new List<ItemViewModel>();
            foreach (Item i in db.Items.ToList())
            {
                l_ivm.Add(GetItemViewModel(i));
            }
            return l_ivm.ToPagedList(page_number, item_count);
        }

        public ActionResult Store(int? page)
        {
            if ((db.Items.Count() + 9) / 10 >= (page ?? 1) && (page == null || page > 0))
            {
                return View(GetItems(page));
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        public ActionResult ViewItem(int? id)
        {
            if(id == null)
            {
                Debug.WriteLine("null id");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Item i = db.Items.Find(id);
            if(i == null)
            {
                Debug.WriteLine("item not found");
                return HttpNotFound();
            }
            else
            {
                return View(GetItemViewModel(i));
            }
        }

        private bool IsImage(HttpPostedFileBase file)
        {
            if (file.ContentType.Contains("image"))
            {
                return true;
            }

            string[] formats = new string[] { ".jpg", ".png", ".gif", ".jpeg" }; // add more if u like...

            // linq from Henrik Stenbæk
            return formats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }

        private void UploadImage(HttpPostedFileBase file, string filename)
        {
            if (file != null)
            {
                if (IsImage(file))
                {
                    CloudStorageAccount csa = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["UnbrickableStorage"].ConnectionString);
                    CloudBlobClient cbcl = csa.CreateCloudBlobClient();
                    CloudBlobContainer cbcon = cbcl.GetContainerReference("images");
                    if (cbcon.CreateIfNotExists())
                    {
                        cbcon.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
                    }
                    CloudBlockBlob cbb = cbcon.GetBlockBlobReference(filename);
                    cbb.UploadFromStream(file.InputStream);
                }
            }           
        }

        private string GetImageURL(string filename)
        {
            CloudStorageAccount csa = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["UnbrickableStorage"].ConnectionString);
            CloudBlobClient cbcl = csa.CreateCloudBlobClient();
            CloudBlobContainer cbcon = cbcl.GetContainerReference("images");
            if (cbcon.CreateIfNotExists())
            {
                cbcon.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            }
            CloudBlockBlob cbb = cbcon.GetBlockBlobReference(filename);
            if (cbb.Exists())
            {
                return cbb.Uri.ToString();
            }
            else
            {
                return "";
            }
        }

        public ActionResult NewItem()
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("LoginPage", "Application");
            }
            else if (!Session["Elevation"].Equals("Administrator"))
            {
                return RedirectToAction("LoggedInProfile");
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewItem(NewItemViewModel nivm)
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("LoginPage", "Application");
            }
            else if (!Session["Elevation"].Equals("Administrator"))
            {
                return RedirectToAction("LoggedInProfile");
            }
            else
            {
                if (db.Items.Where(x => x.name == nivm.name).Count() > 0)
                {
                    this.ModelState.AddModelError("name", "Error: Duplicate Name");
                }
                if (ModelState.IsValid) // checks required in model. does server validation
                {
                    Item i = new Item();                    
                    i.name = nivm.name;
                    i.description = nivm.description;
                    i.price = nivm.price;
                    UploadImage(nivm.image, nivm.name + ".png");
                    db.Items.Add(i);
                    db.SaveChanges();
                    return RedirectToAction("Store", "Application");
                }
                else
                {
                    return RedirectToAction("Store", "Application");
                }
            }
            
        }

        public ActionResult EditItem(int? id)
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("LoginPage", "Application");
            }
            else if (!Session["Elevation"].Equals("Administrator"))
            {
                return RedirectToAction("LoggedInProfile");
            }
            else
            {
                if (id == null)
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
                    return View(GetEditItemViewModel(i));
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditItem(EditItemViewModel eivm)
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("LoginPage", "Application");
            }
            else if (!Session["Elevation"].Equals("Administrator"))
            {
                return RedirectToAction("LoggedInProfile");
            }
            else
            {
                Item i = db.Items.Find(eivm.id);
                if(i == null)
                {
                    this.ModelState.AddModelError("id", "General Error");
                }
                if (ModelState.IsValid) // checks required in model. does server validation
                {
                    i.name = eivm.name;
                    i.description = eivm.description;
                    i.price = eivm.price;
                    UploadImage(eivm.image, eivm.name + ".png");
                    db.SaveChanges();

                    return RedirectToAction("Store", "Application");
                }
                else
                {
                    return RedirectToAction("Store", "Application");
                }
            }
        }

        private int GetIndexOfItem(List<CartItemViewModel> l_civm, int id)
        {
            for(int i=0;i<l_civm.Count; i++)
            {
                if(l_civm[i].id == id)
                {
                    return i;
                }
            }
            return -1;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddItemToCart(AddCartItemViewModel acivm)
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("LoginPage", "Application");
            }
            else if (acivm.quantity <= 0)
            {
                return RedirectToAction("ViewItem", "Application", new { id = acivm.id});
            }
            else
            {
                List<CartItemViewModel> l_civm = (List<CartItemViewModel>)Session["Cart"];
                if(l_civm == null)
                {
                    l_civm = new List<CartItemViewModel>();
                }
                Item i = db.Items.Find(acivm.id);
                if (i == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                else
                {
                    int index = GetIndexOfItem(l_civm, i.id);
                    if (index == -1)
                    {
                        CartItemViewModel civm = new CartItemViewModel();
                        civm.id = acivm.id;
                        civm.name = i.name;
                        civm.price = i.price;
                        civm.quantity = acivm.quantity;
                        civm.image = GetImageURL(i.name+ ".png");
                        l_civm.Add(civm);
                        Session["Cart"] = l_civm;
                    }
                    else
                    {
                        l_civm[index].quantity += acivm.quantity;
                    }
                    return RedirectToAction("Store", "Application");
                }
            }            
        }
        
        
        public ActionResult DonatePackA()
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("LoginPage", "Application");
            }
            else// if (this.ModelState.IsValid)
            {
                //Post p = new Post();
                Account a = db.Accounts.Find(Session["User"]);

                if (a == null)
                {
                    return RedirectToAction("LoginPage", "Application");
                }
                else
                {
                    Debug.WriteLine("got here pendejoz");
                    a.donation_total += 5;
                    db.SaveChanges();
                    Debug.WriteLine("DONATION NOTED");
                    return RedirectToAction("AboutUs", "Application");
                }
            }
        }

        public ActionResult DonatePackB()
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("LoginPage", "Application");
            }
            else// if (this.ModelState.IsValid)
            {
                //Post p = new Post();
                Account a = db.Accounts.Find(Session["User"]);

                if (a == null)
                {
                    return RedirectToAction("LoginPage", "Application");
                }
                else
                {
                    Debug.WriteLine("got here pendejoz");
                    a.donation_total += 10;
                    db.SaveChanges();
                    Debug.WriteLine("DONATION NOTED");
                    return RedirectToAction("AboutUs", "Application");
                }
            }
        }

        public ActionResult DonatePackC()
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("LoginPage", "Application");
            }
            else// if (this.ModelState.IsValid)
            {
                //Post p = new Post();
                Account a = db.Accounts.Find(Session["User"]);

                if (a == null)
                {
                    return RedirectToAction("LoginPage", "Application");
                }
                else
                {
                    Debug.WriteLine("got here pendejoz");
                    a.donation_total += 20;
                    db.SaveChanges();
                    Debug.WriteLine("DONATION NOTED");
                    return RedirectToAction("AboutUs", "Application");
                }
            }
        }
    }
}