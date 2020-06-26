using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.Models
{
    public class ApplicationUser:IdentityUser 
    {
        [Display(Name = "Tên")]
        public string Name { get; set; }
        [Display(Name="Địa chỉ")]
        public string Address { get; set; }
        public string Pass { get; set; }
        [NotMapped]
        public bool IsSuperAdmin { get; set; }    
        [NotMapped]
        public bool UpdateAndInsertRoleOn { get; set; }
        [NotMapped]
        public bool DeleteRoleOn { get; set; }
        [NotMapped]
        public bool RevokeOn { get; set; }

    }
}
