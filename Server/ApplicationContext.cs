using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server;

public class ApplicationContext : DbContext
{
    public DbSet<Rank> Ranks { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<UserPassword> Passwords { get; set; } = null!;
    public DbSet<MessageContent> MessagesContents { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<Chat> Chats { get; set; } = null!;

    public ApplicationContext()
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=database.db");
        base.OnConfiguring(optionsBuilder);
    }
}
