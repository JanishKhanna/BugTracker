using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models.ViewModels
{
    public class AssignTicketViewModel
    {
        public int TicketId { get; set; }
        public SelectList Developers { get; set; }
        public string DeveloperId { get; set; }
    }
}