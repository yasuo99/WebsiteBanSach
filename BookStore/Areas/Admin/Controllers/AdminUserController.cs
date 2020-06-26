using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookStore.Data;
using BookStore.Models;
using BookStore.Models.ViewModel;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BookStore.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.SuperAdminEndUser)]
    [Area("Admin")]
    public class AdminUserController : Controller
    {

        private readonly ApplicationDbContext _db;
        private int PageSize = 10;
        [BindProperty]
        public ApplicationUserViewModel UserVM { get; set; }
        public AdminUserController(ApplicationDbContext db)
        {
            _db = db;
            UserVM = new ApplicationUserViewModel()
            {
                ApplicationUser = new ApplicationUser()
            };
        }
        public IActionResult Index(int productPage = 1)
        {
            ApplicationUserViewModel ApplicationUserVM = new ApplicationUserViewModel()
            {
                ApplicationUsers = new List<Models.ApplicationUser>()
            };
            StringBuilder param = new StringBuilder();

            param.Append("/Admin/AdminUser?productPage=:");
            ApplicationUserVM.ApplicationUsers = _db.ApplicationUsers.ToList();

            var count = ApplicationUserVM.ApplicationUsers.Count;
            ApplicationUserVM.ApplicationUsers = ApplicationUserVM.ApplicationUsers.Skip((productPage - 1) * PageSize).Take(PageSize).ToList();
            ApplicationUserVM.PagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItems = count,
                urlParam = param.ToString()
            };

            return View(ApplicationUserVM);
        }

        //Get Edit
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || id.Trim().Length == 0)
            {
                return NotFound();
            }

            var userFromDb = await _db.ApplicationUsers.FindAsync(id);

            if (userFromDb == null)
            {
                return NotFound();
            }
            List<RoleOnDatabaseViewModel> listrole = new List<RoleOnDatabaseViewModel>();
            RoleOnDatabaseViewModel role = new RoleOnDatabaseViewModel();
            role.TableName = "Authors";
            role.IsSelected = false;
            RoleOnDatabaseViewModel role1 = new RoleOnDatabaseViewModel();
            role1.TableName = "Books";
            role1.IsSelected = false;
            RoleOnDatabaseViewModel role2 = new RoleOnDatabaseViewModel();
            role2.TableName = "Orders";
            role2.IsSelected = false;
            RoleOnDatabaseViewModel role3 = new RoleOnDatabaseViewModel();
            role3.TableName = "Reviews";
            role3.IsSelected = false;
            RoleOnDatabaseViewModel role4 = new RoleOnDatabaseViewModel();
            role4.TableName = "Publishers";
            role4.IsSelected = false;
            RoleOnDatabaseViewModel role5 = new RoleOnDatabaseViewModel();
            role5.TableName = "Discounts";
            role5.IsSelected = false;
            RoleOnDatabaseViewModel role6 = new RoleOnDatabaseViewModel();
            role6.TableName = "Banners";
            role6.IsSelected = false;
            RoleOnDatabaseViewModel role7 = new RoleOnDatabaseViewModel();
            role7.TableName = "SpecialTags";
            role7.IsSelected = false;
            RoleOnDatabaseViewModel role8 = new RoleOnDatabaseViewModel();
            role8.TableName = "Shippers";
            role8.IsSelected = false;
            RoleOnDatabaseViewModel role9 = new RoleOnDatabaseViewModel();
            role9.TableName = "BooksGenrers";
            role9.IsSelected = false;
            listrole.Add(role);
            listrole.Add(role1);
            listrole.Add(role2);
            listrole.Add(role3);
            listrole.Add(role4);
            listrole.Add(role5);
            listrole.Add(role6);
            listrole.Add(role7);
            listrole.Add(role8);
            listrole.Add(role9);
            var roleindatabase = _db.ManagerRoleOnDatabases.Where(u => u.ApplicationUserId == id).ToList();
            UserVM.ApplicationUser = userFromDb;
            List<RoleOnDatabaseViewModel> managerrole = new List<RoleOnDatabaseViewModel>();
            foreach(var item in roleindatabase)
            {
                RoleOnDatabaseViewModel roletemp = new RoleOnDatabaseViewModel()
                {
                    TableName = item.TablesName,
                    IsSelected = true
                };
                if(item.Role == "Update & Insert")
                    UserVM.ApplicationUser.UpdateAndInsertRoleOn = true;
                if (item.Role == "Delete")
                    UserVM.ApplicationUser.DeleteRoleOn = true;
                foreach(var i in listrole)
                {
                    if(i.TableName == item.TablesName)
                    {
                        i.IsSelected = true;
                    }
                }
            }           
            UserVM.RoleOnDatabaseViewModels = listrole;
            return View(UserVM);
        }


        //Post Edit
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(string id)
        {
            if (ModelState.IsValid)
            {
                const string strConnectionString = "Server=DESKTOP-1OCF1PV;Database=WebsiteBanSach;Trusted_Connection=True";
                ApplicationUser userFromDb = _db.ApplicationUsers.Where(u => u.Id == id).FirstOrDefault();
                userFromDb.Name = UserVM.ApplicationUser.Name;
                userFromDb.PhoneNumber = UserVM.ApplicationUser.PhoneNumber;
                if (UserVM.ApplicationUser.UpdateAndInsertRoleOn)
                {
                    int count = 0;
                    foreach (var item in UserVM.RoleOnDatabaseViewModels)
                    {

                        if (item.IsSelected == true)
                        {
                            count++;
                            using (SqlConnection con = new SqlConnection(strConnectionString))
                            {
                                SqlCommand command = new SqlCommand("Grant Update On Object::" + item.TableName + " To " + userFromDb.Name, con);
                                SqlCommand command1 = new SqlCommand("Grant Insert On Object::" + item.TableName + " To " + userFromDb.Name, con);
                                if (con.State == ConnectionState.Closed)
                                    con.Open();
                                command.ExecuteNonQuery();
                                command1.ExecuteNonQuery();
                                con.Close();
                            }
                            var alreadyintable = _db.ManagerRoleOnDatabases.Where(u => u.ApplicationUserId == userFromDb.Id && u.TablesName == item.TableName && u.Role == "Update & Insert").FirstOrDefault();
                            if (alreadyintable == null)
                            {
                                ManagerRoleOnDatabase manager = new ManagerRoleOnDatabase();
                                manager.ApplicationUserId = userFromDb.Id;
                                manager.TablesName = item.TableName;
                                manager.Role = "Update & Insert";
                                _db.Add(manager);
                                await _db.SaveChangesAsync();
                            }
                        }
                        else
                        {
                            using (SqlConnection con = new SqlConnection(strConnectionString))
                            {
                                SqlCommand command = new SqlCommand("Deny Update On Object::" + item.TableName + " To " + userFromDb.Name, con);
                                SqlCommand command1 = new SqlCommand("Deny Insert On Object::" + item.TableName + " To " + userFromDb.Name, con);
                                if (con.State == ConnectionState.Closed)
                                    con.Open();
                                command.ExecuteNonQuery();
                                command1.ExecuteNonQuery();
                                con.Close();
                            }
                            var manager = _db.ManagerRoleOnDatabases.Where(u => u.ApplicationUserId == userFromDb.Id && u.TablesName == item.TableName && u.Role == "Update & Insert").FirstOrDefault();
                            if(manager != null)
                            {
                                _db.Remove(manager);
                            }
                            await _db.SaveChangesAsync();
                        }
                    }
                    if (count == 0)
                    {
                        foreach (var item in UserVM.RoleOnDatabaseViewModels)
                        {
                            using (SqlConnection con = new SqlConnection(strConnectionString))
                            {
                                SqlCommand command = new SqlCommand("Grant Update On Object::" + item.TableName + " To " + userFromDb.Name, con);
                                SqlCommand command1 = new SqlCommand("Grant Insert On Object::" + item.TableName + " To " + userFromDb.Name, con);
                                if (con.State == ConnectionState.Closed)
                                    con.Open();
                                command.ExecuteNonQuery();
                                command1.ExecuteNonQuery();
                                con.Close();
                            }
                            var alreadyintable = _db.ManagerRoleOnDatabases.Where(u => u.ApplicationUserId == userFromDb.Id && u.TablesName == item.TableName && u.Role == "Update & Insert").FirstOrDefault();
                            if (alreadyintable == null)
                            {
                                ManagerRoleOnDatabase manager = new ManagerRoleOnDatabase();
                                manager.ApplicationUserId = userFromDb.Id;
                                manager.TablesName = item.TableName;
                                manager.Role = "Update & Insert";
                                _db.Add(manager);
                                await _db.SaveChangesAsync();
                            }
                        }
                    }
                }
                else
                {
                    foreach (var item in UserVM.RoleOnDatabaseViewModels)
                    {
                        using (SqlConnection con = new SqlConnection(strConnectionString))
                        {
                            SqlCommand command = new SqlCommand("Deny Update On Object::" + item.TableName + " To " + userFromDb.Name, con);
                            SqlCommand command1 = new SqlCommand("Deny Insert On Object::" + item.TableName + " To " + userFromDb.Name, con);
                            if (con.State == ConnectionState.Closed)
                                con.Open();
                            command.ExecuteNonQuery();
                            command1.ExecuteNonQuery();
                            con.Close();
                        }
                        var manager = _db.ManagerRoleOnDatabases.Where(u => u.ApplicationUserId == userFromDb.Id && u.TablesName == item.TableName && u.Role == "Update & Insert").FirstOrDefault();
                        if (manager != null)
                        {
                            _db.Remove(manager);
                        }
                        await _db.SaveChangesAsync();
                    }
                }
                if (UserVM.ApplicationUser.DeleteRoleOn)
                {
                    int count = 0;
                    foreach (var item in UserVM.RoleOnDatabaseViewModels)
                    {
                        if (item.IsSelected == true)
                        {
                            count++;
                            using (SqlConnection con = new SqlConnection(strConnectionString))
                            {
                                SqlCommand command = new SqlCommand("Grant Delete On Object::" + item.TableName + " To " + userFromDb.Name, con);
                                if (con.State == ConnectionState.Closed)
                                    con.Open();
                                command.ExecuteNonQuery();
                                con.Close();
                            }
                            var alreadyintable = _db.ManagerRoleOnDatabases.Where(u => u.ApplicationUserId == userFromDb.Id && u.TablesName == item.TableName && u.Role == "Delete").FirstOrDefault();
                            if (alreadyintable == null)
                            {
                                ManagerRoleOnDatabase manager = new ManagerRoleOnDatabase();
                                manager.ApplicationUserId = userFromDb.Id;
                                manager.TablesName = item.TableName;
                                manager.Role = "Delete";
                                _db.Add(manager);
                                await _db.SaveChangesAsync();
                            }
                        }
                        else
                        {
                            using (SqlConnection con = new SqlConnection(strConnectionString))
                            {
                                SqlCommand command = new SqlCommand("Deny Delete On Object::" + item.TableName + " To " + userFromDb.Name, con);
                                if (con.State == ConnectionState.Closed)
                                    con.Open();
                                command.ExecuteNonQuery();
                                con.Close();
                            }
                            var manager = _db.ManagerRoleOnDatabases.Where(u => u.ApplicationUserId == userFromDb.Id && u.TablesName == item.TableName && u.Role == "Delete").FirstOrDefault();
                            if (manager != null)
                            {
                                _db.Remove(manager);
                            }
                            await _db.SaveChangesAsync();
                        }
                    }
                    if (count == 0)
                    {
                        foreach (var item in UserVM.RoleOnDatabaseViewModels)
                        {
                            using (SqlConnection con = new SqlConnection(strConnectionString))
                            {
                                SqlCommand command = new SqlCommand("Grant Detele On Object::" + item.TableName + " To " + userFromDb.Name, con);
                                if (con.State == ConnectionState.Closed)
                                    con.Open();
                                command.ExecuteNonQuery();
                                con.Close();
                            }
                            var alreadyintable = _db.ManagerRoleOnDatabases.Where(u => u.ApplicationUserId == userFromDb.Id && u.TablesName == item.TableName && u.Role == "Delete").FirstOrDefault();
                            if (alreadyintable == null)
                            {
                                ManagerRoleOnDatabase manager = new ManagerRoleOnDatabase();
                                manager.ApplicationUserId = userFromDb.Id;
                                manager.TablesName = item.TableName;
                                manager.Role = "Delete";
                                _db.Add(manager);
                                await _db.SaveChangesAsync();
                            }
                        }
                    }
                }
                else
                {
                    foreach (var item in UserVM.RoleOnDatabaseViewModels)
                    {
                        using (SqlConnection con = new SqlConnection(strConnectionString))
                        {
                            SqlCommand command = new SqlCommand("Deny Delete On Object::" + item.TableName + " To " + userFromDb.Name, con);
                            if (con.State == ConnectionState.Closed)
                                con.Open();
                            command.ExecuteNonQuery();
                            con.Close();
                        }
                        var manager = _db.ManagerRoleOnDatabases.Where(u => u.ApplicationUserId == userFromDb.Id && u.TablesName == item.TableName && u.Role == "Delete").FirstOrDefault();
                        if (manager != null)
                        {
                            _db.Remove(manager);
                        }
                        await _db.SaveChangesAsync();
                    }
                }
                if (UserVM.ApplicationUser.RevokeOn)
                {
                    foreach (var item in UserVM.RoleOnDatabaseViewModels)
                    {
                        using (SqlConnection con = new SqlConnection(strConnectionString))
                        {
                            SqlCommand command = new SqlCommand("Revoke On Object::" + item.TableName + " from " + userFromDb.Name, con);
                            if (con.State == ConnectionState.Closed)
                                con.Open();
                            command.ExecuteNonQuery();
                            con.Close();
                        }
                        var manager = _db.ManagerRoleOnDatabases.Where(u => u.ApplicationUserId == userFromDb.Id && u.TablesName == item.TableName).FirstOrDefault();
                        if (manager != null)
                        {
                            _db.Remove(manager);
                        }
                        await _db.SaveChangesAsync();
                    }
                }
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View();
        }


        //Get Delete
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || id.Trim().Length == 0)
            {
                return NotFound();
            }

            var userFromDb = await _db.ApplicationUsers.FindAsync(id);
            if (userFromDb == null)
            {
                return NotFound();
            }

            return View(userFromDb);
        }


        //Post Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(string id)
        {
            ApplicationUser userFromDb = _db.ApplicationUsers.Where(u => u.Id == id).FirstOrDefault();
            userFromDb.LockoutEnd = DateTime.Now.AddYears(1000);

            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}