using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModels
{
    public class AddRoleViewModel
    {
        public string UserId { get; set; }
        public List<String> RolesToAdd { get; set; }
    }
}