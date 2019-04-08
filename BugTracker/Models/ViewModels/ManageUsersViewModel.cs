using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModels
{
    public class ManageUsersViewModel
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public List<IdentityRole> UserRoles { get; set; }        
    }
}