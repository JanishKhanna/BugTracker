using BugTracker.Models.Domain;
using BugTracker.Models.MyHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModels
{
    public class TicketDetailsViewModel
    {
        public int TicketId { get; set; }
        public string ProjectName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string TypesOfTicket { get; set; }
        public string TypesOfPriority { get; set; }
        public string TypesOfStatus { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }

        public virtual ApplicationUser OwnerUser { get; set; }
        public string OwnerUserId { get; set; }

        public virtual ApplicationUser AssignedToUser { get; set; }
        public string AssignedToUserId { get; set; }

        public List<CommentViewModel> AllComments { get; set; }    
        public List<TicketHistoryViewModel> TicketHistories { get; set; }
    }
}