using GomexPraksa.ApplicationUserSecurity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GomexPraksa.Auth
{
    public class AuthDbContext : IdentityDbContext<ApplicationUser>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserKategorija> UserKategorije { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserKategorija>()
                .HasKey(uk => new
                {
                    uk.UserId,
                    uk.KategorijaId
                });

            builder.Entity<UserKategorija>()
                .HasOne(uk => uk.User)
                .WithMany(u => u.Kategorije)
                .HasForeignKey(uk => uk.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}