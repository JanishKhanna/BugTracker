using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModels
{
    public class TicketViewModel
    {
        public int TicketId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }

        public string TicketType { get; set; }
        public string TicketPriority { get; set; }
        public string TicketStatus { get; set; }
        public string Project { get; set; }
    }
}