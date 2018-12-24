using System.Data.Entity;
using Domain;

namespace WebUI.Models
{
    public class WebUIContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, add the following
        // code to the Application_Start method in your Global.asax file.
        // Note: this will destroy and re-create your database with every model change.
        // 
        // System.Data.Entity.Database.SetInitializer(new System.Data.Entity.DropCreateDatabaseIfModelChanges<WebUI.Models.WebUIContext>());

        public WebUIContext() : base("name=WebUIContext")
        {
        }

        public DbSet<attendance> attendances { get; set; }

        public DbSet<bankaccount> bankaccounts { get; set; }

        public DbSet<bill> bills { get; set; }

        public DbSet<budget> budgets { get; set; }

        public DbSet<calendar> calendars { get; set; }

        public DbSet<category> categories { get; set; }

        public DbSet<contribution> contributions { get; set; }

        public DbSet<document> documents { get; set; }

        public DbSet<expense> expenses { get; set; }

        public DbSet<ministryexpense> ministryexpenses { get; set; }

        public DbSet<family> families { get; set; }

        public DbSet<goal> goals { get; set; }

        public DbSet<propertyinventory> propertyinventories { get; set; }

        public DbSet<income> incomes { get; set; }

        public DbSet<ministryincome> ministryincomes { get; set; }

        public DbSet<member> members { get; set; }

        public DbSet<ministry> ministries { get; set; }

        public DbSet<payee> payees { get; set; }

        public DbSet<picture> pictures { get; set; }

        public DbSet<property> properties { get; set; }

        public DbSet<responsibility> responsibilities { get; set; }

        public DbSet<staff> staffs { get; set; }

        public DbSet<subcategory> subcategories { get; set; }

        public DbSet<task> tasks { get; set; }

        public DbSet<video> videos { get; set; }

        public DbSet<visitor> visitors { get; set; }

        public DbSet<sale> sales { get; set; }

        public DbSet<product> products { get; set; }

        public DbSet<meeting> meetings { get; set; }

        public DbSet<meetingagenda> meetingagendas { get; set; }

        public DbSet<meetingnote> meetingnotes { get; set; }

        public DbSet<constant> constants { get; set; }
    }
}
