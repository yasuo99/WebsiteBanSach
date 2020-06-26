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
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BookStore.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.SuperAdminEndUser + "," + SD.ManagerEndUser)]
    [Area("Admin")]
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private int PageSize = 10;

        [BindProperty]
        public BooksViewModel BooksVM { get; set; }
        public BooksController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _hostingEnvironment = webHostEnvironment;
            _context = context;
            BooksVM = new BooksViewModel()
            {
                Authors = _context.Authors.ToList(),
                Genrers = _context.Genrers.ToList(),
                Publishers = _context.Publishers.ToList(),
                SpecialTags = _context.SpecialTags.ToList(),
                GenrerSelectedForBookViewModels = new List<GenrerSelectedForBookViewModel>(),
                Book = new Book(),
                BookSellPerMonths = new List<BookSellPerMonth>(),
                PermissionError = null,
                TriggerError = null
            };

        }

        // GET: Admin/Books
        public async Task<IActionResult> Index(int productPage = 1,int sold = 0)
        {
            BooksViewModel BooksVM = new BooksViewModel()
            {
                Authors = new List<Models.Author>()
            };

            StringBuilder param = new StringBuilder();

            param.Append("/Admin/Books?productPage=:");
            BooksVM.Books = _context.Books.ToList();

            var count = BooksVM.Books.Count;
            BooksVM.Books = BooksVM.Books.Skip((productPage - 1) * PageSize).Take(PageSize).ToList();
            BooksVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItems = count,
                urlParam = param.ToString()
            };
            if(sold == 1)
            {
                BooksVM.Books = SaleZero().ToList();
            }
            return View(BooksVM);
        }

        // GET: Admin/Books/Details/5
        public async Task<IActionResult> Details(int id)
        {
            BooksVM.Book = await _context.Books.Include(m => m.SpecialTag).Include(m => m.Author).Include(m => m.Publisher).SingleOrDefaultAsync(m => m.ID == id);
            for (int i = 1; i < 13; i++)
            {
                BookSellPerMonth bspm = new BookSellPerMonth()
                {
                    Month = i,
                    Total = BookSellPerMonth(i, id)
                };
                BooksVM.BookSellPerMonths.Add(bspm);
            }
            BooksVM.DoanhThu = Total(id);
            if (BooksVM.Book == null)
            {
                return NotFound();
            }

            return View(BooksVM);
        }

        // GET: Admin/Books/Create
        public IActionResult Create()
        {
            ViewData["AuthorID"] = new SelectList(_context.Authors, "ID", "FirstName");
            ViewData["PublisherID"] = new SelectList(_context.Publishers, "ID", "ID");
            ViewData["SpecialTagID"] = new SelectList(_context.SpecialTags, "ID", "Name");
            var genrers = _context.BooksGenrers.ToList();
            foreach (var genrer in genrers)
            {
                GenrerSelectedForBookViewModel genrerSelected = new GenrerSelectedForBookViewModel()
                {
                    BookGenrer = genrer,
                    Selected = false
                };
                BooksVM.GenrerSelectedForBookViewModels.Add(genrerSelected);
            }

            return View(BooksVM);
        }

        // POST: Admin/Books/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePOST()
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
                        if (!ModelState.IsValid)
                        {
                            return View(BooksVM);
                        }

                        _context1.Books.Add(BooksVM.Book);
                        await _context1.SaveChangesAsync();

                        //Image being saved

                        string webRootPath = _hostingEnvironment.WebRootPath;
                        var files = HttpContext.Request.Form.Files;

                        var BooksFromDb = _context1.Books.Find(BooksVM.Book.ID);
                        foreach (var genrer in BooksVM.GenrerSelectedForBookViewModels)
                        {
                            if (genrer.Selected == true)
                            {
                                Genrer g = new Genrer()
                                {
                                    BookID = BooksFromDb.ID,
                                    BookGenrerID = genrer.BookGenrer.ID
                                };
                                _context1.Genrers.Add(g);
                            }
                        }
                        await _context1.SaveChangesAsync();
                        if (files.Count != 0)
                        {
                            //Image has been uploaded
                            var uploads = Path.Combine(webRootPath, SD.BookImageFolder);
                            var extension = Path.GetExtension(files[0].FileName);

                            using (var filestream = new FileStream(Path.Combine(uploads, BooksVM.Book.ID + extension), FileMode.Create))
                            {
                                files[0].CopyTo(filestream);
                            }
                            BooksFromDb.Image = @"\" + SD.BookImageFolder + @"\" + BooksVM.Book.ID + extension;
                        }
                        else
                        {
                            //when user does not upload image
                            var uploads = Path.Combine(webRootPath, SD.BookImageFolder + @"\" + SD.DefaultBooksImage);
                            System.IO.File.Copy(uploads, webRootPath + @"\" + SD.BookImageFolder + @"\" + BooksVM.Book.ID + ".png");
                            BooksFromDb.Image = @"\" + SD.BookImageFolder + @"\" + BooksVM.Book.ID + ".png";
                        }
                        ViewData["AuthorID"] = new SelectList(_context1.Authors, "ID", "SecondName", BooksVM.Book.AuthorID);
                        ViewData["PublisherID"] = new SelectList(_context1.Publishers, "ID", "ID", BooksVM.Book.PublisherID);
                        ViewData["SpecialTagID"] = new SelectList(_context1.SpecialTags, "ID", "Name", BooksVM.Book.SpecialTagID);
                        await _context1.SaveChangesAsync();

                        return RedirectToAction(nameof(Index));
                    }

                }
                else
                {

                    if (!ModelState.IsValid)
                    {
                        return View(BooksVM);
                    }

                    _context.Books.Add(BooksVM.Book);
                    await _context.SaveChangesAsync();

                    //Image being saved

                    string webRootPath = _hostingEnvironment.WebRootPath;
                    var files = HttpContext.Request.Form.Files;

                    var BooksFromDb = _context.Books.Find(BooksVM.Book.ID);
                    foreach (var genrer in BooksVM.GenrerSelectedForBookViewModels)
                    {
                        if (genrer.Selected == true)
                        {
                            Genrer g = new Genrer()
                            {
                                BookID = BooksFromDb.ID,
                                BookGenrerID = genrer.BookGenrer.ID
                            };
                            _context.Genrers.Add(g);
                        }
                    }
                    await _context.SaveChangesAsync();
                    if (files.Count != 0)
                    {
                        //Image has been uploaded
                        var uploads = Path.Combine(webRootPath, SD.BookImageFolder);
                        var extension = Path.GetExtension(files[0].FileName);

                        using (var filestream = new FileStream(Path.Combine(uploads, BooksVM.Book.ID + extension), FileMode.Create))
                        {
                            files[0].CopyTo(filestream);
                        }
                        BooksFromDb.Image = @"\" + SD.BookImageFolder + @"\" + BooksVM.Book.ID + extension;
                    }
                    else
                    {
                        //when user does not upload image
                        var uploads = Path.Combine(webRootPath, SD.BookImageFolder + @"\" + SD.DefaultBooksImage);
                        System.IO.File.Copy(uploads, webRootPath + @"\" + SD.BookImageFolder + @"\" + BooksVM.Book.ID + ".png");
                        BooksFromDb.Image = @"\" + SD.BookImageFolder + @"\" + BooksVM.Book.ID + ".png";
                    }
                    ViewData["AuthorID"] = new SelectList(_context.Authors, "ID", "SecondName", BooksVM.Book.AuthorID);
                    ViewData["PublisherID"] = new SelectList(_context.Publishers, "ID", "ID", BooksVM.Book.PublisherID);
                    ViewData["SpecialTagID"] = new SelectList(_context.SpecialTags, "ID", "Name", BooksVM.Book.SpecialTagID);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception e)
            {
                BooksVM.PermissionError = e.InnerException.Message;
                if (BooksVM.PermissionError.Contains("permission"))
                {
                    BooksVM.TriggerError = null;
                }
                else
                {
                    BooksVM.PermissionError = null;
                    BooksVM.TriggerError = e.InnerException.Message.Replace("\r\nThe transaction ended in the trigger. The batch has been aborted.", ".");
                }
                ViewData["AuthorID"] = new SelectList(_context.Authors, "ID", "FirstName");
                ViewData["PublisherID"] = new SelectList(_context.Publishers, "ID", "ID");
                ViewData["SpecialTagID"] = new SelectList(_context.SpecialTags, "ID", "Name");
                return View(BooksVM);
            }
            
        }

        // GET: Admin/Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            BooksVM.Book = await _context.Books.Include(m => m.SpecialTag).Include(m => m.Author).Include(m => m.Publisher).SingleOrDefaultAsync(m => m.ID == id);

            if (BooksVM.Book == null)
            {
                return NotFound();
            }
            ViewData["AuthorID"] = new SelectList(_context.Authors, "ID", "FirstName", BooksVM.Book.AuthorID);
            ViewData["PublisherID"] = new SelectList(_context.Publishers, "ID", "ID", BooksVM.Book.PublisherID);
            ViewData["SpecialTagID"] = new SelectList(_context.SpecialTags, "ID", "Name", BooksVM.Book.SpecialTagID);
            return View(BooksVM);
        }

        // POST: Admin/Books/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
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

                                var productFromDb = _context1.Books.Where(m => m.ID == BooksVM.Book.ID).FirstOrDefault();

                                if (files.Count > 0 && files[0] != null)
                                {
                                    //if user uploads a new image
                                    var uploads = Path.Combine(webRootPath, SD.BookImageFolder);
                                    var extension_new = Path.GetExtension(files[0].FileName);
                                    var extension_old = Path.GetExtension(productFromDb.Image);

                                    if (System.IO.File.Exists(Path.Combine(uploads, BooksVM.Book.ID + extension_old)))
                                    {
                                        System.IO.File.Delete(Path.Combine(uploads, BooksVM.Book.ID + extension_old));
                                    }
                                    using (var filestream = new FileStream(Path.Combine(uploads, BooksVM.Book.ID + extension_new), FileMode.Create))
                                    {
                                        files[0].CopyTo(filestream);
                                    }
                                    BooksVM.Book.Image = @"\" + SD.BookImageFolder + @"\" + BooksVM.Book.ID + extension_new;
                                }
                                else
                                {
                                    if (productFromDb.Image != null)
                                    {
                                        productFromDb.Image = productFromDb.Image;
                                    }
                                }
                                if (BooksVM.Book.Image != null)
                                {
                                    productFromDb.Image = BooksVM.Book.Image;
                                }

                                productFromDb.BookName = BooksVM.Book.BookName;
                                productFromDb.BookDetail = BooksVM.Book.BookDetail;
                                productFromDb.AuthorID = BooksVM.Book.AuthorID;
                                productFromDb.PublisherID = BooksVM.Book.PublisherID;
                                productFromDb.SpecialTagID = BooksVM.Book.SpecialTagID;
                                productFromDb.ReleaseDate = BooksVM.Book.ReleaseDate;
                                productFromDb.Orders = BooksVM.Book.Orders;
                                productFromDb.Reviews = BooksVM.Book.Reviews;
                                productFromDb.Genrers = BooksVM.Book.Genrers;
                                productFromDb.BookPrice = BooksVM.Book.BookPrice;
                                productFromDb.Available = BooksVM.Book.Available;
                                productFromDb.Sold = BooksVM.Book.Sold;
                                await _context1.SaveChangesAsync();

                                return RedirectToAction(nameof(Index));
                            }
                            ViewData["AuthorID"] = new SelectList(_context1.Authors, "ID", "FirstName", BooksVM.Book.AuthorID);
                            ViewData["PublisherID"] = new SelectList(_context1.Publishers, "ID", "ID", BooksVM.Book.PublisherID);
                            ViewData["SpecialTagID"] = new SelectList(_context1.SpecialTags, "ID", "Name", BooksVM.Book.SpecialTagID);
                            return View(BooksVM);
                        }
  

                }
                else
                {

                        if (ModelState.IsValid)
                        {
                            string webRootPath = _hostingEnvironment.WebRootPath;
                            var files = HttpContext.Request.Form.Files;

                            var productFromDb = _context.Books.Where(m => m.ID == BooksVM.Book.ID).FirstOrDefault();

                            if (files.Count > 0 && files[0] != null)
                            {
                                //if user uploads a new image
                                var uploads = Path.Combine(webRootPath, SD.BookImageFolder);
                                var extension_new = Path.GetExtension(files[0].FileName);
                                var extension_old = Path.GetExtension(productFromDb.Image);

                                if (System.IO.File.Exists(Path.Combine(uploads, BooksVM.Book.ID + extension_old)))
                                {
                                    System.IO.File.Delete(Path.Combine(uploads, BooksVM.Book.ID + extension_old));
                                }
                                using (var filestream = new FileStream(Path.Combine(uploads, BooksVM.Book.ID + extension_new), FileMode.Create))
                                {
                                    files[0].CopyTo(filestream);
                                }
                                BooksVM.Book.Image = @"\" + SD.BookImageFolder + @"\" + BooksVM.Book.ID + extension_new;
                            }
                            else
                            {
                                if (productFromDb.Image != null)
                                {
                                    productFromDb.Image = productFromDb.Image;
                                }
                            }
                            if (BooksVM.Book.Image != null)
                            {
                                productFromDb.Image = BooksVM.Book.Image;
                            }

                            productFromDb.BookName = BooksVM.Book.BookName;
                            productFromDb.BookDetail = BooksVM.Book.BookDetail;
                            productFromDb.AuthorID = BooksVM.Book.AuthorID;
                            productFromDb.PublisherID = BooksVM.Book.PublisherID;
                            productFromDb.SpecialTagID = BooksVM.Book.SpecialTagID;
                            productFromDb.ReleaseDate = BooksVM.Book.ReleaseDate;
                            productFromDb.Orders = BooksVM.Book.Orders;
                            productFromDb.Reviews = BooksVM.Book.Reviews;
                            productFromDb.Genrers = BooksVM.Book.Genrers;
                            productFromDb.BookPrice = BooksVM.Book.BookPrice;
                            productFromDb.Available = BooksVM.Book.Available;
                            productFromDb.Sold = BooksVM.Book.Sold;
                            await _context.SaveChangesAsync();

                            return RedirectToAction(nameof(Index));
                        }
                        ViewData["AuthorID"] = new SelectList(_context.Authors, "ID", "FirstName", BooksVM.Book.AuthorID);
                        ViewData["PublisherID"] = new SelectList(_context.Publishers, "ID", "ID", BooksVM.Book.PublisherID);
                        ViewData["SpecialTagID"] = new SelectList(_context.SpecialTags, "ID", "Name", BooksVM.Book.SpecialTagID);
                        return View(BooksVM);
                }
            }
            catch (Exception e)
            {
                BooksVM.PermissionError = e.InnerException.Message;
                if (BooksVM.PermissionError.Contains("permission"))
                {
                    BooksVM.TriggerError = null;
                }
                else
                {
                    BooksVM.PermissionError = null;
                    BooksVM.TriggerError = e.InnerException.Message.Replace("\r\nThe transaction ended in the trigger. The batch has been aborted.", ".");
                }
                ViewData["AuthorID"] = new SelectList(_context.Authors, "ID", "FirstName");
                ViewData["PublisherID"] = new SelectList(_context.Publishers, "ID", "ID");
                ViewData["SpecialTagID"] = new SelectList(_context.SpecialTags, "ID", "Name");
                return View(BooksVM);
            }
        }

        // GET: Admin/Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            BooksVM.Book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Include(b => b.SpecialTag)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (BooksVM.Book == null)
            {
                return NotFound();
            }
            return View(BooksVM);
        }

        // POST: Admin/Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
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
                        string webRootPath = _hostingEnvironment.WebRootPath;
                        Book Books = await _context1.Books.FindAsync(id);

                        if (Books == null)
                        {
                            return NotFound();
                        }
                        else
                        {
                            var uploads = Path.Combine(webRootPath, SD.BookImageFolder);
                            var extension = Path.GetExtension(Books.Image);

                            if (System.IO.File.Exists(Path.Combine(uploads, Books.ID + extension)))
                            {
                                System.IO.File.Delete(Path.Combine(uploads, Books.ID + extension));
                            }
                            _context1.Books.Remove(Books);
                            await _context1.SaveChangesAsync();

                            return RedirectToAction(nameof(Index));
                        }
                    }
                }
                else
                {
                    string webRootPath = _hostingEnvironment.WebRootPath;
                    Book Books = await _context.Books.FindAsync(id);

                    if (Books == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        var uploads = Path.Combine(webRootPath, SD.BookImageFolder);
                        var extension = Path.GetExtension(Books.Image);

                        if (System.IO.File.Exists(Path.Combine(uploads, Books.ID + extension)))
                        {
                            System.IO.File.Delete(Path.Combine(uploads, Books.ID + extension));
                        }
                        _context.Books.Remove(Books);
                        await _context.SaveChangesAsync();

                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch (Exception e)
            {
                BooksVM.PermissionError = e.Message;
                return View(BooksVM);
            }
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.ID == id);
        }
        static string strConnectionString = "Server=DESKTOP-1OCF1PV;Database=WebsiteBanSach;Trusted_Connection=True";
        public int BookSellPerMonth(int month, int id) //danh sach tac pham cua tac gia
        {
            using (SqlConnection con = new SqlConnection(strConnectionString))
            {
                SqlCommand command = new SqlCommand("DoanhSoBanHang", con);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@month", month);
                command.Parameters.AddWithValue("@id", id);
                if (con.State == ConnectionState.Closed)
                    con.Open();
                SqlDataReader dataReader = command.ExecuteReader();
                try
                {
                    dataReader.Read();
                    return Convert.ToInt32(dataReader["Daban"]);
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }
        public double Total(int id) //danh sach tac pham cua tac gia
        {
            using (SqlConnection con = new SqlConnection(strConnectionString))
            {
                SqlCommand command = new SqlCommand("TongTienDauSach", con);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@id", id);
                if (con.State == ConnectionState.Closed)
                    con.Open();
                SqlDataReader dataReader = command.ExecuteReader();
                dataReader.Read();
                return Convert.ToDouble(dataReader["Doanhthu"]);
            }
        }
        public static IEnumerable<Book> SaleZero() //danh sach tac pham cua tac gia
        {
            List<Book> BookList = new List<Book>();
            using (SqlConnection con = new SqlConnection(strConnectionString))
            {
                SqlCommand command = new SqlCommand("SachChuaBan", con);
                command.CommandType = CommandType.StoredProcedure;
                if (con.State == ConnectionState.Closed)
                    con.Open();

                SqlDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    Book book = new Book();
                    book.ID = Convert.ToInt32(dataReader["ID"]);
                    book.BookName = dataReader["BookName"].ToString();
                    book.Image = dataReader["Image"].ToString();
                    if (book.Image == "")
                    {
                        book.Image = null;
                    }
                    book.BookPrice = Convert.ToDouble(dataReader["BookPrice"]);
                    BookList.Add(book);
                }
            }
            return BookList;
        }
    }
}
