using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using DBLayer.Models;


namespace DBLayer
{

    public class DataContext : IdentityDbContext<IdentityUserWebOwn>       //(pomohol som si z internetu tutoriály/AI)
    {
        public DataContext(DbContextOptions<DataContext> opt) : base(opt)
        {
            try
            {
                var databaseCreator = Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
                if (databaseCreator != null)
                {
                    if (!databaseCreator.CanConnect()) databaseCreator.Create();        //vytvori databazu ak neexituje
                    if (!databaseCreator.HasTables()) databaseCreator.CreateTables();   //vytvori tabulky ak databaza nema tabulky
                }
                else
                {
                    throw new Exception("Database is not created in DataContext!");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public DbSet<Rezervation> Rezervations { get; set; }            //moja tvorba
        public DbSet<Room> HRooms { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<HEvent> Events { get; set; }
        public DbSet<UserHEvent> UserHEvents { get; set; }


    }

    public class YourDbContextFactory : IDesignTimeDbContextFactory<DataContext>    //(pomohol som si z internetu tutoriály/AI)
    {
        public DataContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
            optionsBuilder.UseSqlServer("Data Source=localhost;User ID=sa;Password=TaJnEhEsLo!!!123456789;Connect Timeout=30;Encrypt=False;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False;Initial Catalog=HlavnaDatabaza");

            return new DataContext(optionsBuilder.Options);
        }
    }


}
