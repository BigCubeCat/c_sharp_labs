using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;


namespace DbLogger
{
    public class DbLoggerService
    {
        private readonly DbContextOptions<DbLoggerContext> _options;

        public DbLoggerService(string connectionString)
        {
            var builder = new DbContextOptionsBuilder<DbLoggerContext>();
            builder.UseNpgsql(connectionString);

            _options = builder.Options;

            using var ctx = new DbLoggerContext(_options);
            ctx.Database.Migrate();
        }

        public async Task<int> SaveRunAsync(RunSnapshot run)
        {
            await using var ctx = new DbLoggerContext(_options);
            ctx.Runs.Add(run);
            await ctx.SaveChangesAsync();
            return run.Id;
        }

        public async Task<RunSnapshot?> LoadRunAsync(int id)
        {
            await using var ctx = new DbLoggerContext(_options);
            return await ctx.Runs
                .Include(r => r.Steps)
                    .ThenInclude(s => s.Philosophers)
                .Include(r => r.Steps)
                    .ThenInclude(s => s.Forks)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}

