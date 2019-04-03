﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.Domain
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<ApplicationUser> ApplicationUsers { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
    }
}