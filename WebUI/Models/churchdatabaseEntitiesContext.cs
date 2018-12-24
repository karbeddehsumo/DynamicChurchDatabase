using System.Data.Entity;
using Domain;

namespace WebUI.Models
{
    public class churchdatabaseEntitiesContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, add the following
        // code to the Application_Start method in your Global.asax file.
        // Note: this will destroy and re-create your database with every model change.
        // 
        // System.Data.Entity.Database.SetInitializer(new System.Data.Entity.DropCreateDatabaseIfModelChanges<WebUI.Models.churchdatabaseEntitiesContext>());

        public churchdatabaseEntitiesContext() : base("name=churchdatabaseEntitiesContext")
        {
        }

        public DbSet<constant> constants { get; set; }

        public DbSet<expense> expenses { get; set; }

        public DbSet<listtable> listtables { get; set; }

        public DbSet<listitem> listitems { get; set; }

        public DbSet<meetingagenda> meetingagendas { get; set; }

        public DbSet<meetingnote> meetingnotes { get; set; }

        public DbSet<pledge> pledges { get; set; }

        public DbSet<programevent> programevents { get; set; }

        public DbSet<saleitem> saleitems { get; set; }

        public DbSet<budget> budgets { get; set; }
    }
}
