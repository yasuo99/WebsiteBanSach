using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models.ViewModel
{
    public class ReplyReviewViewModel
    {
        public Review Review { get; set; }
        public List<Comment> Comments { get; set; }
    }
}
