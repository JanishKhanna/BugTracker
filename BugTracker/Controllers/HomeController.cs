using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.MyHelpers;
using BugTracker.Models.ViewModels;
using Microsoft.AspNet.Identity;
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
            var userId = User.Identity.GetUserId();
            var viewModel = ProjectHelper.GetUsersProjects(userId)
                .Select(p => new ProjectViewModel
                {
                    ProjectId = p.Id,
                    Name = p.Name,
                    DateCreated = p.DateCreated,
                    DateUpdated = p.DateUpdated                    
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
                                        .ToList(),
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
    }
}