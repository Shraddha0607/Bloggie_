using Bloggie.Web.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bloggie.Web.Data
{
    public class BloggieDbContext: DbContext
    {
        // creating constructor
        public BloggieDbContext(DbContextOptions<BloggieDbContext> options) : base(options) { 
        }

        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Tag> Tags { get; set; }
    }
}
