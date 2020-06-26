using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MailChimp;
using MailChimp.Net.Interfaces;
using MailChimp.Net.Models;
namespace BookStore.Areas.Customer.Controllers
{
    public class MailChimpController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
