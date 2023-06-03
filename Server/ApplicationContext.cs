using Microsoft.EntityFrameworkCore;
using Server.Entities;

namespace Server;

public class ApplicationContext : DbContext
{
    public DbSet<UserEntity> Users { get; set; } = null!;
    public DbSet<MessageEntity> Messages { get; set; } = null!;
    public DbSet<MessageContentEntity> MessagesContents { get; set; } = null!;
    public DbSet<ImageEntity> Images { get; set; } = null!;

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
