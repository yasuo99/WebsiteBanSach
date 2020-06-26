using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models
{
    public class Publisher
    {
        public int ID { get; set; }
        [Display(Name = "Nhà xuất bản")]
        public string Name { get; set; }
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }
        [NotMapped]
        public string TriggerError { get; set; }
        [NotMapped]
        public string PermissionError { get; set; }
    }
}
