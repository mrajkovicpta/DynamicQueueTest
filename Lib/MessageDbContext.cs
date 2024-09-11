using Lib.Entities;
using Microsoft.EntityFrameworkCore;


namespace Lib;

public class MessageDbContext : DbContext
{

    public MessageDbContext(DbContextOptions<MessageDbContext> options) : base(options) { }

    public DbSet<NumberEntity> Numbers => Set<NumberEntity>();
    public DbSet<StringEntity> Strings => Set<StringEntity>();
    public DbSet<MessageConfiguration> Configuration => Set<MessageConfiguration>();
}
