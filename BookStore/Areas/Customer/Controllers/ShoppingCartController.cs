using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookStore.Data;
using BookStore.Extensions;
using BookStore.Models;
using BookStore.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Areas.Customer.Controllers
{
    [Area("Customer")]   
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _db;

        [BindProperty]
        public ShoppingCartViewModel ShoppingCartVM { get; set; }

        public ShoppingCartController(ApplicationDbContext db)
        {
            _db = db;
            ShoppingCartVM = new ShoppingCartViewModel()
            {
                Books = new List<Models.Book>(),    
            };
        }

        //Get Index Shopping Cart
        public async Task<IActionResult> Index()
        {
            ViewData["DiscountID"] = new SelectList(_db.Discounts, "ID", "Code");
            List<ShoppingSessionViewModel> lstShoppingCart = HttpContext.Session.Get<List<ShoppingSessionViewModel>>("ssShoppingCart");
            double total = 0;
            if (lstShoppingCart != null)
            {
                if (lstShoppingCart.Count > 0)
                {
                    foreach (ShoppingSessionViewModel cartItem in lstShoppingCart)
                    {
                        Book prod = _db.Books.Include(p => p.SpecialTag).Include(p => p.Author).Include(m => m.Publisher).Where(p => p.ID == cartItem.Book.ID).FirstOrDefault();
                        prod.Amount = cartItem.Amount;
                        total += prod.BookPrice*prod.Amount;
                        ShoppingCartVM.Books.Add(prod);
                    }
                    var user = await _db.ApplicationUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefaultAsync();
                    ShoppingCartVM.ApplicationUser = user;
                    ShoppingCartVM.Total = total;
                }
            }
            return View(ShoppingCartVM);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> IndexPost() //
        {
            List<ShoppingSessionViewModel> lstCartItems = HttpContext.Session.Get<List<ShoppingSessionViewModel>>("ssShoppingCart"); 
            ShoppingCartVM.Order.Date = DateTime.Now;
            Order Order= ShoppingCartVM.Order;
            var user = await _db.ApplicationUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefaultAsync();
            int oid = 0;
            var orderwaiting = _db.Orders.Where(u => u.ApplicationUserID == user.Id && u.State == "Chưa xác nhận").FirstOrDefault();
            if (orderwaiting != null)
            {
                foreach (ShoppingSessionViewModel productId in lstCartItems)
                {
                    OrderDetail orderDetail = new OrderDetail()
                    {
                        OrderID = orderwaiting.ID,
                        BookID = productId.Book.ID,
                        Amount = productId.Amount,
                        TotalPrice = productId.Amount * productId.Book.BookPrice
                    };
                    _db.OrderDetails.Add(orderDetail);
                }
                lstCartItems = new List<ShoppingSessionViewModel>();
                HttpContext.Session.Set("ssShoppingCart", lstCartItems);
                await _db.SaveChangesAsync();
                return RedirectToAction("OrderConfirmation", "ShoppingCart", new { ID = orderwaiting.ID });
            }
            else
            {
                ShoppingCartVM.ApplicationUser = user;
                if (ShoppingCartVM.Order.Name != null && ShoppingCartVM.Order.PhoneNumber != null && ShoppingCartVM.Order.Address != null)
                {
                    Order.Address = ShoppingCartVM.Order.Address;
                    Order.Name = ShoppingCartVM.Order.Name;
                    Order.PhoneNumber = ShoppingCartVM.Order.PhoneNumber;
                    Order.ApplicationUserID = ShoppingCartVM.ApplicationUser.Id;
                    Order.Total = 0;
                    Order.DiscountID = ShoppingCartVM.Order.DiscountID;
                }

                Order.State = "Chưa xác nhận";
                _db.Orders.Add(Order);
                _db.SaveChanges();

                int orderID = Order.ID;
                oid = orderID;
                var order = _db.Orders.Where(u => u.ID == orderID).Include(u => u.Discount).FirstOrDefault();

                foreach (ShoppingSessionViewModel productId in lstCartItems)
                {
                    OrderDetail orderDetail = new OrderDetail()
                    {
                        BookID = productId.Book.ID,
                        OrderID = orderID,
                        Amount = productId.Amount,
                        TotalPrice = productId.Amount * productId.Book.BookPrice
                    };
                    _db.OrderDetails.Add(orderDetail);
                    order.Total += orderDetail.TotalPrice;
                }
                order.Total -= (order.Total * order.Discount.Value) / 100;
                _db.Update(order);
                _db.SaveChanges();
                lstCartItems = new List<ShoppingSessionViewModel>();
                HttpContext.Session.Set("ssShoppingCart", lstCartItems);
                return RedirectToAction("OrderConfirmation", "ShoppingCart", new { ID = oid });
            }

        }
        public IActionResult Remove(int id)
        {
            List<ShoppingSessionViewModel> lstCartItems = HttpContext.Session.Get<List<ShoppingSessionViewModel>>("ssShoppingCart");
            if (lstCartItems.Count > 0)
            {
                foreach(var item in lstCartItems)
                {
                    if (item.Book.ID == id)
                    {
                        lstCartItems.Remove(item);
                        break;
                    }
                    
                }
            }
            HttpContext.Session.Set("ssShoppingCart", lstCartItems);

            return RedirectToAction(nameof(Index));
        }
        //Get
        public IActionResult OrderConfirmation(int id)
        {
            ShoppingCartVM.Order = _db.Orders.Where(a => a.ID == id).Include(u => u.Discount).FirstOrDefault();
            var books = from b in _db.Books
                        join a in _db.OrderDetails
                        on b.ID equals a.BookID
                        where a.OrderID == id
                        select new Book { ID = a.ID, BookName = b.BookName, BookDetail = b.BookDetail, Image = b.Image, BookPrice = b.BookPrice, Author = b.Author, AuthorID = b.AuthorID, Genrers = b.Genrers, Available = b.Available, Publisher = b.Publisher, PublisherID = b.PublisherID, SpecialTag = b.SpecialTag, SpecialTagID = b.SpecialTagID, ReleaseDate = b.ReleaseDate, Amount = a.Amount };


            ShoppingCartVM.Books = books.ToList();

            return View(ShoppingCartVM);
        }

    }
}