using System.Data.Entity;
using Domain;

namespace WebUI.Models.churchdatabaseEntities
{
    public class Context : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, add the following
        // code to the Application_Start method in your Global.asax file.
        // Note: this will destroy and re-create your database with every model change.
        // 
        // System.Data.Entity.Database.SetInitializer(new System.Data.Entity.DropCreateDatabaseIfModelChanges<WebUI.Models.churchdatabaseEntities.Context>());

        public Context() : base("name=Context")
        {
        }

        public DbSet<ministryincome> ministryincomes { get; set; }

        public DbSet<ministryexpense> ministryexpenses { get; set; }

        public DbSet<announcement> announcements { get; set; }

        public DbSet<attendance> attendances { get; set; }

        public DbSet<listheader> listheaders { get; set; }

        public DbSet<programeventbudget> programeventbudgets { get; set; }

        public DbSet<bankbalance> bankbalances { get; set; }

        public DbSet<story> stories { get; set; }

        public DbSet<propertyinventory> propertyinventories { get; set; }

        public DbSet<property> properties { get; set; }

        public DbSet<picture> pictures { get; set; }

        public DbSet<document> documents { get; set; }
    }
}
