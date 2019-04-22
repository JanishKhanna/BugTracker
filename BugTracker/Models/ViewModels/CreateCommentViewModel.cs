using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModels
{
    public class CreateCommentViewModel
    {
        public int TicketId { get; set; }

        [Required]
        public string Comment { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}