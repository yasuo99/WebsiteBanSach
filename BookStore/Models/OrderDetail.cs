using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models
{
    public class OrderDetail
    {
        public int ID { get; set; }
        public int BookID { get; set; }
        [ForeignKey("BookID"), Display(Name = "Mã sách")]
        public Book Book { get; set; }
        public int OrderID { get; set; }
        [ForeignKey("OrderID"), Display(Name = "Mã đơn hàng")]
        public Order Order { get; set; }
        [Column]
        [Display(Name = "Số lượng")]
        public int Amount { get; set; }
        [Display(Name = "Tổng giá")]
        public double TotalPrice { get; set; }
    }
}
