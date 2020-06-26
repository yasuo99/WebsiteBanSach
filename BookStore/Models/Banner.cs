using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models
{
    public class Banner
    {
        public int ID { get; set; }
        public string Image { get; set; }
        [NotMapped]
        public string PermissionError { get; set; }
    }
}
