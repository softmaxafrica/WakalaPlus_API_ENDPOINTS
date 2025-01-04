using Microsoft.EntityFrameworkCore;
using System.Configuration;
using WakalaPlus.Models;

//using MySql.EntityFrameworkCore;
//using MySQL.Data.EntityFrameworkCore;

namespace WakalaPlus.Shared
{
    public class AppDbContext : DbContext
    {
        IConfiguration _config;

        public DbSet<Agent> Agents { get; set; }
        public DbSet<Customer> Customers{ get; set; }

        public DbSet<CustomerTickets> CustomerTickets { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<OnlineOfflineAgent> OnlineOfflineAgent { get; set; }
        public DbSet<GeneralTranslations> GeneralTranslations { get; set; }
        public DbSet<TicketDetails> TicketDetails { get; set; }
        public DbSet<DeviceDetails> DeviceDetails { get; set; }    

        public DbSet<NotificationMessages> NotificationMessages { get; set; }
        public DbSet<ServiceProviders> ServiceProviders { get; internal set; }

        #region AppDbContext
        public AppDbContext(IConfiguration config)
        {
            _config = config;
        }
        #endregion


        #region OnConfiguring
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(_config.GetSection("ConnectionStrings")["DB_Connection"]);
            base.OnConfiguring(optionsBuilder);

        }
        #endregion

    
    }
}
