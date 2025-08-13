using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using YourNamespaceHere.Models; // Admin için

namespace YourNamespaceHere.Data
{
    public class AppDbContext : IdentityDbContext<Admin>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // İleride diğer tabloları buraya ekleyeceğiz
    }
}
