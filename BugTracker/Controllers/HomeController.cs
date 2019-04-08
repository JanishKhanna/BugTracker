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

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AllProjects()
        {
            var viewModel = ProjectHelper.GetAllProjects()
                .Select(p => new ProjectViewModel
                {
                    ProjectId = p.Id,
                    Name = p.Name,
                    DateCreated = p.DateCreated,
                    DateUpdated = p.DateUpdated,
                    ApplicationUsers = p.ApplicationUsers
                }).ToList();

            return View(viewModel);
        }

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
                    ApplicationUsers = p.ApplicationUsers
                }).ToList();

            return View(viewModel);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpGet]
        [Authorize(Roles = nameof(Roles.Admin) + "," + nameof(Roles.ProjectManager))]
        public ActionResult CreateProject()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = nameof(Roles.Admin) + "," + nameof(Roles.ProjectManager))]
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
        [Authorize(Roles = nameof(Roles.Admin) + "," + nameof(Roles.ProjectManager))]
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
        [Authorize(Roles = nameof(Roles.Admin) + "," + nameof(Roles.ProjectManager))]
        public ActionResult EditProject(int id, CreateEditProjectViewModel formData)
        {
            return SaveProject(id, formData);
        }

        [HttpGet]
        [Authorize(Roles = nameof(Roles.Admin))]
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
        [Authorize(Roles = nameof(Roles.Admin))]
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
        [Authorize(Roles = nameof(Roles.Admin))]
        public ActionResult AddRole(string roleName, string userId)
        {
            if(userId == null)
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
        [Authorize(Roles = nameof(Roles.Admin))]
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

        //public ActionResult ManageProjects()
        //{
        //    return View();
        //}
    }
};