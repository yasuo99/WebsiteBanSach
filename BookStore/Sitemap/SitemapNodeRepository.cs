using BookStore.Models.ViewModel;
using BookStore.Sitemap.Biz;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Sitemap
{
    public class SitemapNodeRepository: ISitemapNodeRepository
    {
        public string SetSitemapNodes(IUrlHelper urlHelper, string path,BooksViewModel BooksVM)
        {
            return clsSitemap.SetSitemap(urlHelper, path,BooksVM);
        }
    }
}
