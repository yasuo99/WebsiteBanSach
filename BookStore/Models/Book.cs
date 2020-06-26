using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models
{
    public class Book
    {
        public int ID { get; set; }
        [Required,Display(Name = "Tên sách")]
        public string BookName { get; set; }
        [Display(Name = "Hình ảnh")]
        public string Image { get; set; }
        [Required, Display(Name = "Mô tả")]
        public string BookDetail { get; set; }
        [Required, Display(Name ="Ngày xuất bản")]
        public DateTime ReleaseDate { get; set; }
        
        public int AuthorID { get; set; }
        [ForeignKey("AuthorID"), Display(Name = "Mã tác giả")]
        public virtual Author Author { get; set; }
        
        public int PublisherID { get; set; }
        [ForeignKey("PublisherID"), Display(Name = "Mã NXB")]
        public virtual Publisher Publisher { get; set; }
        [Required, Display(Name = "Giá sách")]
        public double BookPrice { get; set; }
        public int SpecialTagID { get; set; }
        [ForeignKey("SpecialTagID"), Display(Name = "Tag")]
        public virtual SpecialTag SpecialTag { get; set; }
        [Display(Name = "Hiện có")]
        public int Available { get; set; }
        [Display(Name = "Đã bán")]
        public int Sold { get; set; }
        [NotMapped]
        public int Amount { get; set; }
        public double Rate { get; set; }
        public string Alias { get; set; }
        public virtual ICollection<OrderDetail> Orders { get; set; }
        public virtual ICollection<Review> Reviews { get; set; } 
        public virtual ICollection<Genrer> Genrers { get; set; }
        static string strConnectionString = "Server=DESKTOP-9127M85;Database=WebsiteBanSach;User Id=thanh;Password=thanh1";
        public static IEnumerable<Book> GetBooks()
        {
            List<Book> BooksList = new List<Book>();
            using (SqlConnection con = new SqlConnection(strConnectionString))
            {
                SqlCommand command = new SqlCommand("Select * from KhachHangTiemNang", con);
                if (con.State == ConnectionState.Closed)
                    con.Open();

                SqlDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    Book Book = new Book();
                    Book.ID = Convert.ToInt32(dataReader["ID"]);
                    Book.BookName = dataReader["BookName"].ToString();
                    Book.BookDetail = dataReader["BookDetail"].ToString();
                    BooksList.Add(Book);
                }
            }
            return BooksList;
        }
    }
}
