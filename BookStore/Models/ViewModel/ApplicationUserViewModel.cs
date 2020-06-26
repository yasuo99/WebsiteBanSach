using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models.ViewModel
{
    public class ApplicationUserViewModel
    {
        public ApplicationUser ApplicationUser { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public List<ApplicationUser> ApplicationUsers { get; set; }
        public List<RoleOnDatabaseViewModel> RoleOnDatabaseViewModels { get; set; }
    }
}
