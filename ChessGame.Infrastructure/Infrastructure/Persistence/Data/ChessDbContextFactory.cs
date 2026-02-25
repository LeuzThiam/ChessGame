using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ChessGame.Infrastructure.Persistence.Data
{
    /// <summary>
    /// Factory utilisée uniquement au design-time pour les migrations EF Core.
    /// </summary>
    public class ChessDbContextFactory : IDesignTimeDbContextFactory<ChessDbContext>
    {
        public ChessDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("ChessDb")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ChessGameDb;Integrated Security=True;MultipleActiveResultSets=True;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

            var optionsBuilder = new DbContextOptionsBuilder<ChessDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ChessDbContext(optionsBuilder.Options);
        }
    }
}
