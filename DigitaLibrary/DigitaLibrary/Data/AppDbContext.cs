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

            // 📌 Seed Categories
            b.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Eğitim", Slug = "egitim" },
                new Category { Id = 2, Name = "Sağlık", Slug = "saglik" },
                new Category { Id = 3, Name = "Mühendislik ve Teknoloji", Slug = "muhendislik-teknoloji" },
                new Category { Id = 4, Name = "Fen Bilimleri", Slug = "fen-bilimleri" },
                new Category { Id = 5, Name = "Sosyal Bilimler", Slug = "sosyal-bilimler" },
                new Category { Id = 6, Name = "Beşeri Bilimler", Slug = "beseri-bilimler" },
                new Category { Id = 7, Name = "Uygulamalı Bilimler", Slug = "uygulamali-bilimler" },
                new Category { Id = 8, Name = "Diğer", Slug = "diger" }
            );
        }
    }
}
