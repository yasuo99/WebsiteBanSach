using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookStore.Data;
using BookStore.Helper;
using BookStore.Models;
using BookStore.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayPal.Api;
using BookStore.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace BookStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class PaypalController : Controller
    {
        private Payment payment;
        private int invoiceID;
        private int paymentTotal;
        private readonly ApplicationDbContext _db;

        public PaypalController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }

        [ActionName("PaymentWithPaypal")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> PaymentWithPaypal()
        {
            APIContext apiContext = Configuration.GetAPIContext();
            try
            {
                List<ShoppingSessionViewModel> lstCartItems = HttpContext.Session.Get<List<ShoppingSessionViewModel>>("ssShoppingCart");
                var user = await _db.ApplicationUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefaultAsync();
                Models.Order order = new Models.Order();
                string payerId = Request.Query["PayerID"].ToString();
                if (string.IsNullOrEmpty(payerId))
                {

                    order.ApplicationUserID = user.Id;
                    order.ApplicationUser = user;
                    order.Name = user.Name;
                    order.Address = user.Address;
                    order.PhoneNumber = user.PhoneNumber;
                    order.Date = DateTime.Now;
                    order.DiscountID = 1;
                    order.Total = 0;
                    _db.Orders.Add(order);
                    await _db.SaveChangesAsync();                   
                    invoiceID = order.ID;
                    string baseURL = GetRawUrl(Request) + "?";
                    var guid = Convert.ToString((new Random()).Next(100000));
                    var createdPayment = this.CreatePayment(apiContext, lstCartItems, baseURL + "guid=" + guid, user);
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null;
                    while (links.MoveNext())
                    {
                        Links link = links.Current;
                        if (link.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            paypalRedirectUrl = link.href;
                        }
                    }
                    HttpContext.Session.SetString(guid, createdPayment.id);
                    HttpContext.Session.SetString("invoiceId", invoiceID.ToString());
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    var getInvoiceId = Int32.Parse(HttpContext.Session.GetString("invoiceId"));
                    double total = 0;
                    foreach (var item in lstCartItems)
                    {
                        var book = _db.Books.Where(u => u.ID == item.Book.ID).FirstOrDefault();
                        book.Available -= item.Amount;
                        _db.Books.Update(book);
                        await _db.SaveChangesAsync();
                        OrderDetail orderDetail = new OrderDetail()
                        {
                            BookID = item.Book.ID,
                            OrderID = getInvoiceId,
                            Amount = item.Amount,
                            TotalPrice = item.Book.BookPrice * item.Amount
                        };
                        total += item.Book.BookPrice * item.Amount;
                        _db.OrderDetails.Add(orderDetail);
                        await _db.SaveChangesAsync();
                    }
                    var guid = Request.Query["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, HttpContext.Session.GetString(guid));                   
                    var getOrder = _db.Orders.Where(u => u.ID == getInvoiceId).FirstOrDefault();
                    getOrder.State = "Đã thanh toán";
                    getOrder.Total = total;
                    _db.Orders.Update(getOrder);
                    _db.SaveChanges();
                    lstCartItems = new List<ShoppingSessionViewModel>();
                    HttpContext.Session.Set("ssShoppingCart", lstCartItems);
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("FailureView");
                    }
                }
            }
            catch (Exception ex)
            {
                var getInvoiceId = Int32.Parse(HttpContext.Session.GetString("invoiceId"));
                var getOrder = _db.Orders.Where(u => u.ID == getInvoiceId).FirstOrDefault();
                _db.Orders.Remove(getOrder);
                await _db.SaveChangesAsync();
                Logger.Log("Error" + ex.Message);
                return View("FailureView");
            }
            return View("Success");
        }
        public static string GetRawUrl(HttpRequest request)
        {
            var httpContext = request.HttpContext;
            return $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}{httpContext.Request.QueryString}";
        }
        private Payment CreatePayment(APIContext apiContext, List<ShoppingSessionViewModel> lstCartItems, string redirectUrl,ApplicationUser user)
        {
            var payer = new Payer() { payment_method = "paypal" };
            //tạo danh sách sản phẩm chọn mua
            double withoutTax = 0;
            ItemList itemList = new ItemList() { items = new List<Item>() };
            foreach (var item in lstCartItems)
            {
                var priceToUSD = Math.Round(item.Book.BookPrice/22000, MidpointRounding.AwayFromZero);
                itemList.items.Add(new Item
                {
                    name = item.Book.BookName,
                    currency = "USD",
                    price = priceToUSD.ToString(),
                    quantity = item.Amount.ToString(),
                    sku = item.Book.ID.ToString(),
                });
                withoutTax += priceToUSD*item.Amount;
            }
            var tax = withoutTax * 0.1;
            var shipping = withoutTax * 0.05;
            // similar as we did for credit card, do here and create details object
            var details = new Details()
            {
                tax = Math.Round(tax, MidpointRounding.AwayFromZero).ToString(),
                shipping = Math.Round(shipping, MidpointRounding.AwayFromZero).ToString(),
                subtotal = Math.Round(withoutTax, MidpointRounding.AwayFromZero).ToString()
            };
            var total = withoutTax + tax + shipping;
            paymentTotal = Int32.Parse(Math.Round(total, MidpointRounding.AwayFromZero).ToString());
            // similar as we did for credit card, do here and create amount object
            var amount = new Amount()
            {              
                currency = "USD",
                
                total = paymentTotal.ToString(), // Total must be equal to sum of shipping, tax and subtotal.
                details = details
            };
            //Tạo danh sách giao dịch
            var transactionList = new List<Transaction>();
            transactionList.Add(new Transaction()
            {
                description = "Thank you for using us.",
                invoice_number = DateTime.Now.ToString() + user.Id,
                amount = amount,
                item_list = itemList
            });
            // Configure Redirect Urls here with RedirectUrls object
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl,
                return_url = redirectUrl
            };

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
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution() { payer_id = payerId };
            this.payment = new Payment() { id = paymentId };
            return this.payment.Execute(apiContext, paymentExecution);
        }

    }
}