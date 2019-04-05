﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModels
{
    public class ProjectViewModel
    {
        public int ProjectId { get; set; }
        public string Name { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
    }
}