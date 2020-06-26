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
using Microsoft.AspNetCore.Routing;

namespace BookStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        [BindProperty]
        public CustomerReviewViewModel CusReVM { get; set; }
        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Customer/Reviews
        public async Task<IActionResult> Index() //trả về những review được tạo bởi người dùng đang đăng nhập
        {                   
            var user = await _context.ApplicationUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefaultAsync();
            var reviews = /*(List<Book>)*/_context.Reviews.Where(u => u.ApplicationUserID == user.Id).Include(u => u.Book).Include(u=>u.Order).Distinct().ToList();
            return View(reviews);
        }
        public async Task<IActionResult> CreateReviewForOrder(int id) //Trả về những đánh giá cho đơn hàng có id là id
        {
            var applicationDbContext = _context.Reviews.Where(u => u.OrderID == id).Include(r => r.ApplicationUser).Include(r => r.Book).FirstOrDefault();
            var user = _context.ApplicationUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();          
            var reviews = /*(List<Book>)*/_context.Reviews.Where(u => u.ApplicationUserID == user.Id && u.OrderID == id).Include(u => u.Book).ToList();
            CusReVM = new CustomerReviewViewModel()
            {
                ApplicationUser = user,
                Reviews = reviews
            };
            return View(CusReVM);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = _context.Reviews.Where(u => u.ID == id).Include(u => u.Book).Include(u => u.ApplicationUser).Include(u => u.Order).FirstOrDefault();
            if (review == null)
            {
                return NotFound();
            }
            CusReVM = new CustomerReviewViewModel()
            {
                Review = review,
            };
            return View(CusReVM);
        }

        // POST: Customer/Reviews/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            if (ModelState.IsValid)
            {
                var review = _context.Reviews.Where(u => u.ID == id).FirstOrDefault();
                if (review != null)
                {
                    review.Star = CusReVM.Review.Star;
                    review.CustomerReview = CusReVM.Review.CustomerReview;
                    review.Date = DateTime.Now;
                    _context.Update(review);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Reviews", new { id = CusReVM.Review.OrderID });
                }
            }
            return View(CusReVM);
        }
    }
}
