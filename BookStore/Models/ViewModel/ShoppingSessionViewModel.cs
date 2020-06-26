using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models.ViewModel
{
    public class ShoppingSessionViewModel
    {
        public Book Book { get; set; }
        public Book BookHaveSeen { get; set; }
        public int Amount { get; set; }
    }
}
