using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models
{
    public class BookGenrer
    {
        public int ID { get; set; }
        [Required]
        [Display(Name = "Thể loại")]
        public string Name { get; set; }
        public virtual ICollection<Genrer> Genrers { get; set; }
        [NotMapped]
        public string TriggerError { get; set; }
        [NotMapped]
        public string PermissionError { get; set; }
        public string Alias { get; set; }
    }
}
