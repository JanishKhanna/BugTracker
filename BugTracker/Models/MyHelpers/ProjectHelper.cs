using BugTracker.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.MyHelpers
{
    public class ProjectHelper
    {
        private ApplicationDbContext DbContext;

        public ProjectHelper(ApplicationDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public Project GetProjectById(int id)
        {
            return DbContext.Projects.FirstOrDefault(
                p => !p.ProjectArchived && p.Id == id);
        }

        public List<Project> GetUsersProjects(string userId)
        {
            return DbContext.Projects
                .Where(p => !p.ProjectArchived && p.ApplicationUsers
                .Any(user => user.Id == userId))
                .ToList();
        }

        public List<Project> GetAllProjects() => DbContext.Projects
            .Where(p => !p.ProjectArchived)
            .ToList();
    }
}