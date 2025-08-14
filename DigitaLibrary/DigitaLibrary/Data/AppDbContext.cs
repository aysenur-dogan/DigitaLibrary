using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DigitaLibrary.Models;

namespace DigitaLibrary.Data
{
    // Admin senin Identity kullanıcı sınıfınsa bu doğru.
    // (Eğer adın farklıysa burada ve Program.cs'de o adı kullanmalısın.)
    public class AppDbContext : IdentityDbContext<Admin>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<PostLike> PostLikes => Set<PostLike>();
        public DbSet<PostSave> PostSaves => Set<PostSave>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // ---- İndeksler ----
            b.Entity<Category>()
                .HasIndex(x => x.Slug)
                .IsUnique();

            b.Entity<Post>()
                .HasIndex(x => x.Slug)
                .IsUnique();

            b.Entity<Post>()
                .HasIndex(x => x.CreatedAt);

            // (İsteğe bağlı ama faydalı) kategori/author sorguları için ek indeks:
            b.Entity<Post>()
                .HasIndex(x => x.CategoryId);
            b.Entity<Post>()
                .HasIndex(x => x.AuthorId);

            // ---- İlişkiler ----
            // Post -> Category (çoktan-bire), kategori silinirse yazıları koru (Restrict)
            b.Entity<Post>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Posts)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Post -> Author (çoktan-bire), yazar silinirse yazıları koru (Restrict)
            b.Entity<Post>()
                .HasOne(p => p.Author)
                .WithMany() // Admin içinde Posts koleksiyonu tanımlamadık
                .HasForeignKey(p => p.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- PostLike (UserId + PostId birleşik PK) ----
            b.Entity<PostLike>()
                .HasKey(x => new { x.UserId, x.PostId });

            b.Entity<PostLike>()
                .HasOne(x => x.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<PostLike>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---- PostSave (UserId + PostId birleşik PK) ----
            b.Entity<PostSave>()
                .HasKey(x => new { x.UserId, x.PostId });

            b.Entity<PostSave>()
                .HasOne(x => x.Post)
                .WithMany(p => p.Saves)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<PostSave>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
