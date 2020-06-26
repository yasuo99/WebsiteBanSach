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
using Microsoft.AspNetCore.Authorization;
using BookStore.Utility;
using System.Text;
using Microsoft.AspNetCore.Routing;

namespace BookStore.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.SuperAdminEndUser + "," + SD.ManagerEndUser)]
    [Area("Admin")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private int PageSize = 10;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }
        [BindProperty]
        public CustomerOrderViewModel cusorVM { get; set; }
        // GET: Admin/Orders
        public async Task<IActionResult> Index(int productPage = 1)
        {
            OrdersViewModel OrdersVM = new OrdersViewModel()
            {
                Orders = new List<Models.Order>()
            };

            StringBuilder param = new StringBuilder();

            param.Append("/Admin/Orders?productPage=:");
            OrdersVM.Orders = _context.Orders.Include(o => o.ApplicationUser).Include(o => o.Discount).Include(o => o.Shipper).ToList();

            var count = OrdersVM.Orders.Count;
            OrdersVM.Orders = OrdersVM.Orders.Skip((productPage - 1) * PageSize).Take(PageSize).ToList();
            OrdersVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItems = count,
                urlParam = param.ToString()
            };

            return View(OrdersVM);
        }

        // GET: Admin/Orders/Details/5
        public async Task<IActionResult> Details(int? id)
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

            return View(cusorVM);
        }

        // GET: Admin/Orders/Create
        public IActionResult Create()
        {
            ViewData["ApplicationUserID"] = new SelectList(_context.ApplicationUsers, "ID", "FirstName");
            ViewData["DiscountID"] = new SelectList(_context.Discounts, "ID", "ID");
            ViewData["ShipperID"] = new SelectList(_context.Shippers, "ID", "ID");
            var user1 = _context.ApplicationUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();
            var hasrole = (from a in _context.ManagerRoleOnDatabases
                           join b in _context.ApplicationUsers
                           on a.ApplicationUserId equals b.Id
                           where a.TablesName == "Orders" && a.Role == "Update & Insert"
                           select a).FirstOrDefault();
            if (hasrole != null)
            {
                cusorVM.PermissionError = null;
            }
            else
            {
                cusorVM.PermissionError = "Khong co quyen";
            }
            return View(cusorVM);
        }

        // POST: Admin/Orders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        // GET: Admin/Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
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
            var user = _context.ApplicationUsers.Where(u => u.Id == order.ApplicationUserID).FirstOrDefault();
            order.ApplicationUser = user;
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
            var user1 = _context.ApplicationUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();
            var hasrole = (from a in _context.ManagerRoleOnDatabases
                           join b in _context.ApplicationUsers
                           on a.ApplicationUserId equals b.Id
                           where a.TablesName == "Orders" && a.Role == "Update & Insert"
                           select a).FirstOrDefault();
            if (hasrole != null || User.IsInRole("Super Admin"))
            {
                cusorVM.PermissionError = null;
            }
            else
            {
                cusorVM.PermissionError = "Khong co quyen";
            }
            ViewData["ApplicationUserID"] = new SelectList(_context.ApplicationUsers, "ID", "FirstName", order.ApplicationUserID);
            ViewData["DiscountID"] = new SelectList(_context.Discounts, "ID", "ID", order.DiscountID);
            ViewData["ShipperID"] = new SelectList(_context.Shippers, "ID", "ID", order.ShipperID);
            return View(cusorVM);
        }

        // POST: Admin/Orders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var user = _context.ApplicationUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();
            var _optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            _optionsBuilder.UseSqlServer("Server=DESKTOP-SNH90JC;Database=WebsiteBanSach;User Id=" + '"' + user.Email + '"' + ";Password=" + user.Pass + ";");
            if (User.IsInRole("Manager"))
            {
                using (ApplicationDbContext _context1 = new ApplicationDbContext(_optionsBuilder.Options))
                {
                    if (ModelState.IsValid)
                    {
                        var order = _context1.Orders.Where(u => u.ID == id).FirstOrDefault();
                        if (order != null)
                        {
                            order.Name = cusorVM.Order.Name;
                            order.Address = cusorVM.Order.Address;
                            order.PhoneNumber = cusorVM.Order.PhoneNumber;
                            order.Date = cusorVM.Order.Date;
                            order.DiscountID = cusorVM.Order.DiscountID;
                            order.State = "Xác nhận";
                        }
                        var books = from b in _context1.Books join c in _context1.OrderDetails on b.ID equals c.BookID where c.OrderID == id select new Book { ID = b.ID, Amount = c.Amount };
                        foreach (var book in books.ToList())
                        {
                            var tempbook = _context1.Books.Where(b => b.ID == book.ID).FirstOrDefault();
                            tempbook.Available -= book.Amount;
                            tempbook.Sold += book.Amount;
                        }
                        await _context1.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    ViewData["ApplicationUserID"] = new SelectList(_context1.ApplicationUsers, "ID", "FirstName", cusorVM.Order.ApplicationUserID);
                    ViewData["DiscountID"] = new SelectList(_context1.Discounts, "ID", "ID", cusorVM.Order.DiscountID);
                    ViewData["ShipperID"] = new SelectList(_context1.Shippers, "ID", "ID", cusorVM.Order.ShipperID);
                    return View(cusorVM);
                }
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var order = _context.Orders.Where(u => u.ID == id).FirstOrDefault();
                    if (order != null)
                    {
                        order.Name = cusorVM.Order.Name;
                        order.Address = cusorVM.Order.Address;
                        order.PhoneNumber = cusorVM.Order.PhoneNumber;
                        order.Date = cusorVM.Order.Date;
                        order.DiscountID = cusorVM.Order.DiscountID;
                        order.State = "Xác nhận";
                    }
                    var books = from b in _context.Books join c in _context.OrderDetails on b.ID equals c.BookID where c.OrderID == id select new Book { ID = b.ID, Amount = c.Amount };
                    foreach (var book in books.ToList())
                    {
                        var tempbook = _context.Books.Where(b => b.ID == book.ID).FirstOrDefault();
                        tempbook.Available -= book.Amount;
                        tempbook.Sold += book.Amount;
                    }
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                ViewData["ApplicationUserID"] = new SelectList(_context.ApplicationUsers, "ID", "FirstName", cusorVM.Order.ApplicationUserID);
                ViewData["DiscountID"] = new SelectList(_context.Discounts, "ID", "ID", cusorVM.Order.DiscountID);
                ViewData["ShipperID"] = new SelectList(_context.Shippers, "ID", "ID", cusorVM.Order.ShipperID);
                return View(cusorVM);
            }
        }

        // GET: Admin/Orders/Delete/5
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
            order.PermissionError = null;
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Admin/Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            try
            {
                var user = _context.ApplicationUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();
                var _optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                _optionsBuilder.UseSqlServer("Server=DESKTOP-SNH90JC;Database=WebsiteBanSach;User Id=" + '"' + user.Email + '"' + ";Password=" + user.Pass + ";");
                if (User.IsInRole("Manager"))
                {
                    using (ApplicationDbContext _context1 = new ApplicationDbContext(_optionsBuilder.Options))
                    {
                        _context1.Orders.Remove(order);
                        await _context1.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
                else
                {
                    _context.Orders.Remove(order);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception e)
            {
                order.PermissionError = e.Message;
                return View(cusorVM);
            }

        }
        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.ID == id);
        }
    }
}
