using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models.ViewModel
{
    public class ShoppingCartViewModel
    {
        public List<Book> Books { get; set; }
        public Order Order{ get; set; }
        public OrderDetail OrderDetail { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public double Total { get; set; }
    }
}
