using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models.ViewModels
{
    public class CreateEditTicketViewModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }
        
        [Required]
        public int TicketTypeId { get; set; }

        public SelectList TicketType { get; set; }

        [Required]
        public int TicketPriorityId { get; set; }

        public SelectList TicketPriority { get; set; }

        [Required]
        public int TicketStatusId { get; set; }

        public SelectList TicketStatus { get; set; }

        [Required]
        public int ProjectId { get; set; }
        
        public SelectList Projects { get; set; }      

    }
}