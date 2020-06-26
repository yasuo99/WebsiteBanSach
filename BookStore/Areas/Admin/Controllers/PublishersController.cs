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
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Routing;

namespace BookStore.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.SuperAdminEndUser + "," + SD.ManagerEndUser)]
    [Area("Admin")]
    public class PublishersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private int PageSize = 10;

        public PublishersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Publishers
        public async Task<IActionResult> Index(int productPage = 1)
        {
            PublishersViewModel PublishersVM = new PublishersViewModel()
            {
                Publishers = new List<Models.Publisher>()
            };

            StringBuilder param = new StringBuilder();

            param.Append("/Admin/Publishers?productPage=:");
            PublishersVM.Publishers = _context.Publishers.ToList();

            var count = PublishersVM.Publishers.Count;
            PublishersVM.Publishers = PublishersVM.Publishers.Skip((productPage - 1) * PageSize).Take(PageSize).ToList();
            PublishersVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItems = count,
                urlParam = param.ToString()
            };

            return View(PublishersVM);
        }

        // GET: Admin/Publishers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publisher = await _context.Publishers
                .FirstOrDefaultAsync(m => m.ID == id);
            if (publisher == null)
            {
                return NotFound();
            }

            return View(publisher);
        }

        // GET: Admin/Publishers/Create
        public IActionResult Create()
        {
            Publisher publisher = new Publisher();
            publisher.TriggerError = null;
            publisher.PermissionError = null;

            return View(publisher);
        }

        // POST: Admin/Publishers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name")] Publisher publisher)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    _context.Add(publisher);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View(publisher);
            }
            catch (Exception e)
            {
                publisher.PermissionError = e.InnerException.Message;
                if (publisher.PermissionError.Contains("permission"))
                {
                    publisher.TriggerError = null;
                }
                else
                {
                    publisher.PermissionError = null;
                    publisher.TriggerError = e.InnerException.Message.Replace("\r\nThe transaction ended in the trigger. The batch has been aborted.", ".");
                }
                return View(publisher);
            }
        }

        // GET: Admin/Publishers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publisher = await _context.Publishers.FindAsync(id);
            if (publisher == null)
            {
                return NotFound();
            }
            publisher.TriggerError = null;
            publisher.PermissionError = null;

            return View(publisher);
        }

        // POST: Admin/Publishers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name")] Publisher publisher)
        {
            publisher.TriggerError = null;
            publisher.PermissionError = null;
            try
            {

                if (id != publisher.ID)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(publisher);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!PublisherExists(publisher.ID))
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
                return View(publisher);
            }
            catch (Exception e)
            {

                publisher.PermissionError = e.InnerException.Message;
                if (publisher.PermissionError.Contains("permission"))
                {
                    publisher.TriggerError = null;
                }
                else
                {
                    publisher.PermissionError = null;
                    publisher.TriggerError = e.InnerException.Message.Replace("\r\nThe transaction ended in the trigger. The batch has been aborted.", ".");
                }
                return View(publisher);
            }
        }

        // GET: Admin/Publishers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publisher = await _context.Publishers
                .FirstOrDefaultAsync(m => m.ID == id);
            publisher.PermissionError = null;
            if (publisher == null)
            {
                return NotFound();
            }

            return View(publisher);
        }

        // POST: Admin/Publishers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var publisher = await _context.Publishers.FindAsync(id);
            try
            {
                var user = _context.ApplicationUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();
                var _optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                _optionsBuilder.UseSqlServer("Server=DESKTOP-SNH90JC;Database=WebsiteBanSach;User Id=" + '"' + user.Email + '"' + ";Password=" + user.Pass + ";");
                if (User.IsInRole("Manager"))
                {
                    using (ApplicationDbContext _context1 = new ApplicationDbContext(_optionsBuilder.Options))
                    {

                        _context1.Publishers.Remove(publisher);
                        await _context1.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
                else
                {
                    _context.Publishers.Remove(publisher);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception e)
            {

                publisher.PermissionError = e.Message;
                return View(publisher);
            }

        }

        private bool PublisherExists(int id)
        {
            return _context.Publishers.Any(e => e.ID == id);
        }
    }
}
