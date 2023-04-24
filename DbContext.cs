using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Blogs_Console; 

public class BloggingContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }

    public void AddBlog(Blog blog)
    {
        this.Blogs.Add(blog);
        this.SaveChanges();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var configuration =  new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json");
            
        var config = configuration.Build();
        optionsBuilder.UseSqlServer(@config["BlogsConsole:ConnectionString"]);
    }
}