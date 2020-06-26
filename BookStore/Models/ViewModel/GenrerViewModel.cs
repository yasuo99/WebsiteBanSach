using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models.ViewModel
{
    public class GenrerViewModel
    {
        public Genrer Genrer { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public List<Genrer> Genrers { get; set; }
        
    }
}
