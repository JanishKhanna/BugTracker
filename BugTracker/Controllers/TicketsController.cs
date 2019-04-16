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
        public ActionResult AllTickets()
        {
            var viewModel = TicketHelper.GetAllTickets()
                .Select(p => new TicketViewModel
                {
                    Project = p.Project.Name,
                    TicketId = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    DateCreated = p.DateCreated,
                    DateUpdated = p.DateUpdated,
                    TicketType = DbContext.TicketTypes.First(type => type.Id == p.TicketTypeId).Name,
                    TicketPriority = DbContext.TicketPriorities.First(priority => priority.Id == p.TicketPriorityId).Name,
                    TicketStatus = DbContext.TicketStatuses.First(status => status.Id == p.TicketStatusId).Name
                }).ToList();

            return View(viewModel);
        }

        [HttpGet]
        [Authorize(Roles = nameof(Roles.Submitter))]
        public ActionResult CreateTicket()
        {
            var helper = new ProjectHelper(DbContext);
            var userId = User.Identity.GetUserId();
            var viewModel = new CreateEditTicketViewModel
            {
                Projects = helper.GetUsersProjects(userId)
                            .Select(p => new SelectListItem
                            {
                                Text = p.Name,
                                Value = p.Id.ToString(),
                            }).ToList()
            };

            return View(viewModel);
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
                    TicketType = DbContext.TicketTypes
                                    .First(p => p.Name == formData.TicketType),
                    TicketPriority = DbContext.TicketPriorities         
                                        .First(p => p.Name == formData.TicketPriority), 
                    Project = DbContext.Projects
                                .First(p => p.Id == formData.ProjectId),                           
                    DateCreated = DateTime.Now,
                    OwnerUser = DbContext.Users
                                    .First(p => p.Id == userId),                          
                    AssignedToUser = DbContext.Users
                                        .First(p => p.Id == userId),
                    TicketStatus = DbContext.TicketStatuses
                                    .First(p => p.Name == "Open")
                };

                DbContext.Tickets.Add(myTicket);
            }
            else
            {
                myTicket = DbContext.Tickets.FirstOrDefault(p => p.Id == id);

                if (myTicket == null)
                {
                    return RedirectToAction(nameof(TicketsController.AllTickets));
                }
                myTicket.DateUpdated = DateTime.Now;
            }

            myTicket.Title = formData.Title;
            myTicket.Description = formData.Description;

            DbContext.SaveChanges();

            return RedirectToAction(nameof(TicketsController.AllTickets));
        }        
    }
}