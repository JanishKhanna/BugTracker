using System;

namespace BugTracker.Models.ViewModels
{
    public class CommentViewModel
    {
        public int CommentId { get; set; }
        public int TicketId { get; set; }
        public string Comment { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public ApplicationUser User { get; set; }
    }
}