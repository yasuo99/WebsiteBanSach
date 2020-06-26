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
    public class GenrersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private int PageSize = 10;
        public GenrerViewModel GenrerVM { get; set; }

        public GenrersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Genrers
        public async Task<IActionResult> Index(int productPage = 1)
        {
            GenrerViewModel GenrerVM = new GenrerViewModel()
            {
                Genrers = new List<Models.Genrer>()
            };

            StringBuilder param = new StringBuilder();

            param.Append("/Admin/Genrers?productPage=:");
            GenrerVM.Genrers = _context.Genrers.Include(g => g.Book).Include(g => g.BookGenrer).ToList();

            var count = GenrerVM.Genrers.Count;
            GenrerVM.Genrers = GenrerVM.Genrers.Skip((productPage - 1) * PageSize).Take(PageSize).ToList();
            GenrerVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItems = count,
                urlParam = param.ToString()
            };

            return View(GenrerVM);
        }

        // GET: Admin/Genrers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var genrer = await _context.Genrers
                .Include(g => g.Book)
                .Include(g => g.BookGenrer)
                .FirstOrDefaultAsync(m => m.BookID == id);
            if (genrer == null)
            {
                return NotFound();
            }

            return View(genrer);
        }

        // GET: Admin/Genrers/Create
        public IActionResult Create()
        {
            ViewData["BookID"] = new SelectList(_context.Books, "ID", "BookName");
            ViewData["BookGenrerID"] = new SelectList(_context.BooksGenrers, "ID", "Name");
            return View();
        }

        // POST: Admin/Genrers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,BookID,BookGenrerID")] Genrer genrer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(genrer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BookID"] = new SelectList(_context.Books, "ID", "BookName", genrer.BookID);
            ViewData["BookGenrerID"] = new SelectList(_context.BooksGenrers, "ID", "Name", genrer.BookGenrerID);
            return View(genrer);
        }

        // GET: Admin/Genrers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var genrer = await _context.Genrers.FindAsync(id);
            if (genrer == null)
            {
                return NotFound();
            }
            ViewData["BookID"] = new SelectList(_context.Books, "ID", "BookName", genrer.BookID);
            ViewData["BookGenrerID"] = new SelectList(_context.BooksGenrers, "ID", "Name", genrer.BookGenrerID);
            return View(genrer);
        }

        // POST: Admin/Genrers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,BookID,BookGenrerID")] Genrer genrer)
        {
            if (id != genrer.BookID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(genrer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GenrerExists(genrer.BookID))
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
            ViewData["BookID"] = new SelectList(_context.Books, "ID", "BookName", genrer.BookID);
            ViewData["BookGenrerID"] = new SelectList(_context.BooksGenrers, "ID", "Name", genrer.BookGenrerID);
            return View(genrer);
        }

        // GET: Admin/Genrers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var genrer = await _context.Genrers
                .Include(g => g.Book)
                .Include(g => g.BookGenrer)
                .FirstOrDefaultAsync(m => m.BookID == id);
            if (genrer == null)
            {
                return NotFound();
            }

            return View(genrer);
        }

        // POST: Admin/Genrers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var genrer = await _context.Genrers.FindAsync(id);
            _context.Genrers.Remove(genrer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GenrerExists(int id)
        {
            return _context.Genrers.Any(e => e.BookID == id);
        }
    }
}
