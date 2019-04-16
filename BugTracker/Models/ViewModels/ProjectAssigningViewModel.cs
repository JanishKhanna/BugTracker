using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models.ViewModels
{
    public class ProjectAssigningViewModel
    {
        public int ProjectId { get; set; }
        public string Tickets { get; set; }
        public MultiSelectList AddUser { get; set; }
        public MultiSelectList DeleteUser { get; set; }
        public string[] SelectedAddUsers { get; set; } = new string[] { };
        public string[] SelectedDeleteUsers { get; set; } = new string[] { };
    }
}