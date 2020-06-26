using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models.ViewModel
{
    public class AuthorsViewModel
    {
        public Author Author { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public List<Author> Authors { get; set; }
        public string TriggerError { get; set; }
        public string PermissionError { get; set; }
        public List<Book> Books { get; set; }
    }
}
