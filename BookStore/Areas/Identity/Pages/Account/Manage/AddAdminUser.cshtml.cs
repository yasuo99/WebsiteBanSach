using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookStore.Models;
using BookStore.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookStore.Areas.Identity.Pages.Account.Manage
{
    public class AddAdminUserModel : PageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AddAdminUserModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }
        public async Task<IActionResult> OnGet()
        {
            if (!await _roleManager.RoleExistsAsync(SD.AdminEndUser))
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.AdminEndUser));
            }
            if (!await _roleManager.RoleExistsAsync(SD.SuperAdminEndUser))
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.SuperAdminEndUser));
                ApplicationUser user = (new ApplicationUser
                {
                    UserName = "yasuo12091999@gmail.com",
                    Email = "yasuo12091999@gmail.com",
                    Name = "Thanh",
                    PhoneNumber = "0991238192",
                    Address = "Thủ đức",
                    EmailConfirmed = true
                });
                var useresult = await _userManager.CreateAsync(user, "Thanhpro1@");
                await _userManager.AddToRoleAsync(user, SD.SuperAdminEndUser);
            }
            

            return Page();
        }
    }
}
