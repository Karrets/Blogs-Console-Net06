using Microsoft.EntityFrameworkCore;
using NLog;

// See https://aka.ms/new-console-template for more information

namespace Blogs_Console;

internal abstract class Program {
    private static void Main(string[] args) {
        string path = Directory.GetCurrentDirectory() + "/nlog.config";

        // create instance of Logger
        var logger = LogManager.LoadConfiguration(path).GetCurrentClassLogger();
        logger.Info("Program started");

        bool run = true;
        var db = new BloggingContext();

        while(run) {
            Console.Write("""
                          Choose one of the following:
                          1. (D)isplay all blogs.
                          2. (A)dd blog
                          3. (C)reate Post
                          4. (V)iew Posts
                          Anything else to exit...
                          
                          > 
                          """);

            Blog blog;
            
            switch(Console.ReadLine()?.ToLower()[0]) {
                case 'd':
                case '1':
                    PrintBlogs(db, logger);
                    break;
                case 'a':
                case '2':
                    try {
                        Console.Write("Enter a name for a new Blog: ");
                        string name = Console.ReadLine() ?? "Unnamed Blog";

                        blog = new() {Name = name};

                        db.AddBlog(blog);
                        logger.Info("Blog added - {Name}", name);
                    } catch(DbUpdateException dbUpdateException) {
                        Console.WriteLine("There was an issue pushing your changes to the database, please try again.");
                        logger.Error($"Error while adding blog: {dbUpdateException.Message} - {dbUpdateException}");
                    }
                    break;
                case 'c':
                case '3':
                    Console.WriteLine("Which of the following blogs would you like to add to? Take note of their IDs...");
                    blog = GetBlogFromUser(db, logger);

                    Console.Write("Please enter the title of your post:\n" +
                                      "> ");
                    string title = Console.ReadLine() ?? "";
                    
                    Console.Write("Please enter the content of your post:\n" +
                                      "> ");
                    string content = Console.ReadLine() ?? "";

                    db.AddPost(blog, new Post {
                        Title = title,
                        Content = content
                    });

                    break;
                case 'v':
                case '4':
                    blog = GetBlogFromUser(db, logger);

                    foreach(var post in db.Posts.Where(post => post.BlogId == blog.BlogId).OrderBy(p => p.Title)) {
                        Console.WriteLine(
                            $"""
                            -!-!-!-
                            {post.Title}:
                            {post.Content}
                            """);
                    }
                    
                    break;

                default:
                    run = false;
                    break;
            }
        }

        logger.Info("Program ended");
    }

    private static void PrintBlogs(BloggingContext db, Logger logger) {
        try {
            IOrderedQueryable<Blog> query = db.Blogs.OrderBy(b => b.Name);

            Console.WriteLine("All blogs in the database:");
            foreach(var item in query) {
                Console.WriteLine($"{item.BlogId}. {item.Name}");
            }
        } catch(Exception e) {
            logger.Error($"Error during read to database: {e.Message} - {e}");
        }
    }

    private static Blog GetBlogFromUser(BloggingContext db, Logger logger) {
        PrintBlogs(db, logger);
        
        Blog? blog = null;
        
        while(blog is null) {
            int blogPk;
            bool parsed;
                        
            do {
                Console.Write("Enter a blog id:\n" +
                              "> ");
                parsed = int.TryParse(Console.ReadLine() ?? "", out blogPk);
            } while(!parsed);

            blog = db.Blogs.Find(blogPk);
                        
            if(blog is null) {
                logger.Warn($"Unable to find blog with pk: {blogPk}");
                Console.WriteLine("We were unable to find a blog with that id. Please try again.");
            }

            break;
        }

        return blog!;
    }
}