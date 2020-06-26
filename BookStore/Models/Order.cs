using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models
{
    public class Order
    {
        public int ID { get; set; }
        [Display(Name ="Người đặt")]
        public string ApplicationUserID { get; set; }
        [ForeignKey("ApplicationUserID")]
        public virtual ApplicationUser ApplicationUser { get; set; }
        [Display(Name = "Người nhận")]
        public string Name { get; set; }
        [Display(Name = "Địa chỉ giao hàng")]
        public string Address { get; set; }
        [Display(Name = "SĐT")]
        public string PhoneNumber { get; set; }
        [Display(Name = "Ngày đặt hàng")]
        public DateTime Date { get; set; }
        public int DiscountID { get; set; }
        [ForeignKey("DiscountID"), Display(Name = "Mã giảm giá")]
        public virtual Discount Discount { get; set; }
        [Display(Name = "Mã shipper")]
        public int? ShipperID { get; set; }
        [ForeignKey("ShipperID")]
        public virtual Shipper Shipper { get; set; }
        [Display(Name = "Tổng cộng")]
        public double Total { get; set; }
        [Display(Name = "Tình trạng")]
        public string State { get; set; }
        public virtual ICollection<OrderDetail> Books { get; set; }
        [NotMapped]
        public string TriggerError { get; set; }
        [NotMapped]
        public string PermissionError { get; set; }
    }
}
