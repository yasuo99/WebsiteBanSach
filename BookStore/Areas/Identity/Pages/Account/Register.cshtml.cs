using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BookStore.Models;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace BookStore.Areas.Identity.Pages.Account
{
    //[Authorize(Roles = SD.SuperAdminEndUser)]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public RegisterModel(
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            public string Name { get; set; }
            [Required]
            [StringLength(150)]
            [Display(Name = "Địa chỉ")]
            public string Address { get; set; }
            [Required]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }


            [Display(Name = "Super Admin")]
            public bool IsSuperAdmin { get; set; }

            [Display(Name = "Manager")]
            public bool IsManager { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email, Name = Input.Name, PhoneNumber = Input.PhoneNumber, Address = Input.Address ,Pass = Input.Password};
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync(SD.AdminEndUser))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(SD.AdminEndUser));
                    }
                    if (!await _roleManager.RoleExistsAsync(SD.SuperAdminEndUser))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(SD.SuperAdminEndUser));
                    }
                    if (!await _roleManager.RoleExistsAsync(SD.ManagerEndUser))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(SD.ManagerEndUser));
                    }

                    if (Input.IsSuperAdmin)
                    {
                        await _userManager.AddToRoleAsync(user, SD.SuperAdminEndUser);
                    }
                    else if (Input.IsManager)
                    {
                        await _userManager.AddToRoleAsync(user, SD.ManagerEndUser);
                        CreateUserDatabase(Input.Email, Input.Password);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, SD.AdminEndUser);
                    }


                    _logger.LogInformation("User created a new account with password.");

                    
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    return RedirectToPage("Login");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
        static string strConnectionString = "Server=DESKTOP-1OCF1PV;Database=WebsiteBanSach;Trusted_Connection=True";
        public void CreateUserDatabase(string username, string password)
        {
            using (SqlConnection con = new SqlConnection(strConnectionString))
            {
                SqlCommand command = new SqlCommand("CREATE LOGIN " + '"' + username + '"' + " WITH PASSWORD = '" + password +"' ; create user " + '"' + username + '"' + " for login " + '"' + username + '"', con);
                SqlCommand command2 = new SqlCommand("Alter Role db_datareader add member " + '"' + username + '"', con);
                if (con.State == ConnectionState.Closed)
                    con.Open();
                command.ExecuteNonQuery();
                command2.ExecuteNonQuery();
                con.Close();
                _logger.LogInformation("User created a new account with password.");
            }
        }
    }
}
