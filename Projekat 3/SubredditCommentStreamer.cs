using System.IO;
using System.Text;
using System.Reactive.Subjects;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using Reddit;
using Reddit.Controllers;
using Reddit.Controllers.EventArgs;
using ResponseClasses;
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Text;
using Microsoft.ML.Trainers;
using System.Collections;
using System.Linq;
using System.Net;


public class SubredditCommentStreamer : IObservable<SubredditEvaluatedResult>
{
    private readonly Subject<SubredditEvaluatedResult> subject; 
    private List<SubredditRoll> subreddits;
    public readonly HttpListenerContext context;

    public SubredditCommentStreamer(HttpListenerContext context)
    {
        subject = new Subject<SubredditEvaluatedResult>();
        this.context = context;
    }

    public async Task<List<SubredditRoll>> SetSubreddits(List<string> subreddits)
    {
        var subredditsReturn = new List<SubredditRoll>();
        foreach(var sub in subreddits)
        {
            SubredditRoll subredditRoll = new SubredditRoll(sub);
            await subredditRoll.SetRedditClientProps();
            subredditsReturn.Add(subredditRoll);
        }
        return subredditsReturn;
    }

    public async Task<List<Comment>> ReturnAllCommentsForSubreddits(List<string> subreddits, int numOfPosts, int numOfCommentsPerPost)
    {
        try
        {
            List<SubredditRoll> subredditRolls = await this.SetSubreddits(subreddits);

            List<Comment> comments = new List<Comment>();
            foreach(var subredditRoll in subredditRolls)
            {
                List<Comment> commentsToAppend = await subredditRoll.RedditApi.ReturnAllCommentsBySubreddit(subredditRoll.SubredditName, numOfPosts, numOfCommentsPerPost);
                comments.AddRange(commentsToAppend);    //vraca jos komentare u listu
            }
            //return all comments for subreddit by calling getcomments from authorizationredditapi

            return comments;
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return null;
        }
    }

    public async Task<List<Comment>> ReturnAllCommentsBySubreddit(string subreddit, int numOfPosts, int numOfCommentsPerPost)
    {
        try
        {
            SubredditRoll subredditRoll = new SubredditRoll(subreddit);
            await subredditRoll.SetRedditClientProps();

            List<Comment> comments = await subredditRoll.RedditApi.ReturnAllCommentsBySubreddit(subredditRoll.SubredditName, numOfPosts, numOfCommentsPerPost);

            return comments;
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return null;
        }
    }

    public async Task PerformModeling(List<string> subreddits, int numOfSubreddits, int numOfPosts, int numOfCommentsPerPost)
    {
        string outputResponseForHTML = "";
        bool validRequest = true;
        if (subreddits.Count == 0)
        {
            lock(ConsoleLogLocker.Locker)
            {
                Console.WriteLine("\nThere is no subreddit to work with");
            }
        }
        try
        {
            foreach(var subreddit in subreddits)
            {
                lock(ConsoleLogLocker.Locker)
                {
                    Console.WriteLine($"\nPerforming topic modeling for first {numOfPosts * numOfCommentsPerPost} comments in r/{subreddit}...");
                    
                }
               
                List<Comment> comments = await this.ReturnAllCommentsBySubreddit(subreddit, numOfPosts, numOfCommentsPerPost);
                if (comments == null)
                {
                    //HttpServer.ReturnBadRequest(context, $"Error with fetching comments from r/{subreddit} or r/{subreddit} doesn't exist");
                    context.Response.OutputStream.Write
                    (Encoding.UTF8.GetBytes(HttpServer.createParagraph($"<br/> Subreddit r/{subreddit} doesn't exist <br/>")));
                    //return;
                    //throw new Exception($"Error with fetching comments from r/{subreddit} or r/{subreddit} doesn't exist");
                }
                else{
                     context.Response.OutputStream.Write(Encoding.UTF8.GetBytes
                    (HttpServer.createParagraph($"Performing topic modeling for first {numOfPosts * numOfCommentsPerPost} comments in r/{subreddit}..."))
                    );
                    ConcurrentBag<string> concurrentComments = new ConcurrentBag<string>();
                    foreach(var comment in comments)
                    {
                        concurrentComments.Add(comment.Body);
                    }

                    int numOfTopics = 5;
                    LDA.ProccessData(concurrentComments);
                    LDA.TrainModel(numOfTopics);

                    lock(ConsoleLogLocker.Locker)
                    {
                        Console.WriteLine("\n\nResults of topic modeling:");
                        outputResponseForHTML += HttpServer.createParagraph
                        ($"Results of topic modeling:");
                    }

                    foreach (var comment in concurrentComments)
                    {
                        SubredditEvaluatedResult resultTopic = new SubredditEvaluatedResult(LDA.Predict(comment), subreddit, context);
                        subject.OnNext(resultTopic);
                       
                    }
                }

                
            }

            context.Response.OutputStream.Write
            (Encoding.UTF8.GetBytes(HttpServer.createParagraph("Finished with topic modeling results for specified subreddits")));
            context.Response.OutputStream.Close();


            subject.OnCompleted(); 
            
        }
        catch(Exception ex)
        {
            subject.OnError(ex);
            context.Response.StatusCode = (int)(HttpStatusCode.InternalServerError);
            context.Response.StatusDescription = ex.Message;
            context.Response.OutputStream.Flush();
        }
    }



    public IDisposable Subscribe(IObserver<SubredditEvaluatedResult> observer)
    {
        return subject.Subscribe(observer);
    }



}





