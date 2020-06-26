using BookStore.Models.ViewModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace BookStore.Sitemap.Biz
{
    public static class clsSitemap
    {
        public static string SetSitemap(IUrlHelper urlHelper, string path,BooksViewModel BooksVM)
        {
            string xml = GetSitemapDocument(GetNode(urlHelper,BooksVM));
            XmlDocument document = new XmlDocument();
            document.LoadXml(xml);
            document.Save(path);
            return xml;
        }
        private static string GetSitemapDocument(List<SitemapNode> nodes)
        {
            XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            XElement root = new XElement(xmlns + "urlset");
            foreach(var node in nodes)
            {
                XElement urlElement = new XElement(
                    xmlns + "url", new XElement(xmlns + "loc", Uri.EscapeUriString(node.Url)),
                    node.LastModified == null ? null : new XElement(xmlns + "lastmod", node.LastModified.Value.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:sszzz")),
                    node.Frequency == null ? null : new XElement(xmlns + "changefreq", node.Frequency.Value.ToString().ToLower()),
                    node.Priority == null ? null : new XElement(xmlns + "priority", node.Priority.Value.ToString("F1", CultureInfo.InvariantCulture)));
                root.Add(urlElement);
            }
            XDocument document = new XDocument(root);
            return document.ToString();
        }

        private static List<SitemapNode> GetNode(IUrlHelper urlHelper, BooksViewModel BooksVM)
        {
            var books = BooksVM.Books;
            List<SitemapNode> nodes = new List<SitemapNode>();
            string domain = GetRawUrl(urlHelper.ActionContext.HttpContext.Request);
            var controllers = Assembly.GetExecutingAssembly().GetTypes()
                   .Where(type => typeof(Controller).IsAssignableFrom(type)
                   || type.Name.EndsWith("controller")).ToList();
            string url = domain.Remove(domain.LastIndexOf("/"));
            foreach(var book in books)
            {
                    nodes.Add(new SitemapNode()
                    {
                        Url = url + urlHelper.Action("Details", "Home",new {id = book.ID}),
                        Frequency = SitemapFrequency.Daily,
                        LastModified = DateTime.Now,
                        Priority = new Random().NextDouble()
                    });
            }
            return nodes;
        }
        public static string GetRawUrl(HttpRequest request)
        {
            var httpContext = request.HttpContext;
            return $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}{httpContext.Request.QueryString}";
        }
    }
}
