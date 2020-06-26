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
using System.Text;
using Microsoft.AspNetCore.Authorization;
using BookStore.Utility;

namespace BookStore.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.SuperAdminEndUser + "," + SD.ManagerEndUser)]
    [Area("Admin")]
    public class BookGenrersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private int PageSize = 10;

        public BookGenrersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/BookGenrers
        public async Task<IActionResult> Index(int productPage = 1)
        {
            BookGenrersViewModel BookGenrersVM = new BookGenrersViewModel()
            {
                BookGenrers = new List<Models.BookGenrer>()
            };

            StringBuilder param = new StringBuilder();

            param.Append("/Admin/BookGenrers?productPage=:");
            BookGenrersVM.BookGenrers = _context.BooksGenrers.ToList();

            var count = BookGenrersVM.BookGenrers.Count;
            BookGenrersVM.BookGenrers = BookGenrersVM.BookGenrers.Skip((productPage - 1) * PageSize).Take(PageSize).ToList();
            BookGenrersVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItems = count,
                urlParam = param.ToString()
            };

            return View(BookGenrersVM);
        }

        // GET: Admin/BookGenrers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookGenrer = await _context.BooksGenrers
                .FirstOrDefaultAsync(m => m.ID == id);
            if (bookGenrer == null)
            {
                return NotFound();
            }

            return View(bookGenrer);
        }

        // GET: Admin/BookGenrers/Create
        public IActionResult Create()
        {
            BookGenrer bookGenrer = new BookGenrer();
            bookGenrer.PermissionError = null;
            bookGenrer.TriggerError = null;
            return View(bookGenrer);
        }

        // POST: Admin/BookGenrers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,Alias")] BookGenrer bookGenrer)
        {
            try
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
                            _context1.Add(bookGenrer);
                            await _context1.SaveChangesAsync();
                            return RedirectToAction(nameof(Index));
                        }
                        return View(bookGenrer);

                    }
                }
                else
                {

                    if (ModelState.IsValid)
                    {
                        _context.Add(bookGenrer);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    return View(bookGenrer);

                }
            }
            catch (Exception e)
            {

                bookGenrer.PermissionError = e.InnerException.Message;
                if (bookGenrer.PermissionError.Contains("permission"))
                {
                    bookGenrer.TriggerError = null;
                }
                else
                {
                    bookGenrer.PermissionError = null;
                    bookGenrer.TriggerError = e.InnerException.Message.Replace("\r\nThe transaction ended in the trigger. The batch has been aborted.", ".");
                }
                return View(bookGenrer);
            }

        }

        // GET: Admin/BookGenrers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookGenrer = await _context.BooksGenrers.FindAsync(id);
            if (bookGenrer == null)
            {
                return NotFound();
            }

            bookGenrer.PermissionError = null;
            bookGenrer.TriggerError = null;
            return View(bookGenrer);
        }

        // POST: Admin/BookGenrers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Alias")] BookGenrer bookGenrer)
        {
            try
            {
                var user = _context.ApplicationUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();
                var _optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                _optionsBuilder.UseSqlServer("Server=DESKTOP-SNH90JC;Database=WebsiteBanSach;User Id=" + '"' + user.Email + '"' + ";Password=" + user.Pass + ";");
                if (User.IsInRole("Manager"))
                {
                    using (ApplicationDbContext _context1 = new ApplicationDbContext(_optionsBuilder.Options))
                    {

                        if (id != bookGenrer.ID)
                        {
                            return NotFound();
                        }

                        if (ModelState.IsValid)
                        {
                            try
                            {
                                _context1.Update(bookGenrer);
                                await _context1.SaveChangesAsync();
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                if (!BookGenrerExists(bookGenrer.ID))
                                {
                                    return NotFound();
                                }
                                else
                                {
                                    throw;
                                }
                            }
                            return RedirectToAction(nameof(Index));
                        }
                        return View(bookGenrer);

                    }
                }
                else
                {

                    if (id != bookGenrer.ID)
                    {
                        return NotFound();
                    }

                    if (ModelState.IsValid)
                    {
                        try
                        {
                            _context.Update(bookGenrer);
                            await _context.SaveChangesAsync();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!BookGenrerExists(bookGenrer.ID))
                            {
                                return NotFound();
                            }
                            else
                            {
                                throw;
                            }
                        }
                        return RedirectToAction(nameof(Index));
                    }
                    return View(bookGenrer);

                }
            }
            catch (Exception e)
            {

                bookGenrer.PermissionError = e.InnerException.Message;
                if (bookGenrer.PermissionError.Contains("permission"))
                {
                    bookGenrer.TriggerError = null;
                }
                else
                {
                    bookGenrer.PermissionError = null;
                    bookGenrer.TriggerError = e.InnerException.Message.Replace("\r\nThe transaction ended in the trigger. The batch has been aborted.", ".");
                }
                return View(bookGenrer);
            }

        }

        // GET: Admin/BookGenrers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookGenrer = await _context.BooksGenrers
                .FirstOrDefaultAsync(m => m.ID == id);
            bookGenrer.TriggerError = null;
            bookGenrer.PermissionError = null;
            if (bookGenrer == null)
            {
                return NotFound();
            }

            return View(bookGenrer);
        }

        // POST: Admin/BookGenrers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bookGenrer = await _context.BooksGenrers.FindAsync(id);
            try
            {
                var user = _context.ApplicationUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();
                var _optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                _context.BooksGenrers.Remove(bookGenrer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {

                bookGenrer.PermissionError = e.Message;
                return View(bookGenrer);
            }

        }

        private bool BookGenrerExists(int id)
        {
            return _context.BooksGenrers.Any(e => e.ID == id);
        }
    }
}
