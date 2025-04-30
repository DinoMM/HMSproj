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
using DBLayer.Models.RecepciaModels;
using DBLayer.Models.HSKModels;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;



namespace DBLayer
{
    public class DBContext : IdentityDbContext<IdentityUserOwn>, IDataProtectionKeyContext        //(pomohol som si z internetu tutoriály/AI)
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

        public void ClearPendingChanges()
        {
            var changedEntries = this.ChangeTracker.Entries()
                .Where(e => e.State != EntityState.Unchanged)
                .ToList();

            foreach (var entry in changedEntries)
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.Reload();
                        break;
                }
            }
        }

#if DEBUG
        public List<string> ClearPendingChangesDebug()
        {
            var changedEntries = this.ChangeTracker.Entries()
                    .Where(e => e.State != EntityState.Unchanged)
                    .ToList();

            var list = new List<string>();

            foreach (var entry in changedEntries)
            {
                list.Add($"****PENDING****Entity:{entry.Entity.ToString()}; {entry.State.ToString()};[CURRENT]{string.Join("", entry.Properties.Select(s => s.CurrentValue + "|"))}\n");
                list.Add($"[ORIGINAL]{string.Join("", entry.Properties.Select(s => s.OriginalValue +"|"))}\n");
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.Reload();
                        break;
                }
            }
            return list;
        }
#endif

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } //ukladanie encrypt klucov

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
        public DbSet<DruhPohybu> DruhyPohybov { get; set; }
        public DbSet<ZmenaMeny> ZmenyMien { get; set; }

        public DbSet<Host> Hostia { get; set; }
        public DbSet<HostFlag> HostFlags { get; set; }
        public DbSet<HostConFlag> HostConFlags { get; set; }
        public DbSet<HostConReservation> HostConReservations { get; set; }
        public DbSet<RoomFlag> RoomFlags { get; set; }
        public DbSet<RoomConFlag> RoomConFlags { get; set; }
        public DbSet<RoomInfo> RoomInfos { get; set; }


        public DbSet<Kasa> Kasy { get; set; }
        public DbSet<PokladnicnyDoklad> PokladnicneDoklady { get; set; }
        public DbSet<ItemPokladDokladu> ItemyPokladDokladu { get; set; }

        public DbSet<UniConItemPoklDokladu> UniConItemyPoklDokladu { get; set; }
        public DbSet<ReservationConItemPoklDokladu> ReservationConItemyPoklDokladu { get; set; }
        public DbSet<PolozkaSkladuConItemPoklDokladu> PolozkySkladuConItemPoklDokladu { get; set; }
        public DbSet<UniConItemPoklDokladuFlag> UniConItemPoklDokladuFlagy { get; set; }
        public DbSet<UniConItemPoklDokladuConFlag> UniConItemPoklDokladuConFlagy { get; set; }


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
            modelBuilder.Entity<SkladObdobie>()                     //bez primarneho kluca
                .HasKey(s => new { s.Obdobie, s.Sklad });

            modelBuilder.Entity<HostConFlag>()                      //bez primarneho kluca
                .HasKey(s => new { s.Host, s.HostFlag });
            modelBuilder.Entity<HostConReservation>()                      //bez primarneho kluca
                .HasKey(s => new { s.Host, s.Reservation });
            modelBuilder.Entity<RoomConFlag>()                      //bez primarneho kluca
                .HasKey(s => new { s.Room, s.RoomFlag });
            modelBuilder.Entity<UniConItemPoklDokladuConFlag>()                      //bez primarneho kluca
                .HasKey(s => new { s.UniConItemPoklDokladu, s.UniConItemPoklDokladuFlag });

            modelBuilder.Entity<ReservationConItemPoklDokladu>()
            .HasIndex(e => e.Reservation)
            .IsUnique();

            modelBuilder.Entity<PolozkaSkladuConItemPoklDokladu>()
            .HasIndex(e => e.PolozkaSkladuMnozstva)
            .IsUnique();

            modelBuilder.Entity<Faktura>()
            .HasKey(f => new { f.ID, f.Skupina });
            modelBuilder.Entity<Faktura>()
                .HasOne(f => f.SkupinaX)
                .WithMany()
                .HasForeignKey(f => f.Skupina)
                .IsRequired(false);
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

    public class DelayCommandInterceptor : DbCommandInterceptor
    {
        private readonly TimeSpan _delay;

        public DelayCommandInterceptor(TimeSpan delay)
        {
            _delay = delay;
        }

        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
         DbCommand command,
         CommandEventData eventData,
         InterceptionResult<DbDataReader> result,
         CancellationToken cancellationToken = default)
        {
            await Task.Delay(_delay, cancellationToken);
            return await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

    }
}
