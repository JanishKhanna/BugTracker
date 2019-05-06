using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.MyHelpers;
using BugTracker.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
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
        [Authorize(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
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
                    AssignedToUser = p?.AssignedToUser,
                    TicketType = DbContext.TicketTypes.First(type => type.Id == p.TicketTypeId).Name,
                    TicketPriority = DbContext.TicketPriorities.First(priority => priority.Id == p.TicketPriorityId).Name,
                    TicketStatus = DbContext.TicketStatuses.First(status => status.Id == p.TicketStatusId).Name
                }).ToList();

            return View(viewModel);
        }

        private bool CanEdit(string userId, Ticket myTicket)
        {
            var myCurrentUser = DbContext.Users.FirstOrDefault(p => p.Id == userId);

            if (myCurrentUser == null)
            {
                return false;
            }

            string userEmail = myCurrentUser.Email;
            bool isAdminOrManager = User.IsInRole(nameof(UserRoles.Admin)) || User.IsInRole(nameof(UserRoles.ProjectManager));
            bool isSubmitter = User.IsInRole(nameof(UserRoles.Submitter));
            bool isDeveloper = User.IsInRole(nameof(UserRoles.Developer));
            bool canEdit = isAdminOrManager || (isSubmitter && userEmail == myTicket.OwnerUser.Email) || (myTicket.AssignedToUser != null && isDeveloper && userEmail == myTicket.AssignedToUser.Email);

            return canEdit;
        }

        private bool CanView(string userId, Ticket myTicket)
        {
            var user = DbContext.Users.FirstOrDefault(p => p.Id == userId);
            if (user == null)
            {
                return false;
            }

            bool canView = CanEdit(userId, myTicket) || user.Projects.Any(p => p.Id == myTicket.ProjectId);
            return canView;
        }

        [Authorize]
        public ActionResult MyTickets()
        {
            var userId = User.Identity.GetUserId();
            var myCurrentUser = DbContext.Users.FirstOrDefault(p => p.Id == userId);

            if (myCurrentUser == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            bool isSubmitter = User.IsInRole(nameof(UserRoles.Submitter));
            bool isDeveloper = User.IsInRole(nameof(UserRoles.Developer));
            string userEmail = myCurrentUser.Email;

            var viewModel = DbContext.Tickets.ToList()
                            .Where(myTicket => (isSubmitter && userEmail == myTicket.OwnerUser.Email) || (myTicket.AssignedToUser != null && isDeveloper && userEmail == myTicket.AssignedToUser.Email) || myCurrentUser.Projects.Any(p => p.Id == myTicket.ProjectId))
                            .Select(p => new TicketViewModel
                            {
                                Project = p.Project.Name,
                                TicketId = p.Id,
                                Title = p.Title,
                                Description = p.Description,
                                DateCreated = p.DateCreated,
                                DateUpdated = p.DateUpdated,
                                OwnerUser = p.OwnerUser,
                                AssignedToUser = p?.AssignedToUser,
                                TicketType = DbContext.TicketTypes.FirstOrDefault(type => type.Id == p.TicketTypeId).Name,
                                TicketPriority = DbContext.TicketPriorities.FirstOrDefault(priority => priority.Id == p.TicketPriorityId).Name,
                                TicketStatus = DbContext.TicketStatuses.FirstOrDefault(status => status.Id == p.TicketStatusId).Name
                            }).ToList();

            return View(viewModel);
        }

        [HttpGet]
        [Authorize(Roles = nameof(UserRoles.Submitter))]
        public ActionResult CreateTicket()
        {
            var helper = new ProjectHelper(DbContext);
            var userId = User.Identity.GetUserId();
            var projects = helper.GetUsersProjects(userId);
            var projectSelectList = new SelectList(projects, nameof(Project.Id), nameof(Project.Name));
            var ticketTypeSelectList = new SelectList(DbContext.TicketTypes, nameof(TicketType.Id), nameof(TicketType.Name));
            var ticketPrioritySelectList = new SelectList(DbContext.TicketPriorities, nameof(TicketPriority.Id), nameof(TicketPriority.Name));
            var ticketStatusSelectList = new SelectList(DbContext.TicketStatuses, nameof(TicketStatus.Id), nameof(TicketStatus.Name));
            var viewModel = new CreateEditTicketViewModel
            {
                Projects = projectSelectList,
                TicketType = ticketTypeSelectList,
                TicketPriority = ticketPrioritySelectList,
                TicketStatus = ticketStatusSelectList
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRoles.Submitter))]
        public ActionResult CreateTicket(CreateEditTicketViewModel formData)
        {
            var helper = new ProjectHelper(DbContext);
            var userId = User.Identity.GetUserId();
            var projects = helper.GetUsersProjects(userId);

            if (projects == null || projects.Count < 1)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            return SaveTicket(null, formData);
        }

        private ActionResult SaveTicket(int? id, CreateEditTicketViewModel formData)
        {
            var userId = User.Identity.GetUserId();

            if (!ModelState.IsValid)
            {
                var helper = new ProjectHelper(DbContext);
                var projects = helper.GetUsersProjects(userId);
                var projectSelectList = new SelectList(projects, nameof(Project.Id), nameof(Project.Name));
                var ticketTypeSelectList = new SelectList(DbContext.TicketTypes, nameof(TicketType.Id), nameof(TicketType.Name));
                var ticketPrioritySelectList = new SelectList(DbContext.TicketPriorities, nameof(TicketPriority.Id), nameof(TicketPriority.Name));
                var ticketStatusSelectList = new SelectList(DbContext.TicketStatuses, nameof(TicketStatus.Id), nameof(TicketStatus.Name));
                var viewModel = new CreateEditTicketViewModel
                {
                    Projects = projectSelectList,
                    TicketType = ticketTypeSelectList,
                    TicketPriority = ticketPrioritySelectList,
                    TicketStatus = ticketStatusSelectList
                };

                formData.TicketPriority = ticketPrioritySelectList;
                formData.TicketType = ticketTypeSelectList;
                formData.TicketStatus = ticketStatusSelectList;
                formData.Projects = projectSelectList;

                return View(formData);
            }

            Ticket myTicket;

            if (!id.HasValue)
            {
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
                };

                formData.TicketStatusId = DbContext.TicketStatuses
                                    .First(p => p.Name == "Open").Id;
                DbContext.Tickets.Add(myTicket);
            }
            else
            {
                myTicket = DbContext.Tickets.FirstOrDefault(p => p.Id == id);

                if (myTicket == null)
                {
                    return RedirectToAction(nameof(TicketsController.MyTickets));
                }
                myTicket.DateUpdated = DateTime.Now;
            }

            //History

            var ticketEditor = DbContext.Users.First(p => p.Id == userId);

            if (myTicket.Title != formData.Title)
            {
                //To prevent making a ticket history for new tickets created
                if (id.HasValue)
                {
                    var ticketHistory = new TicketHistory()
                    {
                        NewValue = formData.Title,
                        OldValue = myTicket.Title,
                        PropertyName = nameof(myTicket.Title),
                        User = ticketEditor,
                        DateChanged = DateTime.Now,
                    };

                    DbContext.TicketHistories.Add(ticketHistory);
                    myTicket.TicketHistories.Add(ticketHistory);
                }

                myTicket.Title = formData.Title;
            }

            if (myTicket.Description != formData.Description)
            {
                if (id.HasValue)
                {
                    var ticketHistory = new TicketHistory()
                    {
                        NewValue = formData.Description,
                        OldValue = myTicket.Description,
                        PropertyName = nameof(myTicket.Description),
                        User = ticketEditor,
                        DateChanged = DateTime.Now,
                    };

                    DbContext.TicketHistories.Add(ticketHistory);
                    myTicket.TicketHistories.Add(ticketHistory);
                }

                myTicket.Description = formData.Description;
            }

            if (myTicket.ProjectId != formData.ProjectId)
            {
                if (id.HasValue)
                {
                    var ticketHistory = new TicketHistory()
                    {
                        NewValue = DbContext.Projects.First(p => p.Id == formData.ProjectId).Name,
                        OldValue = myTicket.Project.Name,
                        PropertyName = nameof(myTicket.Project),
                        User = ticketEditor,
                        DateChanged = DateTime.Now,
                    };

                    DbContext.TicketHistories.Add(ticketHistory);
                    myTicket.TicketHistories.Add(ticketHistory);
                }

                myTicket.ProjectId = formData.ProjectId;
            }

            if (myTicket.TicketTypeId != formData.TicketTypeId)
            {
                if (id.HasValue)
                {
                    var ticketHistory = new TicketHistory()
                    {
                        NewValue = DbContext.TicketTypes.First(p => p.Id == formData.TicketTypeId).Name,
                        OldValue = myTicket.TicketType.Name,
                        PropertyName = "Ticket Type",
                        User = ticketEditor,
                        DateChanged = DateTime.Now,
                    };

                    DbContext.TicketHistories.Add(ticketHistory);
                    myTicket.TicketHistories.Add(ticketHistory);
                }

                myTicket.TicketTypeId = formData.TicketTypeId;
            }

            if (myTicket.TicketPriorityId != formData.TicketPriorityId)
            {
                if (id.HasValue)
                {
                    var ticketHistory = new TicketHistory()
                    {
                        NewValue = DbContext.TicketPriorities.First(p => p.Id == formData.TicketPriorityId).Name,
                        OldValue = myTicket.TicketPriority.Name,
                        PropertyName = "Ticket Priority",
                        User = ticketEditor,
                        DateChanged = DateTime.Now,
                    };

                    DbContext.TicketHistories.Add(ticketHistory);
                    myTicket.TicketHistories.Add(ticketHistory);
                }

                myTicket.TicketPriorityId = formData.TicketPriorityId;
            }

            if (myTicket.TicketStatusId != formData.TicketStatusId)
            {
                if (id.HasValue)
                {
                    var ticketHistory = new TicketHistory()
                    {
                        NewValue = DbContext.TicketStatuses.First(p => p.Id == formData.TicketStatusId).Name,
                        OldValue = myTicket.TicketStatus.Name,
                        PropertyName = "Ticket Status",
                        User = ticketEditor,
                        DateChanged = DateTime.Now,
                    };

                    DbContext.TicketHistories.Add(ticketHistory);
                    myTicket.TicketHistories.Add(ticketHistory);
                }

                myTicket.TicketStatusId = formData.TicketStatusId;
            }

            if (myTicket.AssignedToUserId != null)
            {
                var userManager = HttpContext.GetOwinContext()
                                .GetUserManager<ApplicationUserManager>();

                userManager.SendEmail(myTicket.AssignedToUserId, "Ticket was Modified", $"'{myTicket.Title}' was Modified by '{ticketEditor.UserName}'");
            }

            DbContext.SaveChanges();

            return RedirectToAction(nameof(TicketsController.Details), new { id = myTicket.Id });
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

            var userId = User.Identity.GetUserId();

            if (!CanEdit(userId, myTicket))
            {
                return RedirectToAction(nameof(TicketsController.MyTickets));
            }

            var helper = new ProjectHelper(DbContext);

            var ticketTypeSelectList = new SelectList(DbContext.TicketTypes, nameof(TicketType.Id), nameof(TicketType.Name));
            var ticketPrioritySelectList = new SelectList(DbContext.TicketPriorities, nameof(TicketPriority.Id), nameof(TicketPriority.Name));
            var ticketStatusSelectList = new SelectList(DbContext.TicketStatuses, nameof(TicketStatus.Id), nameof(TicketStatus.Name));
            var projects = helper.GetUsersProjects(userId);
            var projectSelectList = new SelectList(projects, nameof(Project.Id), nameof(Project.Name));
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
            var myTicket = DbContext.Tickets.FirstOrDefault(p => p.Id == id);

            if (myTicket == null)
            {
                return RedirectToAction(nameof(TicketsController.MyTickets));
            }

            var userId = User.Identity.GetUserId();

            if (!CanEdit(userId, myTicket))
            {
                return RedirectToAction(nameof(TicketsController.MyTickets));
            }

            return SaveTicket(id, formData);
        }

        [HttpGet]
        [Authorize]
        public ActionResult Details(int? id)
        {
            var myTicket = DbContext.Tickets.FirstOrDefault(p => p.Id == id);

            if (!id.HasValue)
            {
                return RedirectToAction(nameof(TicketsController.MyTickets));
            }

            if (myTicket == null)
            {
                return RedirectToAction(nameof(TicketsController.MyTickets));
            }

            var viewModel = new TicketDetailsViewModel
            {
                TicketId = myTicket.Id,
                Title = myTicket.Title,
                Description = myTicket.Description,
                ProjectName = myTicket.Project.Name,
                TypesOfTicket = myTicket.TicketType.Name,
                TypesOfStatus = myTicket.TicketStatus.Name,
                TypesOfPriority = myTicket.TicketPriority.Name,
                DateCreated = myTicket.DateCreated,
                DateUpdated = myTicket.DateUpdated,
                AllComments = myTicket.TicketComments.Select(p => new CommentViewModel()
                {
                    CommentId = p.Id,
                    Comment = p.Comment,
                    TicketId = p.TicketId,
                    DateCreated = p.DateCreated,
                    DateUpdated = p.DateUpdated,
                    User = p.User
                }).ToList(),

                TicketHistories = myTicket.TicketHistories.Select(p => new TicketHistoryViewModel()
                {
                    HistoryId = p.Id,
                    PropertyName = p.PropertyName,
                    OldValue = p.OldValue,
                    NewValue = p.NewValue,
                    User = p.User,
                    TicketId = p.TicketId,
                    DateChanged = p.DateChanged
                }).ToList()

            };

            return View(viewModel);
        }

        [HttpGet]
        [Authorize]
        public ActionResult CreateComments(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(TicketsController.MyTickets));
            }

            var myTicket = DbContext.Tickets.FirstOrDefault(p => p.Id == id);

            if (myTicket == null)
            {
                return RedirectToAction(nameof(TicketsController.MyTickets));
            }

            var userId = User.Identity.GetUserId();

            if (!CanEdit(userId, myTicket))
            {
                return RedirectToAction(nameof(TicketsController.MyTickets));
            }

            var viewModel = new CreateCommentViewModel
            {
                TicketId = id.Value
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        public ActionResult CreateComments(int id, CreateCommentViewModel formData)
        {
            var myTicket = DbContext.Tickets.FirstOrDefault(p => p.Id == id);

            if (myTicket == null)
            {
                return RedirectToAction(nameof(TicketsController.MyTickets));
            }

            var userId = User.Identity.GetUserId();

            if (!CanEdit(userId, myTicket))
            {
                return RedirectToAction(nameof(TicketsController.Details), new { id = formData.TicketId });
            }

            formData.TicketId = id;
            return SaveComments(null, formData, formData.TicketId);
        }

        private ActionResult SaveComments(int? id, CreateCommentViewModel formData, int ticketId)
        {
            if (!ModelState.IsValid)
            {
                return View(formData);
            }

            TicketComment myComment;

            if (!id.HasValue)
            {
                var userId = User.Identity.GetUserId();
                var myTicket = DbContext.Tickets.FirstOrDefault(p => p.Id == formData.TicketId);

                if (myTicket == null)
                {
                    return RedirectToAction(nameof(TicketsController.Details), new { id = ticketId });
                }                

                myComment = new TicketComment()
                {
                    DateCreated = DateTime.Now,
                    User = DbContext.Users.FirstOrDefault(p => p.Id == userId),
                };

                if (myTicket.AssignedToUserId != null)
                {
                    var userManager = HttpContext.GetOwinContext()
                                    .GetUserManager<ApplicationUserManager>();

                    userManager.SendEmail(myTicket.AssignedToUserId, "New Comment Added", $"A New Comment was added to '{myTicket.Title}' by '{myComment.User.UserName}'"); 
                }

                DbContext.AllComments.Add(myComment);
                myTicket.TicketComments.Add(myComment);
            }
            else
            {
                myComment = DbContext.AllComments.FirstOrDefault(p => p.Id == id);

                if (myComment == null)
                {
                    return RedirectToAction(nameof(TicketsController.Details), new { id = ticketId });
                }
            }

            myComment.Comment = formData.Comment;
            myComment.DateUpdated = DateTime.Now;

            DbContext.SaveChanges();
            return RedirectToAction(nameof(TicketsController.Details), new { id = ticketId });
        }

        [HttpGet]
        [Authorize]
        public ActionResult EditComment(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction(nameof(TicketsController.Details));
            }

            var userId = User.Identity.GetUserId();

            var myComment = DbContext.AllComments
                .FirstOrDefault(p => p.Id == id);

            if (myComment == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var model = new CreateCommentViewModel();
            model.Comment = myComment.Comment;
            model.DateUpdated = myComment.DateUpdated;

            DbContext.SaveChanges();

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public ActionResult EditComment(int id, CreateCommentViewModel formData)
        {
            return SaveComments(id, formData, formData.TicketId);
        }

        [HttpPost]
        [Authorize]
        public ActionResult DeleteComment(int? id, int? ticketId)
        {
            if (!id.HasValue)
            {
                return RedirectToAction(nameof(TicketsController.MyTickets));
            }

            var userId = User.Identity.GetUserId();

            var myComment = DbContext.AllComments
                .FirstOrDefault(p => p.Id == id);

            if (myComment != null)
            {
                DbContext.AllComments.Remove(myComment);
                DbContext.SaveChanges();
            }

            if (ticketId == null)
            {
                return RedirectToAction(nameof(TicketsController.MyTickets));
            }

            return RedirectToAction(nameof(TicketsController.Details), new { id = ticketId });
        }

        [HttpGet]
        [Authorize(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult AssignTickets(int? id)
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

            var helper = new UserRolesHelper(DbContext);
            var developer = helper.UsersInRole(nameof(UserRoles.Developer)).ToList();
            var developerList = new SelectList(developer, nameof(ApplicationUser.Id), nameof(ApplicationUser.UserName));

            var viewModel = new AssignTicketViewModel
            {
                TicketId = myTicket.Id,
                Developers = developerList,
                DeveloperId = myTicket.AssignedToUserId
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult AssignTickets(int? id, AssignTicketViewModel formData)
        {
            var myTicket = DbContext.Tickets.FirstOrDefault(p => p.Id == id);

            if (myTicket == null)
            {
                return RedirectToAction(nameof(TicketsController.AllTickets));
            }

            if (myTicket.AssignedToUserId != formData.DeveloperId)
            {
                var userId = User.Identity.GetUserId();

                var ticketHistory = new TicketHistory()
                {
                    NewValue = DbContext.Users.FirstOrDefault(p => p.Id == formData.DeveloperId)?.UserName ?? "No User Assigned to this Ticket",
                    OldValue = myTicket.AssignedToUser?.UserName ?? "Didn't Assign any User",
                    PropertyName = "Assigned User",
                    User = DbContext.Users.First(p => p.Id == userId),
                    DateChanged = DateTime.Now,
                };

                DbContext.TicketHistories.Add(ticketHistory);
                myTicket.TicketHistories.Add(ticketHistory);

                if (formData.DeveloperId != null)
                {
                    var userManager = HttpContext.GetOwinContext()
                        .GetUserManager<ApplicationUserManager>();

                    userManager.SendEmail(formData.DeveloperId, "You are Assigned to a Ticket", $"You were Assigned to '{myTicket.Title}'" );
                }

                myTicket.AssignedToUserId = formData.DeveloperId;
                DbContext.SaveChanges();
            }

            return RedirectToAction(nameof(TicketsController.AllTickets));
        }
    }
}