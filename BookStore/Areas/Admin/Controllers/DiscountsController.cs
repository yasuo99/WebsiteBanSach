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
    public class DiscountsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private int PageSize = 10;

        public DiscountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Discounts
        public async Task<IActionResult> Index(int productPage = 1)
        {
            DiscountsViewModel DiscountsVM = new DiscountsViewModel()
            {
                Discounts = new List<Models.Discount>()
            };

            StringBuilder param = new StringBuilder();

            param.Append("/Admin/Discounts?productPage=:");
            DiscountsVM.Discounts = _context.Discounts.ToList();

            var count = DiscountsVM.Discounts.Count;
            DiscountsVM.Discounts = DiscountsVM.Discounts.Skip((productPage - 1) * PageSize).Take(PageSize).ToList();
            DiscountsVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItems = count,
                urlParam = param.ToString()
            };

            return View(DiscountsVM);
        }

        // GET: Admin/Discounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var discount = await _context.Discounts
                .FirstOrDefaultAsync(m => m.ID == id);
            if (discount == null)
            {
                return NotFound();
            }

            return View(discount);
        }

        // GET: Admin/Discounts/Create
        public IActionResult Create()
        {
            Discount discount = new Discount();
            discount.TriggerError = null;
            discount.PermissionError = null;

            return View(discount);
        }

        // POST: Admin/Discounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Code,Value,Available")] Discount discount)
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
                            _context1.Add(discount);
                            await _context1.SaveChangesAsync();
                            return RedirectToAction(nameof(Index));
                        }
                        return View(discount);

                    }
                }
                else
                {

                    if (ModelState.IsValid)
                    {
                        _context.Add(discount);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    return View(discount);

                }
            }
            catch (Exception e)
            {
                discount.PermissionError = e.InnerException.Message;
                if (discount.PermissionError.Contains("permission"))
                {
                    discount.TriggerError = null;
                }
                else
                {
                    discount.PermissionError = null;
                    discount.TriggerError = e.InnerException.Message.Replace("\r\nThe transaction ended in the trigger. The batch has been aborted.", ".");
                }
                return View(discount);
            }
        }

        // GET: Admin/Discounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
            {
                return NotFound();
            }
            discount.TriggerError = null;
            discount.PermissionError = null;
            return View(discount);
        }

        // POST: Admin/Discounts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Code,Value,Available")] Discount discount)
        {
            try
            {

                if (id != discount.ID)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(discount);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!DiscountExists(discount.ID))
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
                return View(discount);
            }
            catch (Exception e)
            {
                discount.PermissionError = e.InnerException.Message;
                if (discount.PermissionError.Contains("permission"))
                {
                    discount.TriggerError = null;
                }
                else
                {
                    discount.PermissionError = null;
                    discount.TriggerError = e.InnerException.Message.Replace("\r\nThe transaction ended in the trigger. The batch has been aborted.", ".");
                }
                return View(discount);
            }
        }

        // GET: Admin/Discounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var discount = await _context.Discounts
                .FirstOrDefaultAsync(m => m.ID == id);
            discount.PermissionError = null;
            if (discount == null)
            {
                return NotFound();
            }

            return View(discount);
        }

        // POST: Admin/Discounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            try
            {
                _context.Discounts.Remove(discount);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                discount.PermissionError = e.Message;
                return View(discount);
            }

        }

        private bool DiscountExists(int id)
        {
            return _context.Discounts.Any(e => e.ID == id);
        }
    }
}
