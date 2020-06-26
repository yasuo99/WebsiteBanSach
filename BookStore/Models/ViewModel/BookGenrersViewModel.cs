using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models.ViewModel
{
    public class BookGenrersViewModel
    {
        public BookGenrer BookGenrer { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public List<BookGenrer> BookGenrers { get; set; }    
        public int Count { get; set; }
    }
}
