using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookStore.Data;
using BookStore.Models;
using BookStore.Models.ViewModel;

namespace BookStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        [BindProperty]
        public CustomerOrderViewModel CusOrVM { get; set; }
        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Luyện Ngọc Thanh 
        
        public async Task<IActionResult> Index()
        {
            CustomerOrderViewModel cusorVM = new CustomerOrderViewModel()
            {
                Orders = new List<Order>(),
                OrderDetails = new List<OrderDetail>(),
                Books = new List<Book>()
            };            
            var order = _context.Orders.Include(o => o.ApplicationUser).Include(o => o.Discount).Include(o => o.Shipper).Where(u=> u.ApplicationUser.UserName == User.Identity.Name).ToList();
            cusorVM.Orders = order;

            return View(cusorVM);
            // Trả về danh sách hóa đơn của người dùng hiện tại đang đăng nhập
        }
        public async Task<IActionResult> Receive(int id)
        {
            if (id != 0)
            {
                var orderreceive = _context.Orders.Where(u => u.ID == id).Include(o => o.Discount).Include(o => o.Shipper).Include(o => o.ApplicationUser).FirstOrDefault();
                if (orderreceive == null)
                {
                    return RedirectToAction("Index", "Home", new { area = "Customer" });
                }
                orderreceive.State = "Đã nhận hàng";
                _context.Update(orderreceive);
                await _context.SaveChangesAsync();
                var booktoreview = /*(List<Book>)*/(from b in _context.Books
                                                    join a in _context.OrderDetails
                                                    on b.ID equals a.BookID
                                                    where a.OrderID == id && a.Order.State == "Đã nhận hàng"
                                                    select new Book { ID = b.ID, BookName = b.BookName, BookDetail = b.BookDetail, Image = b.Image, BookPrice = b.BookPrice, Author = b.Author, AuthorID = b.AuthorID, Genrers = b.Genrers, Available = b.Available, Publisher = b.Publisher, PublisherID = b.PublisherID, SpecialTag = b.SpecialTag, SpecialTagID = b.SpecialTagID, ReleaseDate = b.ReleaseDate, Amount = a.Amount });
                foreach (var item in booktoreview)
                {
                    Review review = new Review()
                    {
                        BookID = item.ID,
                        OrderID = id,
                        ApplicationUserID = orderreceive.ApplicationUserID,
                        State = "Chưa duyệt",
                        //Like = 0
                    };
                    _context.Reviews.Add(review);
                    //tạo review cho đơn khi đơn đã xác nhận là được nhận từ khách hàng

                }
                await _context.SaveChangesAsync();
            }          
            int orderid = id;
            return RedirectToAction("Index", "Orders", new { id = orderid });
        }
        // GET: Customer/Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.Discount)
                .Include(o => o.Shipper)
                .Include(o => o.Books)
                .FirstOrDefault(m => m.ID == id);
            var book = (from b in _context.Books
                                        join a in _context.OrderDetails
                                        on b.ID equals a.BookID
                                        where a.OrderID == id
                                        select new Book { ID = b.ID, BookName = b.BookName, BookDetail = b.BookDetail,Image = b.Image, BookPrice = b.BookPrice, Author = b.Author, AuthorID = b.AuthorID, Genrers = b.Genrers, Available = b.Available, Publisher = b.Publisher, PublisherID = b.PublisherID, SpecialTag = b.SpecialTag, SpecialTagID = b.SpecialTagID, ReleaseDate = b.ReleaseDate, Amount = a.Amount });
            var ord = (from b in _context.Books
                                       join a in _context.OrderDetails
                                       on b.ID equals a.BookID
                                       where a.OrderID == id
                                       select new BookTotalPriceViewModel { Book = b, OrderDetail = a });
            CustomerOrderViewModel cusorVM = new CustomerOrderViewModel()
            {
                Order = order,
                Books = book.ToList(),
                BookTotalPriceViewModels = ord.ToList()
            };
            if (order == null)
            {
                return NotFound();
            }

            return View(cusorVM); //trả về viewmodel chứa hóa đơn của khách hàng và danh sách những cuốn sách có trong hóa đơn đó
        }
        // GET: Customer/Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.Discount)
                .Include(o => o.Shipper)
                .Include(o => o.Books)
                .FirstOrDefault(m => m.ID == id);
            var book = /*(List<Book>)*/(from b in _context.Books
                                        join a in _context.OrderDetails
                                        on b.ID equals a.BookID
                                        where a.OrderID == id
                                        select new Book { ID = b.ID, BookName = b.BookName, BookDetail = b.BookDetail, Image = b.Image, BookPrice = b.BookPrice, Author = b.Author, AuthorID = b.AuthorID, Genrers = b.Genrers, Available = b.Available, Publisher = b.Publisher, PublisherID = b.PublisherID, SpecialTag = b.SpecialTag, SpecialTagID = b.SpecialTagID, ReleaseDate = b.ReleaseDate, Amount = a.Amount });
            CustomerOrderViewModel cusorVM = new CustomerOrderViewModel()
            {
                Order = order,
                Books = book.ToList(),
            };
            if (order == null)
            {
                return NotFound();
            }
            ViewData["ApplicationUserID"] = new SelectList(_context.ApplicationUsers, "Id", "Id", order.ApplicationUserID);
            ViewData["DiscountID"] = new SelectList(_context.Discounts, "ID", "ID", order.DiscountID);
            ViewData["ShipperID"] = new SelectList(_context.Shippers, "ID", "ID", order.ShipperID);
            return View(cusorVM);
        }

        // POST: Customer/Orders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            if (ModelState.IsValid)
            {
                var order = _context.Orders.Where(u => u.ID == id).FirstOrDefault();
                if (order != null)
                {
                    order.Name = CusOrVM.Order.Name;
                    order.Address = CusOrVM.Order.Address;
                    order.PhoneNumber = CusOrVM.Order.PhoneNumber;
                    order.Date = CusOrVM.Order.Date;
                    order.DiscountID = CusOrVM.Order.DiscountID;
                    order.State = CusOrVM.Order.State;
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ApplicationUserID"] = new SelectList(_context.ApplicationUsers, "Id", "Id", CusOrVM.Order.ApplicationUserID);
            ViewData["DiscountID"] = new SelectList(_context.Discounts, "ID", "ID", CusOrVM.Order.DiscountID);
            ViewData["ShipperID"] = new SelectList(_context.Shippers, "ID", "ID", CusOrVM.Order.ShipperID);
            return View(CusOrVM);
        }
        [HttpPost]
        public async Task<IActionResult> RemoveInsideOrder(int oid,int bid) // xóa sản phẩm ngay trong đơn đặt hàng đã được gửi và chờ xác nhận
        {
          var orderdetail = _context.OrderDetails.Where(u => u.OrderID == oid && u.BookID == bid).FirstOrDefault();
            if (orderdetail != null)
            {
                _context.OrderDetails.Remove(orderdetail);
            }
          await _context.SaveChangesAsync();
            return RedirectToAction("Edit", "Orders", new { id = oid });
        }
        // GET: Customer/Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.Discount)
                .Include(o => o.Shipper)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Customer/Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.ID == id);
        }
    }
}
