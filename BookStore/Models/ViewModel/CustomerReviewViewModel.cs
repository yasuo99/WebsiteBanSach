using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models.ViewModel
{
    public class CustomerReviewViewModel
    {
        public ApplicationUser ApplicationUser { get; set; }
        public List<Review> Reviews { get; set; }
        public List<Book> Books { get; set; }
        public Book Book { get; set; }
        public Review Review { get; set; }
        public Order Order { get; set; }
        public List<BookReviewViewModel> BookReviewViewModels { get; set; }
        public BookReviewViewModel BookReviewViewModel { get; set; }
        public string PermissionError { get; set; }
    }
}
