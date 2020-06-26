using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BookStore.Models.ViewModel;
using BookStore.Sitemap;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization.Internal;

namespace BookStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RobotsController : Controller
    {
        public IWebHostEnvironment _hostEnvironment;
        [BindProperty]
        public List<RobotsViewModel> RobotsVM { get; set; }
        static readonly ISitemapNodeRepository repository = new SitemapNodeRepository();
        public RobotsController(IWebHostEnvironment hostEnvironment)
        {
            RobotsVM = new List<RobotsViewModel>();
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            string domain = GetRawUrl(Request);
            var controllers = Assembly.GetExecutingAssembly().GetTypes()
                   .Where(type => typeof(Controller).IsAssignableFrom(type)
                   || type.Name.EndsWith("controller")).ToList();
            string url = domain.Remove(domain.LastIndexOf("/"));
            string[] parentDirectory = Directory.GetDirectories(_hostEnvironment.ContentRootPath);
            List<string> directories = new List<string>();

            foreach (var directory in parentDirectory)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(directory);
                string name = dirInfo.Name;
                RobotsVM.Add(new RobotsViewModel()
                {
                    Path = @"/" + name + @"/",
                    Selected = false
                });
                if(Directory.GetDirectories(directory) != null)
                {
                    string[] subDirectory = Directory.GetDirectories(directory);
                    foreach (var sub in subDirectory)
                    {
                        DirectoryInfo subDirInfo = new DirectoryInfo(sub);
                        RobotsVM.Add(new RobotsViewModel()
                        {
                            Path = @"/" + name + @"/" + subDirInfo.Name + @"/",
                            Selected = false
                        });
                    }
                }
                directories.Add(name);
            }
            return View(RobotsVM);
        }
        [HttpPost,ActionName("CreateRobot")]
        [ValidateAntiForgeryToken]
        public IActionResult CreateRobot()
        {
            var robotsTxtPath = Path.Combine(_hostEnvironment.ContentRootPath, "robots.txt");
            string output = "User-agent: *  \n";
            foreach (var robot in RobotsVM)
            {
                if (robot.Selected)
                {
                    output += "Disallow: " + robot.Path + "\n";
                }
            }
            string[] files = System.IO.Directory.GetFiles(_hostEnvironment.WebRootPath, "*.xml");
            foreach (var file in files)
            {
                output += "Sitemap: " + file + "\n";
            }
            System.IO.File.WriteAllText(robotsTxtPath, output);
            return Redirect($"{Request.Scheme}://{Request.Host}" + "/robots.txt");
        }
        public static string GetRawUrl(HttpRequest request)
        {
            var httpContext = request.HttpContext;
            return $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}{httpContext.Request.QueryString}";
        }
    }
}