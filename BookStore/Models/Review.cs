using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models
{
    public class Review
    {
        public int ID { get; set; }
        public int BookID { get; set; }
        [ForeignKey("BookID"), Display(Name = "Mã sách")]
        public virtual Book Book { get; set; }
        public int OrderID { get; set; }
        [ForeignKey("OrderID"), Display(Name = "Mã đơn hàng")]
        public Order Order { get; set; }
        public string ApplicationUserID { get; set; }
        [ForeignKey("ApplicationUserID"), Display(Name = "Mã khách hàng")]
        public virtual ApplicationUser ApplicationUser { get; set; }
        [Display(Name = "Số sao")]
        public int Star { get; set; }
        [Display(Name = "Đánh giá")]
        public string CustomerReview { get; set; }
        [Display(Name = "Thời gian")]
        public DateTime Date { get; set; }
        public string State { get; set; }
        public int Like { get; set; }
    }
}
