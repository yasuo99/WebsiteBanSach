using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models
{
    public class Author
    {
        public int ID { get; set; }
        [Required, Display(Name ="Họ và tên")]
        public string FirstName { get; set; }
        [Required, Display(Name = "Kí danh")]
        public string SecondName { get; set; }
        [Display(Name = "Hình ảnh")]
        public string Image { get; set; }
        [Required, Display(Name = "Công ty")]
        public string CompanyName { get; set; }
        public string Alias { get; set; }
        public ICollection<Book> Books { get; set; }
                
    }
}
