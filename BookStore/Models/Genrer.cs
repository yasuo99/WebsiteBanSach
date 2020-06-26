using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models
{
    public class Genrer
    {
        public int ID { get; set; } 
        public int BookID { get; set; }
        [Display(Name = "Mã sách")]
        public Book Book { get; set; }      
        public int BookGenrerID { get; set; }
        [Display(Name = "Mã thể loại")]
        public BookGenrer BookGenrer { get; set; }
    }
}
