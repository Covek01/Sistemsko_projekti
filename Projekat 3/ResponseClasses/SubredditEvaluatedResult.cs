using System.Net;

namespace ResponseClasses
{
    public class SubredditEvaluatedResult
    {
        public string Text {get; set;}
        public string EvaluatedSubredditName {get; set;}
        public HttpListenerContext Context {get; set;}

        public SubredditEvaluatedResult(string text, string subreddit, HttpListenerContext context)
        {
            this.Text = text;
            this.EvaluatedSubredditName = subreddit;
            this.Context = context;
        }

    }
}
