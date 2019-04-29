using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.MyHelpers;
using BugTracker.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext DbContext;
        private ProjectHelper ProjectHelper;

        public HomeController()
        {
            DbContext = new ApplicationDbContext();
            ProjectHelper = new ProjectHelper(DbContext);
        }

        [Authorize]
        public ActionResult Index()
        {
            return RedirectToAction(nameof(HomeController.FullUserDetails));
        }

        [HttpGet]
        [Authorize]
        public ActionResult FullUserDetails()
        {
            var userId = User.Identity.GetUserId();
            var myUser = DbContext.Users.FirstOrDefault(p => userId == p.Id);
            bool isAdminOrManager = User.IsInRole(nameof(UserRoles.Admin)) || User.IsInRole(nameof(UserRoles.ProjectManager));
            bool isSubmitter = User.IsInRole(nameof(UserRoles.Submitter));
            bool isDeveloper = User.IsInRole(nameof(UserRoles.Developer));
            string userEmail = myUser.Email;

            int myProjects;
            int myTickets;

            if (isAdminOrManager)
            {
                myProjects = DbContext.Projects.Count();
                myTickets = DbContext.Tickets.Count();
            }
            else if (isDeveloper || isSubmitter)
            {
                myTickets = DbContext.Tickets.ToList()
                            .Where(myTicket => (isSubmitter && userEmail == myTicket.OwnerUser.Email) || (myTicket.AssignedToUser != null && isDeveloper && userEmail == myTicket.AssignedToUser.Email))
                            .Count();
                myProjects = myUser.Projects.Count;
            }            
            else
            {
                throw new Exception("Unexpected Role");
            }

            var opentickets = DbContext.Tickets
                                .Where(p => p.TicketStatus.Name == TypesOfStatus.Open.ToString()).Count();
            var resolvedTickets = DbContext.Tickets
                                    .Where(p => p.TicketStatus.Name == TypesOfStatus.Resolved.ToString())
                                    .Count();
            var rejectedTickets = DbContext.Tickets
                                    .Where(p => p.TicketStatus.Name == TypesOfStatus.Rejected.ToString())
                                    .Count();
            var viewModel = new DashBoardViewModel()
            {
                Projects = myProjects,
                Tickets = myTickets,
                OpenTickets = opentickets,
                ResolvedTickets = resolvedTickets,
                RejectedTickets = rejectedTickets
            };

            return View(viewModel);
        }

        [Authorize(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult AllProjects()
        {
            var viewModel = ProjectHelper.GetAllProjects()
                .Select(p => new ProjectViewModel
                {
                    ProjectId = p.Id,
                    Name = p.Name,
                    DateCreated = p.DateCreated,
                    DateUpdated = p.DateUpdated,
                    ApplicationUsers = p.ApplicationUsers,
                    Tickets = p.Tickets
                }).ToList();

            return View(viewModel);
        }

        [Authorize]
        public ActionResult MyProjects()
        {
            var userId = User.Identity.GetUserId();
            var viewModel = ProjectHelper.GetUsersProjects(userId)
                .Select(p => new ProjectViewModel
                {
                    ProjectId = p.Id,
                    Name = p.Name,
                    DateCreated = p.DateCreated,
                    DateUpdated = p.DateUpdated,
                    ApplicationUsers = p.ApplicationUsers,
                    Tickets = p.Tickets
                }).ToList();

            return View(viewModel);
        }

        [HttpGet]
        [Authorize(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult CreateProject()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult CreateProject(CreateEditProjectViewModel formData)
        {
            return SaveProject(null, formData);
        }

        private ActionResult SaveProject(int? id, CreateEditProjectViewModel formData)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            Project myProject;

            if (!id.HasValue)
            {
                var userId = User.Identity.GetUserId();

                myProject = new Project()
                {
                    //Name = formData.Name,
                    DateCreated = DateTime.Now,
                    //DateUpdated = DateTime.Now
                    ApplicationUsers = DbContext.Users
                                        .Where(p => p.Id == userId)
                                        .ToList() ?? new List<ApplicationUser>(),
                };
                DbContext.Projects.Add(myProject);
            }
            else
            {
                myProject = DbContext.Projects.FirstOrDefault(p => p.Id == id);

                if (myProject == null)
                {
                    return RedirectToAction(nameof(HomeController.Index));
                }
                myProject.DateUpdated = DateTime.Now;
            }

            myProject.Name = formData.Name;

            DbContext.SaveChanges();

            return RedirectToAction(nameof(HomeController.Index));
        }

        [HttpGet]
        [Authorize(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult EditProject(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction(nameof(HomeController.Index));
            }

            var myProject = DbContext.Projects.FirstOrDefault(p => p.Id == id);

            if (myProject == null)
            {
                return RedirectToAction(nameof(HomeController.Index));
            }

            var viewModel = new CreateEditProjectViewModel();
            viewModel.Name = myProject.Name;
            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult EditProject(int id, CreateEditProjectViewModel formData)
        {
            return SaveProject(id, formData);
        }

        [HttpGet]
        [Authorize(Roles = nameof(UserRoles.Admin))]
        public ActionResult ManageUsers()
        {
            var allUsers = DbContext.Users.ToList();
            var viewModel = new List<ManageUsersViewModel>();
            foreach (var user in allUsers)
            {

                var roles = DbContext.Roles
                    .Where(role => role.Users
                    .FirstOrDefault(p => p.UserId == user.Id) != null)
                    .ToList();
                var newModel = new ManageUsersViewModel()
                {
                    UserId = user.Id,
                    UserRoles = roles,
                    DisplayName = user.DisplayName
                };

                viewModel.Add(newModel);
            }

            return View(viewModel);
        }

        [HttpGet]
        [Authorize(Roles = nameof(UserRoles.Admin))]
        public ActionResult AddRole(string userId)
        {
            if (userId == null)
            {
                RedirectToAction(nameof(HomeController.Index));
            }

            var rolesHelper = new UserRolesHelper(DbContext);
            var listOfRoles = rolesHelper.ListUserRoles(userId).ToList();
            var viewModel = new AddRoleViewModel()
            {
                RolesToAdd = DbContext.Roles
                .Where(p => !listOfRoles.Contains(p.Name))
                .Select(p => p.Name)
                .ToList(),
                UserId = userId
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRoles.Admin))]
        public ActionResult AddRole(string roleName, string userId)
        {
            if (userId == null)
            {
                RedirectToAction(nameof(HomeController.Index));
            }

            var rolesHelper = new UserRolesHelper(DbContext);

            bool addedRole = rolesHelper.AddUserToRole(userId, roleName);

            if (!addedRole)
            {
                throw new Exception("Role not added");
            }

            return RedirectToAction(nameof(HomeController.ManageUsers));
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRoles.Admin))]
        public ActionResult DeleteRole(string roleName, string userId)
        {
            if (userId == null)
            {
                RedirectToAction(nameof(HomeController.Index));
            }

            var roleHelper = new UserRolesHelper(DbContext);

            bool roleRemoved = roleHelper.RemoveUserFromRole(userId, roleName);

            if (!roleRemoved)
            {
                throw new Exception("Failed to Remove Roles");
            }

            return RedirectToAction(nameof(HomeController.ManageUsers));
        }

        [HttpGet]
        [Authorize(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult EditMembers(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction(nameof(HomeController.Index));
            }
            var viewModel = new ProjectAssigningViewModel();
            var myProject = DbContext.Projects.FirstOrDefault(p => p.Id == id);
            viewModel.ProjectId = myProject.Id;
            var users = DbContext.Users.ToList();
            var usersAssigned = myProject.ApplicationUsers.ToList();
            var userNotAssigned = DbContext.Users.ToList()
                .Where(p => !usersAssigned.Any(user => user.Id == p.Id))
                .ToList();

            var usersToAdd = new HashSet<ApplicationUser>(userNotAssigned);
            var usersToDelete = new HashSet<ApplicationUser>(usersAssigned);

            foreach (var user in usersAssigned)
            {
                if (myProject.ApplicationUsers.Contains(user))
                {
                    myProject.ApplicationUsers.Remove(user);
                }
            }
            viewModel.AddUser = new MultiSelectList(usersToAdd, "Id", "DisplayName", usersAssigned);
            viewModel.DeleteUser = new MultiSelectList(usersToDelete, "Id", "DisplayName", userNotAssigned);
            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRoles.Admin) + "," + nameof(UserRoles.ProjectManager))]
        public ActionResult EditMembers(ProjectAssigningViewModel formData)
        {
            var myProject = DbContext.Projects.FirstOrDefault(p => p.Id == formData.ProjectId);
            if (myProject == null)
            {
                return RedirectToAction(nameof(HomeController.Index));
            }

            if (formData.SelectedDeleteUsers != null)
            {
                foreach (var userId in formData.SelectedDeleteUsers)
                {
                    var user = DbContext.Users.FirstOrDefault(p => p.Id == userId);
                    myProject.ApplicationUsers.Remove(user);
                }
            }

            if (formData.SelectedAddUsers != null)
            {
                foreach (var userId in formData.SelectedAddUsers)
                {
                    var user = DbContext.Users.FirstOrDefault(p => p.Id == userId);
                    myProject.ApplicationUsers.Add(user);
                }
            }

            DbContext.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}