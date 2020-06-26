using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models
{
    public class Shipper
    {
        public int ID { get; set; }
        [Display(Name = "Shipper")]
        public string Name { get; set; }
        [Display(Name = "SĐT")]
        public string Phone { get; set; }
        [Display(Name = "Hình ảnh")]
        public string Images { get; set; }
        [NotMapped]
        public string TriggerError { get; set; }
        [NotMapped]
        public string PermissionError { get; set; }
    }
}
