using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;

namespace DemoDapper
{
    class Program
    {
        static string connectionString = @"<CONNECTION STRING HERE>";

        static void Main(string[] args)
        {
            ShowPostsTyped();
            ShowPostsADO();

            InsertPost();

            ShowPostsDynamic();

            ShowCommentsJoinPost();

            Console.WriteLine("end...");
            Console.ReadLine();
        }


        static void ShowPostsTyped()
        {
            Console.WriteLine("showing typed posts");

            var posts = GetPostsTyped();

            // showing the results
            foreach (var post in posts)
                Console.WriteLine($"{post.Title} - {post.Description}");

            Console.WriteLine();
        }

        static IEnumerable<BlogPost> GetPostsTyped()
        {
            IEnumerable<BlogPost> result;

            // normal/usual connection
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // the magic, a typed and simple query
                result = connection.Query<BlogPost>("Select * From BlogPosts");
            }

            return result;
        }


        static void ShowPostsADO()
        {
            Console.WriteLine("showing posts by ADO");

            var posts = GetPostsADO();

            // showing the results
            foreach (var post in posts)
                Console.WriteLine($"{post.Title} - {post.Description}");

            Console.WriteLine();
        }

        static IEnumerable<BlogPost> GetPostsADO()
        {
            var result = new List<BlogPost>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand("Select * From BlogPosts", connection))
            {
                connection.Open();

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var post = new BlogPost();
                    post.Title = reader.GetString(reader.GetOrdinal("Title"));
                    post.Description = reader.GetString(reader.GetOrdinal("Description"));
                    result.Add(post);
                }

            }

            return result;
        }


        static void ShowPostsDynamic()
        {
            Console.WriteLine("showing dynamic posts");

            var posts = GetPostsDynamic();

            // showing the results
            foreach (var post in posts)
                Console.WriteLine($"{post.Title} - {post.Description}");

            Console.WriteLine();
        }

        static IEnumerable<dynamic> GetPostsDynamic()
        {
            IEnumerable<dynamic> result;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                // with parameters
                result = connection.Query("Select * From BlogPosts where Title like '%' + @title + '%'", new { title = "first" });
            }

            return result;
        }


        static void ShowCommentsJoinPost()
        {
            Console.WriteLine("showing comments and its post");

            var comments = GetCommentsJoinPost();

            // showing the results
            foreach (var comment in comments)
                Console.WriteLine($"{comment.Message}: {comment.BlogPost.Title} - {comment.BlogPost.Description}");

            Console.WriteLine();
        }

        static IEnumerable<Comment> GetCommentsJoinPost()
        {
            IEnumerable<Comment> result;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // join comments with post
                result = connection.Query<Comment, BlogPost, Comment>(
                    "Select * From Comments c Join BlogPosts b on c.BlogPost_Id = b.Id",
                    (comment, post) => { comment.BlogPost = post; return comment; });
            }

            return result;
        }


        static void InsertPost()
        {
            var entity = new BlogPost
            {
                Title = "Post inserted by dapper",
                Description = "Bla bla bla..."
            };

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                connection.Execute("insert BlogPosts (Title, Description) values (@Title, @Description)", entity);
            }
        }


        public class Comment
        {
            public int Id { get; set; }
            public string Message { get; set; }
            public BlogPost BlogPost { get; set; }
        }

        public class BlogPost
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public List<Comment> Comments { get; set; }
        }
    }
}
