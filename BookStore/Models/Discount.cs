using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models
{
    public class Discount
    {
        public int ID { get; set; }
        [Display(Name = "Mã giảm giá")]
        public string Code { get; set; }
        [Display(Name = "Giá trị")]
        public int Value { get; set; }
        [Display(Name = "Hiện có")]
        public int Available { get; set; }
        public virtual ICollection<Book> Books { get; set; }
        [NotMapped]
        public string TriggerError { get; set; }
        [NotMapped]
        public string PermissionError { get; set; }
    }
}
