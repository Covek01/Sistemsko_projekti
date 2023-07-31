using ResponseClasses;
using System.Net;
using System.Threading;
using System.IO;
using System.Text;
using System.Runtime;
using System.Net;

public class HttpServer
{
    public readonly HttpListener listener;
    public List<string> Subreddits {get; set;}

    public HttpServer()
    {
        listener = new HttpListener();
    }


    public void StartServer(string[] prefixes)
        {
            if (listener.IsListening)
            {
                return; //already listening;
            }
            foreach (string p in prefixes)
            {
                listener.Prefixes.Add(p);
            }

            listener.Start();
            Console.WriteLine("Ready...");

            // this.streamer = new SubredditCommentStreamer();
            // this.observer = new SubredditCommentObserver();

            // var subscription = this.streamer.Subscribe(this.observer);
            

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                Task.Run(() => handleRequest(context));
            }

            //subscription.Dispose();

        }

        public async Task handleRequest(HttpListenerContext context)
        {
            List<string> subreddits = this.ParseQueryString(context);
            if (subreddits == null )
            {
                if (context.Request.QueryString.GetKey(0) == "subreddit")
                {
                    lock(ConsoleLogLocker.Locker)
                    {
                        context.Response.StatusCode = (int)(HttpStatusCode.BadRequest);
                        Console.WriteLine($"\n\nStatuss code: {context.Response.StatusCode},There is no subreddit for calculating");
                        context.Response.OutputStream.Close();
                    }
                }
                return;
            }
            int subredditCount = subreddits.Count;

            int numOfPosts = 10;
            int numOfCommentsPerPost = 10;

            var streamer = new SubredditCommentStreamer(context);
            var observer = new SubredditCommentObserver();

            var subscription = streamer.Subscribe(observer);
            
            await streamer.PerformModeling(subreddits, subredditCount, numOfPosts, numOfCommentsPerPost);


            subscription.Dispose();
        }

        public static string createParagraph(string text)
        {
            return "<p>" + text + "</p>";
        }

        public List<string> ParseQueryString(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            List<string> subreddits = new List<string>();
            bool subredditFound = false;

            string key = request.QueryString.GetKey(0);
            if (key == null)
            {
                ReturnBadRequest(context, "bad query string");
            }
            
            if (key == "subreddit")
            {
                var value = request.QueryString[key];

                var subredditValues = value.Split(',');

                if (subredditValues.Length > 0)
                {
                    foreach (var subredditValue in subredditValues)
                    {
                        subreddits.Add(Uri.EscapeDataString(subredditValue));
                    }
                }
                subredditFound = true;
            }
            else
            {
                ReturnBadRequest(context, "bad query string");
            }
            
            if (subredditFound == true)
            {
                return subreddits;
            }
            else
            {
                return null;
            }

        }

        public static void ReturnBadRequest(HttpListenerContext context, string message)
        {
            lock(ConsoleLogLocker.Locker)
            {
                context.Response.StatusCode = (int)(HttpStatusCode.BadRequest);
                context.Response.StatusDescription = message;
                Console.WriteLine($"\n\nStatus code: {context.Response.StatusCode}, {message}");
                context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(HttpServer.createParagraph("Bad request")));
                context.Response.OutputStream.Close();
            }
        }

        public void StopServer()
        {
            if (listener.IsListening)
            {
                listener.Stop();
            }
        }
}