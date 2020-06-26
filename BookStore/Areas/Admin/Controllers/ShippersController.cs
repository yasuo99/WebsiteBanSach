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
    public class ShippersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private int PageSize = 10;

        public ShippersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Shippers
        public async Task<IActionResult> Index(int productPage = 1)
        {
            ShippersViewModel ShippersVM = new ShippersViewModel()
            {
                Shippers = new List<Models.Shipper>()
            };

            StringBuilder param = new StringBuilder();

            param.Append("/Admin/Shippers?productPage=:");
            ShippersVM.Shippers = _context.Shippers.ToList();

            var count = ShippersVM.Shippers.Count;
            ShippersVM.Shippers = ShippersVM.Shippers.Skip((productPage - 1) * PageSize).Take(PageSize).ToList();
            ShippersVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItems = count,
                urlParam = param.ToString()
            };

            return View(ShippersVM);
        }

        // GET: Admin/Shippers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shipper = await _context.Shippers
                .FirstOrDefaultAsync(m => m.ID == id);
            if (shipper == null)
            {
                return NotFound();
            }

            return View(shipper);
        }

        // GET: Admin/Shippers/Create
        public IActionResult Create()
        {
            Shipper shipper = new Shipper();
            shipper.TriggerError = null;
                shipper.PermissionError = null;

            return View(shipper);
        }

        // POST: Admin/Shippers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,Phone")] Shipper shipper)
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
                            _context1.Add(shipper);
                            await _context1.SaveChangesAsync();
                            return RedirectToAction(nameof(Index));
                        }
                        return View(shipper);

                    }
                }
                else
                {

                    if (ModelState.IsValid)
                    {
                        _context.Add(shipper);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    return View(shipper);

                }
            }
            catch (Exception e)
            {

                shipper.PermissionError = e.InnerException.Message;
                if (shipper.PermissionError.Contains("permission"))
                {
                    shipper.TriggerError = null;
                }
                else
                {
                    shipper.PermissionError = null;
                    shipper.TriggerError = e.InnerException.Message.Replace("\r\nThe transaction ended in the trigger. The batch has been aborted.", ".");
                }
                return View(shipper);
            }
            
        }

        // GET: Admin/Shippers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shipper = await _context.Shippers.FindAsync(id);
            
            if (shipper == null)
            {
                return NotFound();
            }
            shipper.PermissionError = null;
            shipper.TriggerError = null;
            return View(shipper);
        }

        // POST: Admin/Shippers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Phone")] Shipper shipper)
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

                            if (id != shipper.ID)
                            {
                                return NotFound();
                            }

                            if (ModelState.IsValid)
                            {
                                try
                                {
                                    _context1.Update(shipper);
                                    await _context1.SaveChangesAsync();
                                }
                                catch (DbUpdateConcurrencyException)
                                {
                                    if (!ShipperExists(shipper.ID))
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
                            return View(shipper);



                    }
                }
                else
                {

                        if (id != shipper.ID)
                        {
                            return NotFound();
                        }

                        if (ModelState.IsValid)
                        {
                            try
                            {
                                _context.Update(shipper);
                                await _context.SaveChangesAsync();
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                if (!ShipperExists(shipper.ID))
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
                        return View(shipper);


                }
            }
            catch (Exception e)
            {

                shipper.PermissionError = e.InnerException.Message;
                if (shipper.PermissionError.Contains("permission"))
                {
                    shipper.TriggerError = null;
                }
                else
                {
                    shipper.PermissionError = null;
                    shipper.TriggerError = e.InnerException.Message.Replace("\r\nThe transaction ended in the trigger. The batch has been aborted.", ".");
                }
                return View(shipper);
            }
            
        }

        // GET: Admin/Shippers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shipper = await _context.Shippers
                .FirstOrDefaultAsync(m => m.ID == id);
            shipper.PermissionError = null;
            shipper.TriggerError = null;
            if (shipper == null)
            {
                return NotFound();
            }

            return View(shipper);
        }

        // POST: Admin/Shippers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var shipper = await _context.Shippers.FindAsync(id);
            try
            {
                var user = _context.ApplicationUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();
                var _optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                _optionsBuilder.UseSqlServer("Server=DESKTOP-SNH90JC;Database=WebsiteBanSach;User Id=" + '"' + user.Email + '"' + ";Password=" + user.Pass + ";");
                if (User.IsInRole("Manager"))
                {
                    using (ApplicationDbContext _context1 = new ApplicationDbContext(_optionsBuilder.Options))
                    {
                        _context1.Shippers.Remove(shipper);
                        await _context1.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
                else
                {

                    _context.Shippers.Remove(shipper);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception e)
            {

                shipper.PermissionError = e.Message;
                return View(shipper);
            }
        }

        private bool ShipperExists(int id)
        {
            return _context.Shippers.Any(e => e.ID == id);
        }
    }
}
