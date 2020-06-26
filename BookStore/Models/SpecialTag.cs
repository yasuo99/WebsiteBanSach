using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models
{
    public class SpecialTag
    {
        public int ID { get; set; }
        [Required]
        [Display(Name = "Tag")]
        public string Name { get; set; }
        public virtual ICollection<Book> Books { get; set; }
        public string Alias { get; set; }
        [NotMapped]
        public string TriggerError { get; set; }
        [NotMapped]
        public string PermissionError { get; set; }
    }
}
