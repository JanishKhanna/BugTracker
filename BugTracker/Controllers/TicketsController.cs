using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.MyHelpers;
using BugTracker.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class TicketsController : Controller
    {
        private ApplicationDbContext DbContext;
        private TicketHelper TicketHelper;

        public TicketsController()
        {
            DbContext = new ApplicationDbContext();
            TicketHelper = new TicketHelper(DbContext);
        }

        //GET: Tickets
        public ActionResult Index()
        {
            var viewModel = TicketHelper.GetAllTickets()
                .Select(p => new TicketViewModel
                {
                    TicketId = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    DateCreated = p.DateCreated,
                    DateUpdated = p.DateUpdated,
                    TicketType = DbContext.TicketTypes.First(type => type.Id == p.TicketTypeId).Name,
                    TicketPriority = DbContext.TicketPriorities.First(priority => priority.Id == p.TicketPriorityId).Name,
                    TicketStatus = DbContext.TicketStatuses.First(status => status.Id == p.TicketStatusId).Name
                });

            return View(viewModel);
        }

        [HttpGet]
        [Authorize(Roles = nameof(Roles.Submitter))]
        public ActionResult CreateTicket()
        {           

            return View();
        }

        [HttpPost]
        [Authorize(Roles = nameof(Roles.Submitter))]
        public ActionResult CreateTicket(CreateEditTicketViewModel formData)
        {
            return SaveTicket(null, formData);
        }

        private ActionResult SaveTicket(int? id, CreateEditTicketViewModel formData)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            Ticket myTicket;

            if (!id.HasValue)
            {
                var userId = User.Identity.GetUserId();

                myTicket = new Ticket()
                {                    
                    //DateCreated = DateTime.Now,
                    //OwnerUser = DbContext.Users
                    //                .Where(p => p.Id == userId)
                    //                .ToList()
                    
                    //ApplicationUsers = DbContext.Users
                    //                    .Where(p => p.Id == userId)
                    //                    .ToList() ?? new List<ApplicationUser>(),
                };
                DbContext.Tickets.Add(myTicket);
            }
            else
            {
                myTicket = DbContext.Tickets.FirstOrDefault(p => p.Id == id);

                if (myTicket == null)
                {
                    return RedirectToAction(nameof(HomeController.Index));
                }
                myTicket.DateUpdated = DateTime.Now;
            }

            myTicket.Title = formData.Title;
            myTicket.Description = formData.Description;

            DbContext.SaveChanges();

            return RedirectToAction(nameof(HomeController.Index));
        }
    }
}