using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models
{
    public class Comment
    {
        public int ID { get; set; }
        public int ReviewID { get; set; }
        [ForeignKey("ReviewID")]
        public Review Review { get; set; }
        public string ApplicationUserID { get; set; }
        [ForeignKey("ApplicationUserID")]
        public ApplicationUser ApplicationUser { get; set; }
        public string ReplyReview { get; set; }
        public int Like { get; set; }
    }
}
