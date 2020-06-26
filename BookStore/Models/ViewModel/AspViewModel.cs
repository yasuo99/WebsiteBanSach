using BookStore.Areas.Admin.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models.ViewModel
{
    public class AspViewModel
    {
        public string Area { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
    }
}
