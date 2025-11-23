using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AIWA.EntityFrameworkCore.SQLite;

public class AiwaDbContextSQLiteDesignTimeFactory : IDesignTimeDbContextFactory<AiwaDbContext>
{
    public AiwaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AiwaDbContext>();
        optionsBuilder.UseSqlite();
        return new AiwaDbContext(optionsBuilder.Options);
    }
}
