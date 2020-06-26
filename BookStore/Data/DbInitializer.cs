
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookStore.Utility;
using BookStore.Models;

namespace BookStore.Data
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task Initialize()
        {
            if (_db.Database.GetPendingMigrations().Count() > 0)
            {
                _db.Database.Migrate();
            }

            if (_db.Roles.Any(r => r.Name == SD.SuperAdminEndUser)) return;

            _roleManager.CreateAsync(new IdentityRole(SD.AdminEndUser)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.SuperAdminEndUser)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.ManagerEndUser)).GetAwaiter().GetResult();
            _userManager.CreateAsync(new ApplicationUser
            {
                UserName = "yasuo12091999@gmail.com",
                Email = "yasuo12091999@gmail.com",
                Name = "Thanh",
                PhoneNumber = "0991238192",
                Address = "Thủ đức",
                EmailConfirmed = true,
                Pass = "Thanhpro1@"
            }, "Thanhpro1@").GetAwaiter().GetResult();
            IdentityUser usertodo = await _db.Users.Where(u => u.Email == "yasuo12091999@gmail.com").FirstOrDefaultAsync();
            await _userManager.AddToRoleAsync(usertodo, SD.SuperAdminEndUser);
        }


    }
}
