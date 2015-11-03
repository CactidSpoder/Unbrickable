using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Unbrickable.Controllers
{
    using PayPal.Api;
    using System.Diagnostics;
    using System.Net;
    using Models;
    using ViewModels;

    public class PaypalController : Controller
    {
        private UnbrickableDatabase db = new UnbrickableDatabase();

        // GET: Paypal
        public ActionResult Index()
        {
            return View(Session["Cart"]);
        }

        public static class Configuration
        {
            //these variables will store the clientID and clientSecret
            //by reading them from the web.config
            public readonly static string ClientId;
            public readonly static string ClientSecret;

            static Configuration()
            {
                var config = GetConfig();
                ClientId = config["clientId"];
                ClientSecret = config["clientSecret"];
            }

            // getting properties from the web.config
            public static Dictionary<string, string> GetConfig()
            {
                return PayPal.Api.ConfigManager.Instance.GetProperties();
            }

            private static string GetAccessToken()
            {
                // getting accesstocken from paypal     
                /*                string accessToken = new OAuthTokenCredential
                            (ClientId, ClientSecret, GetConfig()).GetAccessToken();
                                var config = ConfigManager.Instance.GetProperties();
                                return accessToken;*/
                var config = ConfigManager.Instance.GetProperties();
                var accessToken = new OAuthTokenCredential(config).GetAccessToken();
                return accessToken;
            }

            public static APIContext GetAPIContext()
            {
                // return apicontext object by invoking it with the accesstoken
                APIContext apiContext = new APIContext(GetAccessToken());
                apiContext.Config = GetConfig();
                return apiContext;
            }
        }

        private bool IsCartEmpty()
        {
            if(Session["Cart"] == null)
            {
                return true;
            }
            else
            {
                List<CartItemViewModel> l_civm = (List<CartItemViewModel>)Session["Cart"];
                if (l_civm.Count > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            
        }
        
        public ActionResult PaymentWithPaypal()
        {

            if (Session["User"] != null)
            {
                if (!IsCartEmpty())
                {
                    //getting the apiContext as earlier
                    APIContext apiContext = Configuration.GetAPIContext();

                    string payerId = Request.Params["PayerID"];

                    if (string.IsNullOrEmpty(payerId))
                    {
                        //this section will be executed first because PayerID doesn't exist
                        //it is returned by the create function call of the payment class

                        // Creating a payment
                        // baseURL is the url on which paypal sendsback the data.
                        // So we have provided URL of this controller only
                        string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority +
                                    "/Paypal/PaymentWithPayPal?";

                        string cancelURI = Request.Url.Scheme + "://" + Request.Url.Authority +
                                    "/Paypal/CancelPaymentWithPayPal?";

                        //guid we are generating for storing the paymentID received in session
                        //after calling the create function and it is used in the payment execution

                        var guid = Convert.ToString((new Random()).Next(100000));

                        //CreatePayment function gives us the payment approval url
                        //on which payer is redirected for paypal account payment


                        Models.Transaction t = new Models.Transaction();
                        t.account_id = Convert.ToInt32(Session["User"]);
                        t.transaction_status_id = 1;

                        var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid, cancelURI + "guid=" + guid, t);
                        t.paypal_transaction_id = createdPayment.id;

                        db.Transactions.Add(t);

                        db.SaveChanges();
                        //get links returned from paypal in response to Create function call

                        var links = createdPayment.links.GetEnumerator();
                        string paypalRedirectUrl = null;

                        while (links.MoveNext())
                        {
                            Links lnk = links.Current;

                            if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                            {
                                //saving the payapalredirect URL to which user will be redirected for payment
                                paypalRedirectUrl = lnk.href;
                            }
                        }
                        // saving the paymentID in the key guid
                        Session.Add(guid, createdPayment.id);                        
                        
                        return Redirect(paypalRedirectUrl);
                    }
                    else
                    {
                        // This section is executed when we have received all the payments parameters

                        // from the previous call to the function Create

                        // Executing a payment

                        var guid = Request.Params["guid"];

                        var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                        string trans_id = executedPayment.id;
                        Models.Transaction t = db.Transactions.Where(x => x.paypal_transaction_id == trans_id).FirstOrDefault();
                        if(t == null)
                        {
                            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                        }
                        else
                        {
                            if (executedPayment.state.ToLower() == "approved")
                            {
                                t.transaction_status_id = 2;
                                Session["Cart"] = new List<CartItemViewModel>();
                                db.SaveChanges();
                                return RedirectToAction("Index", "Paypal");
                            }
                            else
                            {
                                return RedirectToAction("Index", "Paypal");
                            }
                        }                        
                    }

                    //NOTICE PLEBEIAN
                }
                else
                {
                    return RedirectToAction("Store", "Application");
                }
                
            }
            else
            { 
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
        }

        public ActionResult CancelPaymentWithPaypal()
        {
            var guid = Request.Params["guid"];
            string payment_id = Session[guid] as string;
            Models.Transaction t = db.Transactions.Where(x => x.paypal_transaction_id == payment_id).FirstOrDefault();
            if(t != null)
            {
                t.transaction_status_id = 3;
                db.SaveChanges();
                return RedirectToAction("Index", "Paypal");
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        private PayPal.Api.Payment payment;

        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution() { payer_id = payerId };
            this.payment = new Payment() { id = paymentId };
            
            Session["Cart"] = new List<CartItemViewModel>();
            return this.payment.Execute(apiContext, paymentExecution);
        }

        private Payment CreatePayment(APIContext apiContext, string returnUrl, string cancelUrl, Models.Transaction t)
        {
            //similar to credit card create itemlist and add item objects to it
            var itemList = new ItemList() { items = new List<PayPal.Api.Item>() };
            decimal tax = 0, shipping = 0;
            decimal t_subtotes = 0;
            List<CartItemViewModel> l_civm = (List<CartItemViewModel>)Session["Cart"];

            /*
                Insert Transaction To Database; Set Status to 1 (NotPaid)
                Make sure to create TransactionItems
            */

            foreach (CartItemViewModel civm in l_civm)
            {
                TransactionItem ti = new TransactionItem();
                ti.transaction_id = t.id;
                ti.item_name = civm.name;
                ti.item_price = civm.price;
                ti.quantity = civm.quantity;
                db.TransactionItems.Add(ti);
                itemList.items.Add(new PayPal.Api.Item()
                {
                    name = civm.name,
                    currency = "USD",
                    price = Math.Round(civm.price, 2).ToString(),
                    quantity = civm.quantity.ToString(),
                    sku = civm.id.ToString()
                });

                t_subtotes += civm.price * civm.quantity;
                // similar as we did for credit card, do here and create details object
            }
            var totes = tax + shipping + t_subtotes;

            var payer = new Payer() { payment_method = "paypal" };

            // Configure Redirect Urls here with RedirectUrls object
            var redirUrls = new RedirectUrls()
            {
                cancel_url = cancelUrl,
                return_url = returnUrl
            };

            var details = new Details()
            {
                tax = Math.Round(tax, 2).ToString(),
                shipping = Math.Round(shipping, 2).ToString(),
                subtotal = Math.Round(t_subtotes, 2).ToString()
            };            

            // similar as we did for credit card, do here and create amount object
            var amount = new Amount()
            {
                currency = "USD",
                total = Math.Round(totes, 2).ToString(), // Total must be equal to sum of shipping, tax and subtotal.
                details = details
            };
            

            var transactionList = new List<PayPal.Api.Transaction>();

            transactionList.Add(new PayPal.Api.Transaction()
            {
                description = "Transaction description.",
                invoice_number = Session["User"].ToString() + "-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                amount = amount,
                item_list = itemList
            });

            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };
            
            // Create a payment using a APIContext
            return this.payment.Create(apiContext);
        }
    }
}