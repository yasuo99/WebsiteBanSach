using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models
{
    public class CustomerAddress
    {
        public int ID { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

    }
}
