namespace PokeSim.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<PokeSim.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "PokeSim.Models.ApplicationDbContext";
        }

        protected override void Seed(PokeSim.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            UserStore<ApplicationUser> userStore = new UserStore<ApplicationUser>(context);
            UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(userStore);

            string adminEmail = "system@pokesim.com";
            string adminPw = "passW0rd!";

            if (!context.Users.Any(i => i.UserName == adminEmail))
            {
                ApplicationUser adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail };
                userManager.Create(adminUser, adminPw);

                context.Roles.AddOrUpdate(r => r.Name, new IdentityRole { Name = EnumHelpers.ROLE_ADMIN });
                context.SaveChanges();

                userManager.AddToRole(adminUser.Id, EnumHelpers.ROLE_ADMIN);
            }

        }
    }
}
