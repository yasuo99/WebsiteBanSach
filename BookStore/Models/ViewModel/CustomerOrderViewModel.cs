using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models.ViewModel
{
    public class CustomerOrderViewModel
    {
        public List<Order> Orders { get; set; }
        public Order Order { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
        public OrderDetail OrderDetail { get; set; }
        public List<Book> Books { get; set; }
        public Book Book { get; set; }
        public List<BookTotalPriceViewModel> BookTotalPriceViewModels { get; set; }
        public string TriggerError { get; set; }
        public string PermissionError { get; set; }
    }
}
