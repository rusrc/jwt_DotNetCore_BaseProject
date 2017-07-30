using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace WebApplication2.Models
{
    public class CoreDbContext : IdentityDbContext<User>
    {
        public CoreDbContext(DbContextOptions<CoreDbContext> options)
            :base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>().ToTable("User", "dbo");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaim", "dbo");
            builder.Entity<IdentityRole>().ToTable("Role", "dbo");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaim", "dbo");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogin", "dbo");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRole", "dbo");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserToken", "dbo");

        }
    }
}
