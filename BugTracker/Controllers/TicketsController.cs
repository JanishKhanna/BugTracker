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
        [Authorize(Roles = nameof(Roles.Admin) + "," + nameof(Roles.ProjectManager))]
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
                    OwnerUser = p.OwnerUser,
                    //AssignedToUser = ,
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
            var Projects = helper.GetUsersProjects(userId);
            var projectSelectList = new SelectList(Projects, nameof(Project.Id), nameof(Project.Name));
            var viewModel = new CreateEditTicketViewModel
            {
                Projects = projectSelectList
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
                                    .First(p => p.Id == formData.TicketTypeId),
                    TicketPriority = DbContext.TicketPriorities
                                        .First(p => p.Id == formData.TicketPriorityId),
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
            myTicket.ProjectId = formData.ProjectId;
            myTicket.TicketTypeId = formData.TicketTypeId;
            myTicket.TicketPriorityId = formData.TicketPriorityId;
            myTicket.TicketStatusId = formData.TicketStatusId;

            DbContext.SaveChanges();

            return RedirectToAction(nameof(TicketsController.AllTickets));
        }

        [HttpGet]
        [Authorize]
        public ActionResult EditTicket(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction(nameof(TicketsController.AllTickets));
            }

            var myTicket = DbContext.Tickets.FirstOrDefault(p => p.Id == id);

            if (myTicket == null)
            {
                return RedirectToAction(nameof(TicketsController.AllTickets));
            }

            var helper = new ProjectHelper(DbContext);
            var userId = User.Identity.GetUserId();
            var ticketTypeSelectList = new SelectList(DbContext.TicketTypes, nameof(TicketType.Id), nameof(TicketType.Name));
            var ticketPrioritySelectList = new SelectList(DbContext.TicketPriorities, nameof(TicketPriority.Id), nameof(TicketPriority.Name));
            var ticketStatusSelectList = new SelectList(DbContext.TicketStatuses, nameof(TicketStatus.Id), nameof(TicketStatus.Name));
            var Projects = helper.GetUsersProjects(userId);
            var projectSelectList = new SelectList(Projects, nameof(Project.Id), nameof(Project.Name));
            var viewModel = new CreateEditTicketViewModel
            {
                Title = myTicket.Title,
                Projects = projectSelectList,
                Description = myTicket.Description,
                ProjectId = myTicket.ProjectId,
                TicketPriority = ticketPrioritySelectList,
                TicketType = ticketTypeSelectList,
                TicketStatus = ticketStatusSelectList,
                TicketTypeId = myTicket.TicketTypeId,
                TicketPriorityId = myTicket.TicketPriorityId,
                TicketStatusId = myTicket.TicketStatusId
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        public ActionResult EditTicket(int id, CreateEditTicketViewModel formData)
        {
            return SaveTicket(id, formData);
        }

        [HttpGet]
        public ActionResult Details(int? id)
        {
            var myticket = DbContext.Tickets.FirstOrDefault(p => p.Id == id);

            if (!id.HasValue)
            {
                return RedirectToAction(nameof(TicketsController.AllTickets));
            }

            if (myticket == null)
            {
                return RedirectToAction(nameof(TicketsController.AllTickets));
            }

            var viewModel = new TicketDetailsViewModel
            {                
                TicketId = myticket.Id,
                Title = myticket.Title,
                Description = myticket.Description,
                ProjectName = myticket.Project.Name,
                TypesOfTicket = myticket.TicketType.Name,
                TypesOfStatus = myticket.TicketStatus.Name,
                TypesOfPriority = myticket.TicketPriority.Name,
                DateCreated = myticket.DateCreated,
                DateUpdated = myticket.DateUpdated,                
                AllComments = myticket.TicketComments.Select(p => new CommentViewModel()
                {
                    CommentId = p.Id,
                    Comment = p.Comment,
                    TicketId = p.TicketId,
                    DateCreated = p.DateCreated,
                    DateUpdated = p.DateUpdated,   
                    User = p.User                    
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        [Authorize]
        public ActionResult CreateComments()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public ActionResult CreateComments(int id, CreateCommentViewModel formData)
        {
            formData.TicketId = id;
            return SaveComments(null, formData);
        }

        private ActionResult SaveComments(int? id, CreateCommentViewModel formData)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            TicketComment myComment;

            if (!id.HasValue)
            {
                var userId = User.Identity.GetUserId();
                var comment = DbContext.Tickets.FirstOrDefault(p => p.Id == formData.TicketId);

                if (comment == null)
                {
                    return RedirectToAction(nameof(TicketsController.AllTickets));
                }

                myComment = new TicketComment()
                {
                    DateCreated = DateTime.Now,
                    User = DbContext.Users.FirstOrDefault(p => p.Id == userId),
                };

                DbContext.AllComments.Add(myComment);
                comment.TicketComments.Add(myComment);
            }
            else
            {
                myComment = DbContext.AllComments.FirstOrDefault(p => p.Id == id);

                if (myComment == null)
                {
                    return RedirectToAction(nameof(TicketsController.AllTickets));
                }                
            }

            myComment.Comment = formData.Comment;
            myComment.DateUpdated = DateTime.Now;

            DbContext.SaveChanges();
            return RedirectToAction(nameof(TicketsController.AllTickets));
        }
    }
}