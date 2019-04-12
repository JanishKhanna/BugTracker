using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.Domain
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }

        public Project Project { get; set; }
        public int ProjectId { get; set; }

        public TicketType TicketType { get; set; }
        public int TicketTypeId { get; set; }

        public TicketPriority TicketPriority { get; set; }
        public int TicketPriorityId { get; set; }

        public TicketStatus TicketStatus { get; set; }
        public int TicketStatusId { get; set; }

        public virtual List<TicketComment> TicketComments { get; set; }
        
        public virtual List<TicketAttachment> TicketAttachments { get; set; }

        public ApplicationUser OwnerUser { get; set; }
        public string OwnerUserId { get; set; }
        
        public ApplicationUser AssignedToUser { get; set; }
        public string AssignedToUserId { get; set; }

        public Ticket()
        {
            DateCreated = DateTime.Now;
            TicketComments = new List<TicketComment>();
            TicketAttachments = new List<TicketAttachment>();
        }
    }
}