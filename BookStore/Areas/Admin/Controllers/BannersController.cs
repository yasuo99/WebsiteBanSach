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
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace BookStore.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.SuperAdminEndUser + "," + SD.ManagerEndUser)]
    [Area("Admin")]
    public class BannersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public BannersController(ApplicationDbContext context,IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: Admin/Banners
        public async Task<IActionResult> Index()
        {
            return View(await _context.Banners.ToListAsync());
        }


        // GET: Admin/Banners/Create
        public IActionResult Create()
        {
            Banner banner = new Banner();
            banner.PermissionError = null;
            return View(banner);
        }

        // POST: Admin/Banners/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePOST([Bind("ID,Image")] Banner banner)
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

                            _context1.Add(banner);
                            await _context1.SaveChangesAsync();

                            //Image being saved
                            string webRootPath = _hostingEnvironment.WebRootPath;
                            var files = HttpContext.Request.Form.Files;

                            var bannerFromDb = _context1.Banners.Find(banner.ID);
                            if (files.Count != 0)
                            {
                                //Image has been uploaded
                                var uploads = Path.Combine(webRootPath, SD.BannerImageFolder);
                                var extenstion = Path.GetExtension(files[0].FileName);

                                using (var filestream = new FileStream(Path.Combine(uploads, banner.ID + extenstion), FileMode.Create))
                                {
                                    files[0].CopyTo(filestream);
                                }
                                bannerFromDb.Image = @"\" + SD.BannerImageFolder + @"\" + banner.ID + extenstion;
                            }
                            else
                            {
                                var uploads = Path.Combine(webRootPath, SD.BannerImageFolder + @"\" + SD.DefaultBannerImage);
                                System.IO.File.Copy(uploads, webRootPath + @"\" + SD.BannerImageFolder + @"\" + banner.ID + ".png");
                                bannerFromDb.Image = @"\" + SD.BannerImageFolder + @"\" + banner.ID + ".png";
                            }
                            await _context1.SaveChangesAsync();
                            return RedirectToAction(nameof(Index));
                        }
                        return View(banner);
                    }
                }
                else
                {
                    if (ModelState.IsValid)
                    {

                        _context.Add(banner);
                        await _context.SaveChangesAsync();

                        //Image being saved
                        string webRootPath = _hostingEnvironment.WebRootPath;
                        var files = HttpContext.Request.Form.Files;

                        var bannerFromDb = _context.Banners.Find(banner.ID);
                        if (files.Count != 0)
                        {
                            //Image has been uploaded
                            var uploads = Path.Combine(webRootPath, SD.BannerImageFolder);
                            var extenstion = Path.GetExtension(files[0].FileName);

                            using (var filestream = new FileStream(Path.Combine(uploads, banner.ID + extenstion), FileMode.Create))
                            {
                                files[0].CopyTo(filestream);
                            }
                            bannerFromDb.Image = @"\" + SD.BannerImageFolder + @"\" + banner.ID + extenstion;
                        }
                        else
                        {
                            var uploads = Path.Combine(webRootPath, SD.BannerImageFolder + @"\" + SD.DefaultBannerImage);
                            System.IO.File.Copy(uploads, webRootPath + @"\" + SD.BannerImageFolder + @"\" + banner.ID + ".png");
                            bannerFromDb.Image = @"\" + SD.BannerImageFolder + @"\" + banner.ID + ".png";
                        }
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    return View(banner);
                }
            }
            catch (Exception e)
            {
                banner.PermissionError = e.Message;
                return View(banner);
            }           
        }
        // GET: Admin/Banners/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var banner = await _context.Banners.FindAsync(id);
            banner.PermissionError = null;
            if (banner == null)
            {
                return NotFound();
            }
            return View(banner);
        }

        // POST: Admin/Banners/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Image")] Banner banner)
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
                            string webRootPath = _hostingEnvironment.WebRootPath;
                            var files = HttpContext.Request.Form.Files;

                            var bannerFromDb = _context1.Authors.Where(m => m.ID == id).FirstOrDefault();

                            if (files.Count > 0 && files[0] != null)
                            {
                                //if user uploads a new image
                                var uploads = Path.Combine(webRootPath, SD.BannerImageFolder);
                                var extension_new = Path.GetExtension(files[0].FileName);
                                var extension_old = Path.GetExtension(bannerFromDb.Image);

                                if (System.IO.File.Exists(Path.Combine(uploads, banner.ID + extension_old)))
                                {
                                    System.IO.File.Delete(Path.Combine(uploads, banner.ID + extension_old));
                                }
                                using (var filestream = new FileStream(Path.Combine(uploads, banner.ID + extension_new), FileMode.Create))
                                {
                                    files[0].CopyTo(filestream);
                                }
                                banner.Image = @"\" + SD.BannerImageFolder + @"\" + banner.ID + extension_new;
                            }

                            if (banner.Image != null)
                            {
                                bannerFromDb.Image = banner.Image;
                            }
                            await _context1.SaveChangesAsync();

                            return RedirectToAction(nameof(Index));
                        }

                        return View(banner);
                    }
                }
                else
                {
                    if (ModelState.IsValid)
                    {
                        string webRootPath = _hostingEnvironment.WebRootPath;
                        var files = HttpContext.Request.Form.Files;

                        var bannerFromDb = _context.Authors.Where(m => m.ID == id).FirstOrDefault();

                        if (files.Count > 0 && files[0] != null)
                        {
                            //if user uploads a new image
                            var uploads = Path.Combine(webRootPath, SD.BannerImageFolder);
                            var extension_new = Path.GetExtension(files[0].FileName);
                            var extension_old = Path.GetExtension(bannerFromDb.Image);

                            if (System.IO.File.Exists(Path.Combine(uploads, banner.ID + extension_old)))
                            {
                                System.IO.File.Delete(Path.Combine(uploads, banner.ID + extension_old));
                            }
                            using (var filestream = new FileStream(Path.Combine(uploads, banner.ID + extension_new), FileMode.Create))
                            {
                                files[0].CopyTo(filestream);
                            }
                            banner.Image = @"\" + SD.BannerImageFolder + @"\" + banner.ID + extension_new;
                        }

                        if (banner.Image != null)
                        {
                            bannerFromDb.Image = banner.Image;
                        }
                        await _context.SaveChangesAsync();

                        return RedirectToAction(nameof(Index));
                    }

                    return View(banner);
                }
            }
            catch (Exception e)
            {
                banner.PermissionError = e.Message;
                return View(banner);
            }          
        }
        // GET: Admin/Banners/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var banner = await _context.Banners
                .FirstOrDefaultAsync(m => m.ID == id);
            banner.PermissionError = null;
            if (banner == null)
            {
                return NotFound();
            }

            return View(banner);
        }

        // POST: Admin/Banners/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            try
            {
                var user = _context.ApplicationUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();
                var _optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                _optionsBuilder.UseSqlServer("Server=DESKTOP-SNH90JC;Database=WebsiteBanSach;User Id=" + '"' + user.Email + '"' + ";Password=" + user.Pass + ";");
                if (User.IsInRole("Manager"))
                {
                    using (ApplicationDbContext _context1 = new ApplicationDbContext(_optionsBuilder.Options))
                    {                      
                        _context1.Banners.Remove(banner);
                        await _context1.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
                else
                {
                    _context.Banners.Remove(banner);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception e)
            {
                banner.PermissionError = e.Message;
                return View(banner);
            }            
        }

        private bool BannerExists(int id)
        {
            return _context.Banners.Any(e => e.ID == id);
        }
    }
}
