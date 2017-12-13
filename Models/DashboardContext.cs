using Microsoft.EntityFrameworkCore;
 
namespace connectingToDBTESTING.Models
{
    public class DashboardContext : DbContext
    {
        // base() calls the parent class' constructor passing the "options" parameter along
        public DashboardContext(DbContextOptions<DashboardContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Amessage> Amessages { get; set; }
        public DbSet<Acomment> Acomments { get; set; }
    }
}