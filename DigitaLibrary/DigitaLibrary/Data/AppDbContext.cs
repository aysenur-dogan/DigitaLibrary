using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DigitaLibrary.Models;

namespace DigitaLibrary.Data
{
    public class AppDbContext : IdentityDbContext<Admin>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<PostLike> PostLikes => Set<PostLike>();
        public DbSet<PostSave> PostSaves => Set<PostSave>();

        // Yeni:
        public DbSet<AcademicWork> AcademicWorks => Set<AcademicWork>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            b.Entity<Category>().HasIndex(x => x.Slug).IsUnique();
            b.Entity<Post>().HasIndex(x => x.Slug).IsUnique();

            // AcademicWork slug benzersiz
            b.Entity<AcademicWork>().HasIndex(x => x.Slug).IsUnique();

            // İlişkiler
            b.Entity<Post>()
                .HasOne(p => p.Category).WithMany(c => c.Posts)
                .HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);

            b.Entity<Post>()
                .HasOne(p => p.Author).WithMany()
                .HasForeignKey(p => p.AuthorId).OnDelete(DeleteBehavior.Restrict);

            b.Entity<PostLike>().HasKey(x => new { x.UserId, x.PostId });
            b.Entity<PostSave>().HasKey(x => new { x.UserId, x.PostId });

            b.Entity<AcademicWork>()
                .HasOne(x => x.Category).WithMany()
                .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);

            b.Entity<AcademicWork>()
                .HasOne(x => x.Author).WithMany()
                .HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
