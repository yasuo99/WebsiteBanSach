using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BookStore.Models;
using BookStore.Data;
using Microsoft.EntityFrameworkCore;
using BookStore.Extensions;
using System.Dynamic;
using Microsoft.AspNetCore.Mvc.Rendering;
using BookStore.Models.ViewModel;
using System.Security.Claims;
using System.Text;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Net.WebSockets;
using BookStore.Sitemap;
using Microsoft.Extensions.Hosting.Internal;

namespace BookStore.Controllers
{
    //Luyện Ngọc Thanh - 17110221
    [Area("Customer")]
    public class HomeController : Controller
    {
        static readonly ISitemapNodeRepository repository = new SitemapNodeRepository();
        private readonly ApplicationDbContext _db;
        private int PageSize = 18;
        private readonly IWebHostEnvironment _hostingEnvironment;
        [BindProperty]
        public BooksViewModel BooksVM { get; set; }
        public HomeController(ApplicationDbContext db, IWebHostEnvironment hostingEnvironment)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;
        }
        [Route("tim-kiem-nang-cao/tac-gia/{author}", Name = "AdvancedSearch1")]
        [Route("tim-kiem-nang-cao/the-loai-{genres}", Name = "AdvancedSearch2")]
        [Route("tim-kiem-nang-cao/sap-xep-{order}", Name = "AdvancedSearch3")]
        [Route("tim-kiem-nang-cao/tac-gia-{author}/sap-xep-{order}", Name ="AdvancedSearch13")]
        [Route("tim-kiem-nang-cao/tac-gia-{author}/the-loai-{genres}", Name = "AdvancedSearch12")]
        [Route("tim-kiem-nang-cao/the-loai-{genres}/sap-xep-{order}", Name = "AdvancedSearch23")]
        [Route("tim-kiem-nang-cao/tac-gia-{author}/the-loai-{genres}/sap-xep-{order}", Name = "AdvancedSearch123")]
        public async Task<IActionResult> AdvancedSearch(int productPage = 1,string advancedsearch = null, string author = null, string genres = null, string order = null)
        {
            StringBuilder param = new StringBuilder();
            ViewData["SpecialTag"] = await _db.SpecialTags.ToListAsync();

            BooksViewModel BooksVM = new BooksViewModel()
            {
                Books = new List<Models.Book>(),
                BooksSeen = new List<Book>(),
                ReplyReviewViewModels = new List<ReplyReviewViewModel>(),
                BestSeller = new List<Book>(),
                SameGenrer = new List<Book>(),
                BookGenrers = new List<BookGenrer>()
            };

            StringBuilder param1 = new StringBuilder();

            param1.Append("/Customer/Home?productPage=:");
            param1.Append("&search=");

            BooksVM.Books = _db.Books.Include(a => a.Author).Include(a => a.Publisher).Include(a => a.SpecialTag).ToList();
            BooksVM.BestSeller = BooksVM.Books.OrderByDescending(u => u.Sold).Take(10).ToList();
            //tìm kiếm bằng 1 thanh search bar
            var count = BooksVM.Books.Count;
            if (genres != null)
            {
                var x = from a in _db.Books
                        join b in _db.Genrers on a.ID equals b.BookID
                        join c in _db.BooksGenrers on b.BookGenrerID equals c.ID
                        where c.Alias == genres
                        select a;
                BooksVM.Books = x.ToList();
            }
            //lọc theo thứ tự
            if (author != null)
            {
                BooksVM.Books = BooksVM.Books.Where(u => u.Author.Alias == author).ToList();
            }    
            if(order != null)
            {
                if(order == "a-z")
                {
                    BooksVM.Books = BooksVM.Books.OrderBy(u => u.BookName).ToList();
                }   
                if(order == "z-a")
                {
                    BooksVM.Books = BooksVM.Books.OrderByDescending(u => u.BookName).ToList();
                }   
                if(order == "ngay-xuat-ban")
                {
                    BooksVM.Books = BooksVM.Books.OrderBy(u => u.ReleaseDate).ToList();
                }   
                if(order == "gia-giam-dan")
                {
                    BooksVM.Books = BooksVM.Books.OrderByDescending(u => u.BookPrice).ToList();
                }   
                if(order == "gia-tang-dan")
                {
                    BooksVM.Books = BooksVM.Books.OrderBy(u => u.BookPrice).ToList();
                }    
            }
            //tìm những cuốn sách có cùng 1 trong các thể loại với cuốn đang xem    
            BooksVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItems = count,
                urlParam = param.ToString()
            };
            //tạo phiên cho những sản phẩm đã xem qua
            List<int> lstSeen = HttpContext.Session.Get<List<int>>("ssSeenBook");
            if (lstSeen == null)
            {
                lstSeen = new List<int>();
            }
            if (lstSeen != null)
            {
                foreach (var i in lstSeen)
                {
                    var tempbook = _db.Books.Where(m => m.ID == i).Include(m => m.Author).Include(m => m.Publisher).Include(m => m.SpecialTag).FirstOrDefault();
                    BooksVM.BooksSeen.Add(tempbook);
                }
            }
            var banner = _db.Banners.ToList();
            BooksVM.Banners = banner;
            var bookgenrer = _db.BooksGenrers.ToList();
            BooksVM.BookGenrers = bookgenrer;
            BooksVM.Authors = _db.Authors.ToList();
            return View("Index",BooksVM);
        }
        public async Task<IActionResult> Index(int productPage = 1, string timkiem = null, string genresparam = null, string authorparam = null, string orderparam = null, string tag = null)
        {
            
            ViewData["SpecialTag"] = await _db.SpecialTags.ToListAsync();
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            BooksViewModel BooksVM = new BooksViewModel()
            {
                Books = new List<Models.Book>(),
                BooksSeen = new List<Book>(),
                ReplyReviewViewModels = new List<ReplyReviewViewModel>(),
                BestSeller = new List<Book>(),
                SameGenrer = new List<Book>(),
                BookGenrers = new List<BookGenrer>()
            };

            StringBuilder param = new StringBuilder();

            param.Append("/Customer/Home?productPage=:");
            param.Append("&tim-kiem=");

            BooksVM.Books = _db.Books.Include(a => a.Author).Include(a => a.Publisher).Include(a => a.SpecialTag).ToList();
            BooksVM.BestSeller = BooksVM.Books.OrderByDescending(u => u.Sold).Take(10).ToList();
            if (User.IsInRole(SD.AdminEndUser + "," + SD.SuperAdminEndUser))
            {
                BooksVM.Books = BooksVM.Books.Where(a => (a.ID).ToString() == claim.Value).ToList();
            }
            //tìm kiếm bằng 1 thanh timkiem bar
            if (timkiem != null)
            {
                //BooksVM.Books = BooksVM.Books.Where(a => a.BookName.ToLower().Contains(timkiem.ToLower()) || a.Author.SecondName.ToLower().Contains(timkiem.ToLower()) || a.Publisher.Name.ToLower().Contains(timkiem.ToLower()) || a.SpecialTag.Name.ToLower().Contains(timkiem.ToLower())).ToList();
                BooksVM.Books = Search(timkiem).ToList();
                param.Append(modifyString(timkiem));
            }
            if (authorparam != null && orderparam == null && genresparam == null)
            {
                return RedirectToRoute("AdvancedSearch1", new { author = authorparam});
            }
            if (authorparam == null && orderparam == null && genresparam != null)
            {
                return RedirectToRoute("AdvancedSearch2", new {genres = genresparam});
            }
            if (authorparam == null && orderparam != null && genresparam == null)
            {
                return RedirectToRoute("AdvancedSearch3", new {order = orderparam });
            }
            if (authorparam != null && genresparam != null && orderparam == null)
            {
                return RedirectToRoute("AdvancedSearch12", new { author = authorparam, genres = genresparam});
            }
            if (authorparam != null && orderparam != null && genresparam == null)
            {
                return RedirectToRoute("AdvancedSearch13", new { author = authorparam, order = orderparam });
            }
            if (authorparam != null && orderparam != null && genresparam != null)
            {
                return RedirectToRoute("AdvancedSearch123", new { author = authorparam, genres = genresparam , order = orderparam});
            }
            if (authorparam == null && orderparam != null && genresparam != null)
            {
                return RedirectToRoute("AdvancedSearch23", new {genres = genresparam, order = orderparam });
            }
            if (tag != null)
            {
                BooksVM.Books = SearchByTag(tag).ToList();
            }
            var count = BooksVM.Books.Count;
            var x = HttpContext.Session.GetString("author");
            var y = HttpContext.Session.GetString("genres");
            var z = HttpContext.Session.GetString("order");
            //lọc theo thứ tự
         
            BooksVM.Books = BooksVM.Books.Skip((productPage - 1) * PageSize).Take(PageSize).ToList();
            BooksVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItems = count,
                urlParam = param.ToString()
            };
            //tạo phiên cho những sản phẩm đã xem qua
            List<int> lstSeen = HttpContext.Session.Get<List<int>>("ssSeenBook");
            if (lstSeen == null)
            {
                lstSeen = new List<int>();
            }
            if (lstSeen != null)
            {
                foreach (var i in lstSeen)
                {
                    var tempbook = _db.Books.Where(m => m.ID == i).Include(m => m.Author).Include(m => m.Publisher).Include(m => m.SpecialTag).FirstOrDefault();
                    BooksVM.BooksSeen.Add(tempbook);
                }
            }
            var banner = _db.Banners.ToList();
            BooksVM.Banners = banner;
            var bookgenrer = _db.BooksGenrers.ToList();
            BooksVM.BookGenrers = bookgenrer;
            BooksVM.Authors = _db.Authors.ToList();
            var path = Path.Combine(_hostingEnvironment.WebRootPath + @"\sitemap.xml");
            string xml = repository.SetSitemapNodes(Url, path,BooksVM);
            return View(BooksVM);
        }
        [HttpPost, ActionName("DetailsPost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DetailsPost(int id, int amount)
        {

            List<ShoppingSessionViewModel> lstShoppingCart = HttpContext.Session.Get<List<ShoppingSessionViewModel>>("ssShoppingCart");
            if (lstShoppingCart == null)
            {
                lstShoppingCart = new List<ShoppingSessionViewModel>();
            }
            ShoppingSessionViewModel shoppingSessionViewModel = new ShoppingSessionViewModel();
            var book = await _db.Books.Where(u => u.ID == BooksVM.Book.ID).FirstOrDefaultAsync();
            shoppingSessionViewModel.Book = book;
            shoppingSessionViewModel.Amount = BooksVM.Book.Amount;
            lstShoppingCart.Add(shoppingSessionViewModel);
            HttpContext.Session.Set("ssShoppingCart", lstShoppingCart);

            return RedirectToAction("Index", "Home", new { area = "Customer" });

        }
        public async Task<IActionResult> Details(int id, string alias = null, int reviewPage = 1) //tạo session mua hàng nếu chưa có, session bao gồm một list viewmodel nơi chứa cuốn sách lựa chọn và số lượng cho mỗi cuốn
        {
            //Tạo session xác định những cuốn sách đã xem qua
            List<int> lstSeen = HttpContext.Session.Get<List<int>>("ssSeenBook");
            if (lstSeen == null)
            {
                lstSeen = new List<int>();
            }
            var Book = await _db.Books.Where(m => m.ID == id).Include(m => m.Author).Include(m => m.Publisher).Include(m => m.SpecialTag).FirstOrDefaultAsync();
            if (!lstSeen.Exists(u => u.Equals(id)))
            {
                lstSeen.Add(id);
            }
            HttpContext.Session.Set("ssSeenBook", lstSeen);
            var reviews = _db.Reviews.Where(u => u.BookID == id && u.State == "Đã duyệt").Include(u => u.ApplicationUser).Include(u => u.Book).Include(u => u.Order).ToList();

            BooksViewModel booksvm = new BooksViewModel()
            {
                Book = Book,
                Reviews = reviews,
                BooksSeen = new List<Book>(),
                ReplyReviewViewModels = new List<ReplyReviewViewModel>(),
                SameGenrer = new List<Book>(),
                Books = new List<Book>()
            };
            foreach (var review in reviews)
            {

                var comments = _db.Comments.Where(u => u.ReviewID == review.ID).Include(u => u.ApplicationUser).ToList();
                ReplyReviewViewModel rep = new ReplyReviewViewModel()
                {
                    Review = review,
                    Comments = comments
                };
                booksvm.ReplyReviewViewModels.Add(rep);
            }
            if (lstSeen != null)
            {
                foreach (var i in lstSeen)
                {
                    if (i != id)
                    {
                        var tempbook = _db.Books.Where(m => m.ID == i).Include(m => m.Author).Include(m => m.Publisher).Include(m => m.SpecialTag).FirstOrDefault();
                        booksvm.BooksSeen.Add(tempbook);
                    }
                }
            }
            //thực hiện phân trang cho review
            booksvm.Reviews = booksvm.Reviews.Skip((reviewPage - 1) * 8).Take(8).ToList();
            int count = reviews.Count;
            StringBuilder param = new StringBuilder();
            param.Append("/Customer/Home/Details/" + id + "?reviewPage=:");
            booksvm.PagingInfo = new PagingInfo
            {
                CurrentPage = reviewPage,
                ItemsPerPage = 8,
                TotalItems = count,
                urlParam = param.ToString()
            };
            var genrerofbook = from a in _db.Books
                               join b in _db.Genrers
                               on a.ID equals b.BookID
                               join c in _db.BooksGenrers
                               on b.BookGenrerID equals c.ID
                               where b.BookID == id
                               select c.Name;
            //danh sách những cuốn sách cùng thể loại
            List<Book> samegenrer = new List<Book>();
            foreach (var genrer in genrerofbook)
            {
                var samegenrerbook = (from a in _db.Books
                                      join b in _db.Genrers
                                      on a.ID equals b.BookID
                                      join c in _db.BooksGenrers
                                      on b.BookGenrerID equals c.ID
                                      where c.Name == genrer && a.ID != id
                                      select a).ToList();
                foreach (var item in samegenrerbook)
                {
                    samegenrer.Add(item);
                }
            }
            var author = _db.Authors.Where(u => u.ID == Book.AuthorID).FirstOrDefault();
            booksvm.Author = author;
            booksvm.SameGenrer = samegenrer;
            return View(booksvm);
        }
        public IActionResult Privacy()
        {
            return View();
        }
        
        public IActionResult Remove(int id, int amount)
        {
            //xóa sản phẩm và số lượng của nó ra khỏi session
            List<ShoppingSessionViewModel> lstShoppingCart = HttpContext.Session.Get<List<ShoppingSessionViewModel>>("ssShoppingCart");
            if (lstShoppingCart.Count > 0)
            {
                foreach (var item in lstShoppingCart)
                {
                    if (item.Book.ID == id)
                    {
                        lstShoppingCart.Remove(item);
                        break;
                    }
                }
            }

            HttpContext.Session.Set("ssShoppingCart", lstShoppingCart);

            return RedirectToAction(nameof(Index));
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpPost]
        public async Task<IActionResult> ReplyReview(int rid, int bid, string replyreview) //phản hồi cho đánh giá có mã đơn hàng là rid và mã cuốn sách được đánh giá là bid
        {
            if (replyreview == null)
            {
                return RedirectToAction(nameof(Index));
            }
            var user = _db.ApplicationUsers.Where(u => u.Email == User.Identity.Name).FirstOrDefault();
            Comment comment = new Comment()
            {
                ReviewID = rid,
                ApplicationUserID = user.Id,
                ReplyReview = replyreview,
                Like = 0
            };
            _db.Comments.Add(comment);
            await _db.SaveChangesAsync();
            return RedirectToAction("Details", "Home", new { id = bid });
        }
        static string strConnectionString = "Server=DESKTOP-1OCF1PV;Database=WebsiteBanSach;Trusted_Connection=True";
        public static IEnumerable<Book> GetBooks()  //Top 10 cuon sach ban chay nhat
        {
            List<Book> BooksList = new List<Book>();
            using (SqlConnection con = new SqlConnection(strConnectionString))
            {
                SqlCommand command = new SqlCommand("Select * from Top10BestSeller", con);
                if (con.State == ConnectionState.Closed)
                    con.Open();

                SqlDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    Book Book = new Book();
                    Book.ID = Convert.ToInt32(dataReader["ID"]);
                    Book.BookName = dataReader["BookName"].ToString();
                    Book.BookDetail = dataReader["BookDetail"].ToString();
                    Book.Image = dataReader["Image"].ToString();
                    if (Book.Image == "")
                    {
                        Book.Image = null;
                    }
                    Book.BookPrice = Convert.ToDouble(dataReader["BookPrice"]);
                    BooksList.Add(Book);
                }
            }
            return BooksList;
        }
        public static IEnumerable<Book> GetHighRateBooks() //danh sach sach duoc danh gia cao
        {
            List<Book> BooksList = new List<Book>();
            using (SqlConnection con = new SqlConnection(strConnectionString))
            {
                SqlCommand command = new SqlCommand("Select * from SachDuocDanhGiaCao", con);
                if (con.State == ConnectionState.Closed)
                    con.Open();

                SqlDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    Book Book = new Book();
                    Book.ID = Convert.ToInt32(dataReader["ID"]);
                    Book.BookName = dataReader["BookName"].ToString();
                    Book.BookDetail = dataReader["BookDetail"].ToString();
                    Book.BookPrice = Convert.ToDouble(dataReader["BookPrice"]);
                    Book.Image = dataReader["Image"].ToString();
                    if (Book.Image == "")
                    {
                        Book.Image = null;
                    }
                    BooksList.Add(Book);
                }
            }
            return BooksList;
        }
        public static IEnumerable<PotentialCustomerViewModel> GetHighPaidCustomer() //danh sach sach duoc danh gia cao
        {
            List<PotentialCustomerViewModel> List = new List<PotentialCustomerViewModel>();
            using (SqlConnection con = new SqlConnection(strConnectionString))
            {
                SqlCommand command = new SqlCommand("Select * from KhachHangTiemNang", con);
                if (con.State == ConnectionState.Closed)
                    con.Open();

                SqlDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    PotentialCustomerViewModel po = new PotentialCustomerViewModel()
                    {
                        Email = dataReader["Email"].ToString(),
                        Total = Convert.ToDouble(dataReader["TongTieu"])
                    };
                    List.Add(po);
                }
            }
            return List;
        }
        public static IEnumerable<Book> GetBookSameGenrer(string genrer)
        {
            List<Book> BookList = new List<Book>();
            using (SqlConnection con = new SqlConnection(strConnectionString))
            {
                SqlCommand command = new SqlCommand("SachTheLoai", con);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Name", genrer);
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
        public static IEnumerable<Author> GetAuthors() //top 5 tac gia dua tren doanh so ban hang hoac so luong sach cua tac gia do trong cua hang
        {
            List<Author> AuthorsList = new List<Author>();
            using (SqlConnection con = new SqlConnection(strConnectionString))
            {
                SqlCommand command = new SqlCommand("Select * from Top5Author", con);
                if (con.State == ConnectionState.Closed)
                    con.Open();

                SqlDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    Author author = new Author();
                    author.ID = Convert.ToInt32(dataReader["ID"]);
                    author.SecondName = dataReader["SecondName"].ToString();
                    author.Image = dataReader["Image"].ToString();
                    if (author.Image == "")
                    {
                       author.Image = null;
                    }
                    AuthorsList.Add(author);
                }
            }
            return AuthorsList;
        }
        public static IEnumerable<Book> GetListBookOfAuthor(string secondname) //danh sach tac pham cua tac gia
        {
            List<Book> BookList = new List<Book>();
            using (SqlConnection con = new SqlConnection(strConnectionString))
            {
                SqlCommand command = new SqlCommand("SachCuaTacGia", con);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Name", secondname);
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
                        book.Image = null;
                    book.BookPrice = Convert.ToDouble(dataReader["BookPrice"]);
                    BookList.Add(book);
                }
            }
            return BookList;
        }
        public static int CountBooksOfAuthor(string genrer)
        {
            using (SqlConnection con = new SqlConnection(strConnectionString))
            {
                SqlCommand command = new SqlCommand("SachCuaTheLoai", con);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Name", genrer);
                if (con.State == ConnectionState.Closed)
                    con.Open();
                SqlDataReader dataReader = command.ExecuteReader();
                dataReader.Read();
                if (dataReader.HasRows)
                {
                    return Convert.ToInt32(dataReader["SoLuong"]);
                }
                else
                    return 0;
            }
        }
        public IEnumerable<Book> Search(string search) //danh sach tac pham cua tac gia
        {
            int count = 0;
            bool checkString = false;
            try
            {
                int year = Int32.Parse(search);
                checkString = true;
            }
            catch
            {
                checkString = false;
            }
            for (int i = 0; i < search.Length; i++)
            {
                if (search[i] == '-' || search[i] == '/')
                {
                    count++;
                    checkString = false;
                }
            }
            if (checkString)
            {
                if(count == 0)
                {
                    search += "-01-01";
                }
                if(count == 1)
                {
                    search += "-01";
                }    
                List<Book> list = new List<Book>();
                using (SqlConnection con = new SqlConnection(strConnectionString))
                {
                    SqlCommand command = new SqlCommand("Select * from Fn_SearchBookDate(@date)", con);
                    command.Parameters.AddWithValue("@date", search);
                    if (con.State == ConnectionState.Closed)
                        con.Open();
                    SqlDataAdapter da = new SqlDataAdapter(command);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    foreach (DataRow row in dt.Rows)
                    {
                        Book book = new Book()
                        {
                            ID = Convert.ToInt32(row["ID"]),
                            BookName = row["BookName"].ToString(),
                            Image = row["Image"].ToString(),
                            BookPrice = Convert.ToDouble(row["BookPrice"])
                        };
                        if (book.Image == "")
                            book.Image = null;
                        list.Add(book);
                    }
                    con.Close();
                }
                return list;
            }
            else
            {
                List<Book> list = new List<Book>();
                using (SqlConnection con = new SqlConnection(strConnectionString))
                {
                    SqlCommand command = new SqlCommand("Select * from Fn_SearchBook(@name)", con);
                    command.Parameters.AddWithValue("@name", search);
                    if (con.State == ConnectionState.Closed)
                        con.Open();
                    SqlDataAdapter da = new SqlDataAdapter(command);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    foreach (DataRow row in dt.Rows)
                    {
                        Book book = new Book()
                        {
                            ID = Convert.ToInt32(row["ID"]),
                            BookName = row["BookName"].ToString(),
                            Image = row["Image"].ToString(),
                            BookPrice = Convert.ToDouble(row["BookPrice"])
                        };
                        if (book.Image == "")
                            book.Image = null;
                        list.Add(book);
                    }
                    con.Close();
                }
                return list;
            }
        }
        public static IEnumerable<Book> SearchByTag(string search) //danh sach tac pham cua tac gia
        {
            List<Book> list = new List<Book>();
            using (SqlConnection con = new SqlConnection(strConnectionString))
            {
                SqlCommand command = new SqlCommand("Select * from Fn_SearchBookByTag(@name)", con);
                command.Parameters.AddWithValue("@name", search);
                if (con.State == ConnectionState.Closed)
                    con.Open();
                SqlDataAdapter da = new SqlDataAdapter(command);
                DataTable dt = new DataTable();
                da.Fill(dt);
                foreach (DataRow row in dt.Rows)
                {
                    Book book = new Book()
                    {
                        ID = Convert.ToInt32(row["ID"]),
                        BookName = row["BookName"].ToString(),
                        Image = row["Image"].ToString(),
                        BookPrice = Convert.ToDouble(row["BookPrice"])
                    };
                    if (book.Image == "")
                        book.Image = null;
                    list.Add(book);
                }
            }
            return list;
        }
        public string modifyString(string search)
        {
            string x = search.ToLower().Replace(' ', '-');
            char[] aArr = { 'â', 'ẫ', 'ậ', 'ầ', 'ẩ', 'ấ', 'ặ', 'ẳ', 'ằ', 'ẵ', 'ă', 'ắ', 'à', 'á', 'ạ', 'ã', 'ả' };
            char[] eArr = { 'ê', 'ể', 'ề', 'ệ', 'ễ', 'ế' };
            char[] oArr = { 'ô', 'ổ', 'ồ', 'ố', 'ộ', 'ỗ' };
            char[] iArr = { 'ỉ', 'ì', 'í', 'ị', 'ĩ' };
            char[] uArr = { 'ủ', 'ũ', 'ú', 'ù', 'ụ' };
            char[] yArr = { 'ý', 'ỷ', 'ỳ', 'ỹ', 'ỵ'};
            foreach(var i in aArr)
            {
                x = x.Replace(i, 'a');
            }
            foreach (var i in eArr)
            {
                x = x.Replace(i, 'e');
            }
            foreach (var i in oArr)
            {
                x = x.Replace(i, 'o');
            }
            foreach (var i in iArr)
            {
                x = x.Replace(i, 'i');
            }
            foreach (var i in uArr)
            {
                x = x.Replace(i, 'u');
            }
            foreach (var i in yArr)
            {
                x = x.Replace(i, 'y');
            }
            return x;
        }
    }
}
