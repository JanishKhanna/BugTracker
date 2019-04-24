namespace BugTracker.Migrations
{
    using BugTracker.Models;
    using BugTracker.Models.Domain;
    using BugTracker.Models.MyHelpers;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Collections.Generic;
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

            //var ticketTypeManager = new 

            if (!context.Roles.Any(p => p.Name == nameof(UserRoles.Admin)))
            {
                var adminRole = new IdentityRole(nameof(UserRoles.Admin));
                roleManager.Create(adminRole);
            }

            if (!context.Roles.Any(p => p.Name == nameof(UserRoles.ProjectManager)))
            {
                var projectManagerRole = new IdentityRole(nameof(UserRoles.ProjectManager));
                roleManager.Create(projectManagerRole);
            }

            if(!context.Roles.Any(p => p.Name == nameof(UserRoles.Developer)))
            {
                var developerRole = new IdentityRole(nameof(UserRoles.Developer));
                roleManager.Create(developerRole);
            }

            if(!context.Roles.Any(p => p.Name == nameof(UserRoles.Submitter)))
            {
                var submitterRole = new IdentityRole(nameof(UserRoles.Submitter));
                roleManager.Create(submitterRole);
            }

            if(!context.TicketTypes.Any(p => p.Name == nameof(TypesOfTicket.Bug)))
            {
                var bugTicketType = new TicketType();
                bugTicketType.Name = nameof(TypesOfTicket.Bug);
                context.TicketTypes.Add(bugTicketType);
            }

            if (!context.TicketTypes.Any(p => p.Name == nameof(TypesOfTicket.Feature)))
            {
                var featureTicketType = new TicketType();
                featureTicketType.Name = nameof(TypesOfTicket.Feature);
                context.TicketTypes.Add(featureTicketType);
            }

            if (!context.TicketTypes.Any(p => p.Name == nameof(TypesOfTicket.DataBase)))
            {
                var dataBaseTicketType = new TicketType();
                dataBaseTicketType.Name = nameof(TypesOfTicket.DataBase);
                context.TicketTypes.Add(dataBaseTicketType);
            }

            if (!context.TicketTypes.Any(p => p.Name == nameof(TypesOfTicket.Support)))
            {
                var supportTicketType = new TicketType();
                supportTicketType.Name = nameof(TypesOfTicket.Support);
                context.TicketTypes.Add(supportTicketType);
            }

            if (!context.TicketPriorities.Any(p => p.Name == nameof(TypesOfPriority.Low)))
            {
                var lowPriority = new TicketPriority();
                lowPriority.Name = nameof(TypesOfPriority.Low);
                context.TicketPriorities.Add(lowPriority);
            }

            if (!context.TicketPriorities.Any(p => p.Name == nameof(TypesOfPriority.Medium)))
            {
                var mediumPriority = new TicketPriority();
                mediumPriority.Name = nameof(TypesOfPriority.Medium);
                context.TicketPriorities.Add(mediumPriority);
            }

            if (!context.TicketPriorities.Any(p => p.Name == nameof(TypesOfPriority.High)))
            {
                var highPriority = new TicketPriority();
                highPriority.Name = nameof(TypesOfPriority.High);
                context.TicketPriorities.Add(highPriority);

            }

            if (!context.TicketStatuses.Any(p => p.Name == nameof(TypesOfStatus.Open)))
            {
                var openStatus = new TicketStatus();
                openStatus.Name = nameof(TypesOfStatus.Open);
                context.TicketStatuses.Add(openStatus);
            }

            if (!context.TicketStatuses.Any(p => p.Name == nameof(TypesOfStatus.Resolved)))
            {
                var resolvedStatus = new TicketStatus();
                resolvedStatus.Name = nameof(TypesOfStatus.Resolved);
                context.TicketStatuses.Add(resolvedStatus);
            }

            if (!context.TicketStatuses.Any(p => p.Name == nameof(TypesOfStatus.Rejected)))
            {
                var rejectedStatus = new TicketStatus();
                rejectedStatus.Name = nameof(TypesOfStatus.Rejected);
                context.TicketStatuses.Add(rejectedStatus);
            }

            context.SaveChanges();

            ApplicationUser adminUser;

            if (!context.Users.Any(p => p.UserName == "admin@mybugtracker.com"))
            {
                adminUser = new ApplicationUser();
                adminUser.UserName = "admin@mybugtracker.com";
                adminUser.Email = "admin@mybugtracker.com";
                adminUser.EmailConfirmed = true; //To Test Email if Confirmed or not.
                adminUser.DisplayName = "Admin Display Name";

                userManager.Create(adminUser, "Password-1");
            }
            else
            {
                adminUser = context.Users.First(p => p.UserName == "admin@mybugtracker.com");
            }

            if(!userManager.IsInRole(adminUser.Id, nameof(UserRoles.Admin)))
            {
                userManager.AddToRole(adminUser.Id, nameof(UserRoles.Admin));
            }

            context.SaveChanges();

            Project myProject;

            if(!context.Projects.Any(p => p.Name == "My New Project"))
            {
                myProject = new Project()
                {
                    Name = "My New Project",
                    ApplicationUsers = new List<ApplicationUser>()
                    {
                        adminUser
                    }
                };

                context.Projects.Add(myProject);
            }
            else
            {
                myProject = context.Projects.First(p => p.Name == "My New Project");
            }

            context.SaveChanges();
        }
    }
}
