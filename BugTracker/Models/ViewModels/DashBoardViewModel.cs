using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModels
{
    public class DashBoardViewModel
    {
        public int Projects { get; set; }
        public int Tickets { get; set; }
        public int OpenTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int RejectedTickets { get; set; }
    }
}