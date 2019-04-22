using BugTracker.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.MyHelpers
{
    public class TicketHelper
    {
        private ApplicationDbContext DbContext;

        public TicketHelper(ApplicationDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public List<Ticket> GetAllTickets() => DbContext.Tickets.ToList();

        public Ticket GetTicketById(int id)
        {
            return DbContext.Tickets.FirstOrDefault(
                p => p.Id == id);
        }
    }
}