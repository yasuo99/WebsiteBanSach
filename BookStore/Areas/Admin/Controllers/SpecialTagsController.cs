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
    public class SpecialTagsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private int PageSize = 10;

        public SpecialTagsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/SpecialTags
        public async Task<IActionResult> Index(int productPage = 1)
        {
            SpecialTagsViewModel SpecialTagsVM = new SpecialTagsViewModel()
            {
                SpecialTags = new List<Models.SpecialTag>()
            };

            StringBuilder param = new StringBuilder();

            param.Append("/Admin/SpecialTags?productPage=:");
            SpecialTagsVM.SpecialTags = _context.SpecialTags.ToList();

            var count = SpecialTagsVM.SpecialTags.Count;
            SpecialTagsVM.SpecialTags = SpecialTagsVM.SpecialTags.Skip((productPage - 1) * PageSize).Take(PageSize).ToList();
            SpecialTagsVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItems = count,
                urlParam = param.ToString()
            };

            return View(SpecialTagsVM);
        }

        // GET: Admin/SpecialTags/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var specialTag = await _context.SpecialTags
                .FirstOrDefaultAsync(m => m.ID == id);
            if (specialTag == null)
            {
                return NotFound();
            }

            return View(specialTag);
        }

        // GET: Admin/SpecialTags/Create
        public IActionResult Create()
        {
            SpecialTag special = new SpecialTag();
            special.PermissionError = null;
            special.TriggerError = null;
            special.PermissionError = null;
            return View(special);
        }

        // POST: Admin/SpecialTags/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,Alias")] SpecialTag specialTag)
        {
            specialTag.TriggerError = null;
            specialTag.PermissionError = null;
            try
            {

                if (ModelState.IsValid)
                {
                    _context.Add(specialTag);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View(specialTag);
            }
            catch (Exception e)
            {
                return View(specialTag);
            }


        }

        // GET: Admin/SpecialTags/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var specialTag = await _context.SpecialTags.FindAsync(id);

            if (specialTag == null)
            {
                return NotFound();
            }
            specialTag.PermissionError = null;
            specialTag.TriggerError = null;

            return View(specialTag);
        }

        // POST: Admin/SpecialTags/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Alias")] SpecialTag specialTag)
        {
            specialTag.TriggerError = null;
            specialTag.PermissionError = null;
            try
            {

                if (id != specialTag.ID)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(specialTag);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!SpecialTagExists(specialTag.ID))
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
                return View(specialTag);
            }
            catch (Exception e)
            {

                return View(specialTag);
            }

        }

        // GET: Admin/SpecialTags/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var specialTag = await _context.SpecialTags
                .FirstOrDefaultAsync(m => m.ID == id);
            specialTag.PermissionError = null;
            if (specialTag == null)
            {
                return NotFound();
            }

            return View(specialTag);
        }

        // POST: Admin/SpecialTags/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var specialTag = await _context.SpecialTags.FindAsync(id);
            try
            {
                _context.SpecialTags.Remove(specialTag);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {

                specialTag.PermissionError = e.Message;
                return View(specialTag);
            }

        }

        private bool SpecialTagExists(int id)
        {
            return _context.SpecialTags.Any(e => e.ID == id);
        }
    }
}
