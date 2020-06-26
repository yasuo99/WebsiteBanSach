using System;
using System.Collections.Generic;
using System.Text;
using BookStore.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)
        {
        }
        public DbSet<Book> Books { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Genrer> Genrers { get; set; }
        public DbSet<BookGenrer> BooksGenrers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Shipper> Shippers { get; set; }
        public DbSet<SpecialTag> SpecialTags { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<ManagerRoleOnDatabase> ManagerRoleOnDatabases { get; set; }
        public ApplicationDbContext Create(string connectionstring)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>();
            options.UseSqlServer(connectionstring);
            return new ApplicationDbContext(options.Options);
        }
    }
}

