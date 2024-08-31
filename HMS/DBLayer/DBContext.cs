using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Design;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using DBLayer.Models;



namespace DBLayer
{
    public class DBContext : IdentityDbContext<IdentityUserOwn>       //(pomohol som si z internetu tutoriály/AI)
    {
        public DBContext(DbContextOptions<DBContext> opt) : base(opt)
        {
            try
            {

                var databaseCreator = Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
                if (databaseCreator == null)
                {
                    throw new Exception("Database is not created in DBContext!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public DbSet<Dodavatel> Dodavatelia { get; set; }
        public DbSet<Objednavka> Objednavky { get; set; }
        public DbSet<PolozkaSkladu> PolozkySkladu { get; set; }
        public DbSet<PolozkaSkladuObjednavky> PolozkySkladuObjednavky { get; set; }
        public DbSet<Sklad> Sklady { get; set; }
        public DbSet<SkladObdobie> SkladObdobi { get; set; }
        public DbSet<PolozkaSkladuMnozstvo> PolozkaSkladuMnozstva { get; set; }
        public DbSet<SkladUzivatel> SkladUzivatelia { get; set; }
        public DbSet<PrijemkaPolozka> PrijemkyPolozky { get; set; }     //ten isty typ pre Prijem/Vydaj
        public DbSet<Prijemka> Prijemky { get; set; }
        public DbSet<Vydajka> Vydajky { get; set; }
        public DbSet<PrijemkaPolozka> VydajkyPolozky { get; set; }      //ten isty typ pre Prijem/Vydaj
        public DbSet<Faktura> Faktury { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<IdentityUserLogin<string>>().HasNoKey();
            base.OnModelCreating(modelBuilder); // This will create the ASP.NET Identity tables

            modelBuilder.Entity<IdentityUserLogin<string>>().HasKey(x => x.UserId);


            modelBuilder.Entity<Objednavka>()   //pre cyklicke mazanie
            .HasOne(o => o.DodavatelX)
            .WithMany()
            .HasForeignKey(o => o.Dodavatel)
            .OnDelete(DeleteBehavior.Cascade); // Cascade delete rule for Dodavatel

            modelBuilder.Entity<Objednavka>()
                .HasOne(o => o.OdberatelX)
                .WithMany()
                .HasForeignKey(o => o.Odberatel)
                .OnDelete(DeleteBehavior.NoAction); // No action delete rule for Odberatel


            //modelBuilder.Entity<PolozkaSkladuMnozstvo>()    //definovanie 2 foreign klucov
            //.HasKey(e => new { e.PolozkaSkladu, e.Sklad });

            modelBuilder.Entity<SkladUzivatel>()            //definovanie 2 foreign klucov
            .HasKey(e => new { e.Sklad, e.Uzivatel });


            //modelBuilder.Entity<Vydajka>()   //pre cyklicke mazanie
            //.HasOne(o => o.SkladX)
            //.WithMany()
            //.HasForeignKey(o => o.Sklad)
            //.OnDelete(DeleteBehavior.Cascade); // Cascade delete rule for SkladOd

            //modelBuilder.Entity<Vydajka>()
            //    .HasOne(o => o.SkladDoX)
            //    .WithMany()
            //    .HasForeignKey(o => o.SkladDo)
            //    .OnDelete(DeleteBehavior.NoAction); // No action delete rule for SkladDo

            modelBuilder.Entity<Vydajka>()
                .ToTable("Vydajka");

            modelBuilder.Entity<Vydajka>()
                .HasOne(o => o.SkladX)
                .WithMany()
                .HasForeignKey(o => o.Sklad)
                .OnDelete(DeleteBehavior.Restrict); // Restrict delete rule for Sklad

            modelBuilder.Entity<Vydajka>()
                .HasOne(o => o.SkladDoX)
                .WithMany()
                .HasForeignKey(o => o.SkladDo)
                .OnDelete(DeleteBehavior.Restrict); // Restrict delete rule for SkladDo

            // modelBuilder.Entity<Sklad>()    //sklad ma unique obdobie
            //.HasIndex(s => new { s.ID, s.Obdobie })
            //.IsUnique();
            //modelBuilder.Entity<Sklad>()
            // .HasKey(s => new { s.ID, s.Obdobie });
            modelBuilder.Entity<SkladObdobie>()
                .HasKey(s => new { s.Obdobie, s.Sklad });
        }

        public class YourDbContextFactory : IDesignTimeDbContextFactory<DBContext>    //Pre manazovanie migraci je potrebna tato trieda (pomohol som si z internetu tutoriály/AI)
        {
            public DBContext CreateDbContext(string[] args)
            {
                var optionsBuilder = new DbContextOptionsBuilder<DBContext>();
                optionsBuilder.UseSqlServer("Data Source = localhost, 1433; Database = MyDatabase; User ID = sa; Password = TaJnEhEsLo!!!123456789; Connect Timeout = 30; Encrypt = True; Trust Server Certificate = True; Application Intent = ReadWrite; Multi Subnet Failover = False");

                return new DBContext(optionsBuilder.Options);
            }
        }


    }
}
