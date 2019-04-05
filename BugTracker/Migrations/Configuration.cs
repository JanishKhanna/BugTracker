namespace BugTracker.Migrations
{
    using BugTracker.Models;
    using BugTracker.Models.MyHelpers;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<BugTracker.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "BugTracker.Models.ApplicationDbContext";
        }

        protected override void Seed(BugTracker.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
            var userManager =
                new UserManager<ApplicationUser>(
                        new UserStore<ApplicationUser>(context));

            var roleManager =
               new RoleManager<IdentityRole>(
                   new RoleStore<IdentityRole>(context));

            if (!context.Roles.Any(p => p.Name == nameof(Roles.Admin)))
            {
                var adminRole = new IdentityRole(nameof(Roles.Admin));
                roleManager.Create(adminRole);
            }

            if (!context.Roles.Any(p => p.Name == nameof(Roles.ProjectManager)))
            {
                var projectManagerRole = new IdentityRole(nameof(Roles.ProjectManager));
                roleManager.Create(projectManagerRole);
            }

            if(!context.Roles.Any(p => p.Name == nameof(Roles.Developer)))
            {
                var developerRole = new IdentityRole(nameof(Roles.Developer));
                roleManager.Create(developerRole);
            }

            if(!context.Roles.Any(p => p.Name == nameof(Roles.Submitter)))
            {
                var submitterRole = new IdentityRole(nameof(Roles.Submitter));
                roleManager.Create(submitterRole);
            }

            ApplicationUser adminUser;

            if (!context.Users.Any(p => p.UserName == "admin@mybugtracker.com"))
            {
                adminUser = new ApplicationUser();
                adminUser.UserName = "admin@mybugtracker.com";
                adminUser.Email = "admin@mybugtracker.com";
                adminUser.EmailConfirmed = true; //To Test Email if Confirmed or not.

                userManager.Create(adminUser, "Password-1");
            }
            else
            {
                adminUser = context.Users.First(p => p.UserName == "admin@mybugtracker.com");
            }

            if(!userManager.IsInRole(adminUser.Id, nameof(Roles.Admin)))
            {
                userManager.AddToRole(adminUser.Id, nameof(Roles.Admin));
            }
        }
    }
}
