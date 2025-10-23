using Microsoft.EntityFrameworkCore;
using MyBoards.Entities.Configurations;
using MyBoards.Entities.ViewModels;
namespace MyBoards.Entities
{
    public class MyBoardsContext : DbContext
    {
        public MyBoardsContext(DbContextOptions<MyBoardsContext> options) :base(options)
        {
            
        }
        public DbSet<WorkItem> WorkItems { get; set; }
        public DbSet<Issue> Issue{ get; set; }
        public DbSet<Epic> Epic{ get; set; }
        public DbSet<Task> Task{ get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<WorkItemTag> WorkItemTag { get; set; }
        public DbSet<TopAuthor> ViewTopAuthors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }
    }
}
