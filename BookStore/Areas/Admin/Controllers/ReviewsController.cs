using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using BookStore.Utility;
using BookStore.Models.ViewModel;

namespace BookStore.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.SuperAdminEndUser + "," + SD.ManagerEndUser)]
    [Area("Admin")]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        [BindProperty]
        public CustomerReviewViewModel CusReVM { get; set; }
        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
            CusReVM = new CustomerReviewViewModel()
            {
                PermissionError = null,
            };
        }

        // GET: Admin/Reviews
        public async Task<IActionResult> Index(string value, CustomerReviewViewModel temp)
        {
            if (temp == null)
            {
                CusReVM = new CustomerReviewViewModel()
                {
                    Reviews = _context.Reviews.Where(u => u.CustomerReview != null).Include(r => r.Book).Include(r => r.ApplicationUser).ToList()
                };
                if (value == "confirm")
                {
                    CusReVM.Reviews = _context.Reviews.Where(u => u.CustomerReview != null && u.State == "Đã duyệt").Include(r => r.Book).Include(r => r.ApplicationUser).ToList();
                }
                if (value == "unconfirm")
                {
                    CusReVM.Reviews = _context.Reviews.Where(u => u.CustomerReview != null && u.State == "Chưa duyệt").Include(r => r.Book).Include(r => r.ApplicationUser).ToList(); ;
                }
                if (value == "remove")
                {
                    CusReVM.Reviews = _context.Reviews.Where(u => u.CustomerReview != null && u.State == "Không chấp nhận").Include(r => r.Book).Include(r => r.ApplicationUser).ToList(); ;
                }
                return View(CusReVM);
            }
            else
            {
                temp.Reviews = _context.Reviews.Where(u => u.CustomerReview != null).Include(r => r.Book).Include(r => r.ApplicationUser).ToList();
                return View(temp);
            }
        }

        public async Task<IActionResult> Confirm(int id)
        {
            try
            {

                var review = _context.Reviews.Where(u => u.ID == id).Include(u => u.ApplicationUser).Include(u => u.Order).Include(u => u.Book).FirstOrDefault();
                review.State = "Đã duyệt";
                _context.Update(review);
                await _context.SaveChangesAsync();
                var bookhasreviewed = _context.Reviews.Where(u => u.BookID == review.BookID && u.State == "Đã duyệt").ToList();
                double rating = 0;
                foreach (var i in bookhasreviewed)
                {
                    rating += i.Star;
                }
                var book = _context.Books.Where(u => u.ID == review.BookID).FirstOrDefault();
                book.Rate = Math.Round(rating / bookhasreviewed.Count, 1, MidpointRounding.ToEven);
                _context.Update(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                CusReVM.PermissionError = e.Message;
                return RedirectToAction(nameof(Index), CusReVM);
            }
        }
        public async Task<IActionResult> UnConfirm(int id)
        {
            try
            {

                var review = _context.Reviews.Where(u => u.ID == id).Include(u => u.ApplicationUser).Include(u => u.Order).Include(u => u.Book).FirstOrDefault();
                review.State = "Không chấp nhận";
                _context.Update(review);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                CusReVM.PermissionError = e.Message;
                return RedirectToAction(nameof(Index), CusReVM);
            }
        }
        // GET: Admin/Reviews/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews
                .Include(r => r.Book)
                .Include(r => r.ApplicationUser)
                .FirstOrDefaultAsync(m => m.BookID == id);
            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        // POST: Admin/Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            var review = await _context.Reviews.FindAsync(id);
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.BookID == id);
        }
    }
}
