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
        public string TicketType { get; set; }

        [Required]
        public string TicketPriority { get; set; }

        [Required]
        public int ProjectId { get; set; }
        
        public List<SelectListItem> Projects { get; set; }      

    }
}