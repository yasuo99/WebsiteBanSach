using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models.ViewModel
{
    public class OrdersViewModel
    {
        public Order Order { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public List<Order> Orders { get; set; }
        public string TriggerError { get; set; }
        public string PermissionError { get; set; }
    }
}
