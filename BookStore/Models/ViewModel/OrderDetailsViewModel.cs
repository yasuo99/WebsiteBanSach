using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models.ViewModel
{
    public class OrderDetailsViewModel
    {
        public OrderDetail OrderDetail { get; set; }
        public Order Order { get; set; }
        public List<Book> Books { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}
