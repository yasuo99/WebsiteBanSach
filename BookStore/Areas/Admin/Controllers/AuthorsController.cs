using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookStore.Data;
using BookStore.Models;
using Microsoft.AspNetCore.Hosting;
using BookStore.Models.ViewModel;
using System.IO;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using Microsoft.Data.SqlClient;

namespace BookStore.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.SuperAdminEndUser + "," + SD.ManagerEndUser)]
    [Area("Admin")]
    public class AuthorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private int PageSize = 10;
        [BindProperty]
        public AuthorsViewModel AuthorsVM { get; set; }
        public AuthorsController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment)
        {

            _context = context;
            _hostingEnvironment = hostingEnvironment;
            AuthorsVM = new AuthorsViewModel()
            {
                Author = new Models.Author(),
            };
        }
        //"Server=DESKTOP-SNH90JC;Database=WebsiteBanSach;Trusted_Connection=true;MultipleActiveResultSets=true;User Id=amen;Password=thanh1;"
        // GET: Admin/Authors
        public async Task<IActionResult> Index(int productPage = 1)
        {
            AuthorsViewModel AuthorsVM = new AuthorsViewModel()
            {
                Authors = new List<Models.Author>()
            };

            StringBuilder param = new StringBuilder();

            param.Append("/Admin/Authors?productPage=:");
            AuthorsVM.Authors = _context.Authors.ToList();

            var count = AuthorsVM.Authors.Count;
            AuthorsVM.Authors = AuthorsVM.Authors.Skip((productPage - 1) * PageSize).Take(PageSize).ToList();
            AuthorsVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItems = count,
                urlParam = param.ToString()
            };
            //string connectionstring = "Server=DESKTOP-9127M85;Database=WebsiteBanSach;User Id=thanh;Password=thanh1;";
            //SqlConnection con = new SqlConnection(connectionstring);
            //if (con.State == System.Data.ConnectionState.Open)
            //{
            //    con.Close();
            //}
            //var user = _context.ApplicationUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();
            //string connection = "Server=DESKTOP-9127M85;Database=WebsiteBanSach;User Id=" + user.Name + ";Password=" + user.Pass + ";";
            //SqlConnection con1 = new SqlConnection(connectionstring);
            //con1.Open();
            return View(AuthorsVM);
        }

        // GET: Admin/Authors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            AuthorsVM.Author = await _context.Authors
                .FirstOrDefaultAsync(m => m.ID == id);
            if (AuthorsVM.Author == null)
            {
                return NotFound();
            }

            return View(AuthorsVM.Author);
        }

        // GET: Admin/Authors/Create
        public IActionResult Create()
        {
            AuthorsViewModel authors = new AuthorsViewModel();
            authors.PermissionError = null;
            authors.TriggerError = null;
            return View(authors);
        }

        // POST: Admin/Authors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePOST()
        {
            try
            {
                var user = _context.ApplicationUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();

                if (!ModelState.IsValid)
                {
                    return View(AuthorsVM);
                }
                _context.Authors.Add(AuthorsVM.Author);
                await _context.SaveChangesAsync();

                //Image being saved
                string webRootPath = _hostingEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;

                var authorFromDb = _context.Authors.Find(AuthorsVM.Author.ID);
                if (files.Count != 0)
                {
                    //Image has been uploaded
                    var uploads = Path.Combine(webRootPath, SD.AuthorImageFolder);
                    var extenstion = Path.GetExtension(files[0].FileName);

                    using (var filestream = new FileStream(Path.Combine(uploads, AuthorsVM.Author.ID + extenstion), FileMode.Create))
                    {
                        files[0].CopyTo(filestream);
                    }
                    authorFromDb.Image = @"\" + SD.AuthorImageFolder + @"\" + AuthorsVM.Author.ID + extenstion;
                }
                else
                {
                    var uploads = Path.Combine(webRootPath, SD.AuthorImageFolder + @"\" + SD.DefaultAuthorImage);
                    System.IO.File.Copy(uploads, webRootPath + @"\" + SD.AuthorImageFolder + @"\" + AuthorsVM.Author.ID + ".png");
                    authorFromDb.Image = @"\" + SD.AuthorImageFolder + @"\" + AuthorsVM.Author.ID + ".png";
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {

                AuthorsVM.PermissionError = e.InnerException.Message;
                if (AuthorsVM.PermissionError.Contains("permission"))
                {
                    AuthorsVM.TriggerError = null;
                }
                else
                {
                    AuthorsVM.PermissionError = null;
                    AuthorsVM.TriggerError = e.InnerException.Message.Replace("\r\nThe transaction ended in the trigger. The batch has been aborted.", ".");
                }
                return View(AuthorsVM);
            }
        }

        // GET: Admin/Authors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (AuthorsVM.Author == null)
            {
                return NotFound();
            }
            AuthorsVM.TriggerError = null;
            AuthorsVM.PermissionError = null;
            AuthorsVM.Author = await _context.Authors.Include(m => m.Books).SingleOrDefaultAsync(m => m.ID == id);
            return View(AuthorsVM);
        }

        // POST: Admin/Authors/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,FirstName,SecondName,Image,CompanyName")] Author author)
        {
            try
            {
                var user = _context.ApplicationUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();

                if (ModelState.IsValid)
                {
                    string webRootPath = _hostingEnvironment.WebRootPath;
                    var files = HttpContext.Request.Form.Files;

                    var authorFromDb = _context.Authors.Where(m => m.ID == AuthorsVM.Author.ID).FirstOrDefault();

                    if (files.Count > 0 && files[0] != null)
                    {
                        //if user uploads a new image
                        var uploads = Path.Combine(webRootPath, SD.AuthorImageFolder);
                        var extension_new = Path.GetExtension(files[0].FileName);
                        var extension_old = Path.GetExtension(authorFromDb.Image);

                        if (System.IO.File.Exists(Path.Combine(uploads, AuthorsVM.Author.ID + extension_old)))
                        {
                            System.IO.File.Delete(Path.Combine(uploads, AuthorsVM.Author.ID + extension_old));
                        }
                        using (var filestream = new FileStream(Path.Combine(uploads, AuthorsVM.Author.ID + extension_new), FileMode.Create))
                        {
                            files[0].CopyTo(filestream);
                        }
                        AuthorsVM.Author.Image = @"\" + SD.AuthorImageFolder + @"\" + AuthorsVM.Author.ID + extension_new;
                    }

                    if (AuthorsVM.Author.Image != null)
                    {
                        authorFromDb.Image = AuthorsVM.Author.Image;
                    }

                    authorFromDb.FirstName = AuthorsVM.Author.FirstName;
                    authorFromDb.SecondName = AuthorsVM.Author.SecondName;
                    authorFromDb.Image = AuthorsVM.Author.Image;
                    authorFromDb.CompanyName = AuthorsVM.Author.CompanyName;
                    authorFromDb.Books = AuthorsVM.Author.Books;
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }

                return View(AuthorsVM);
            }
            catch (Exception e)
            {

                AuthorsVM.PermissionError = e.InnerException.Message;
                if (AuthorsVM.PermissionError.Contains("permission"))
                {
                    AuthorsVM.TriggerError = null;
                }
                else
                {
                    AuthorsVM.PermissionError = null;
                    AuthorsVM.TriggerError = e.InnerException.Message.Replace("\r\nThe transaction ended in the trigger. The batch has been aborted.", ".");
                }
                return View(AuthorsVM);
            }


        }

        // GET: Admin/Authors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            AuthorsVM.Author = await _context.Authors
                .FirstOrDefaultAsync(m => m.ID == id);
            if (AuthorsVM.Author == null)
            {
                return NotFound();
            }

            return View(AuthorsVM);
        }

        // POST: Admin/Authors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {

                string webRootPath = _hostingEnvironment.WebRootPath;
                Author Author = await _context.Authors.FindAsync(id);

                if (Author == null)
                {
                    return NotFound();
                }
                else
                {
                    var uploads = Path.Combine(webRootPath, SD.AuthorImageFolder);
                    var extension = Path.GetExtension(Author.Image);

                    if (System.IO.File.Exists(Path.Combine(uploads, Author.ID + extension)))
                    {
                        System.IO.File.Delete(Path.Combine(uploads, Author.ID + extension));
                    }
                    _context.Authors.Remove(Author);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception e)
            {
                AuthorsVM.PermissionError = e.Message;
                return View(AuthorsVM);
            }
        }

        private bool AuthorExists(int id)
        {
            return _context.Authors.Any(e => e.ID == id);
        }
    }
}

