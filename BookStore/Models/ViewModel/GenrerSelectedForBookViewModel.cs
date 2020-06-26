using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models.ViewModel
{
    public class GenrerSelectedForBookViewModel
    {
        public BookGenrer BookGenrer { get; set; }
        public bool Selected { get; set; }
    }
}
