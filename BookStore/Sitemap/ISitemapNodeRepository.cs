using BookStore.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Sitemap
{
    interface ISitemapNodeRepository
    {
        string SetSitemapNodes(IUrlHelper urlHelper, string path, BooksViewModel BooksVM);
    }
}
