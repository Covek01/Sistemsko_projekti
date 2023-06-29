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

public class SubredditCommentStreamer : IObservable<SubredditEvaluatedResult>
{
    private readonly Subject<SubredditEvaluatedResult> subject; 
    private List<SubredditRoll> subreddits;

    public SubredditCommentStreamer()
    {
        subject = new Subject<SubredditEvaluatedResult>();
        
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
                    throw new Exception($"Error with fetching comments from r/{subreddit}");
                }

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
                }
                foreach (var comment in concurrentComments)
                {
                    string topic = LDA.Predict(comment);
                    //subject.OnNext(topic)
                    int a = 1;
                }
            }
            


            // var commentTexts = comments.Select(comment => new CommentData { Text = comment.Body });

            //perform modeling
            
            
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }



    public IDisposable Subscribe(IObserver<SubredditEvaluatedResult> observer)
    {
        return subject.Subscribe(observer);
    }



}

public class CommentData
{
    [LoadColumn(0)]
    public string Text { get; set; }
}

public class Stats
{
    public float[] Score { get; set; }
}



// public static class TopicModeling
// {
//     public static void PerformTopicModeling(string[] documents, int numTopics)
//     {
//         MLContext mlContext = new MLContext();

//         // Define the data schema
//         var data = new List<TextData>();
//         for (int i = 0; i < documents.Length; i++)
//         {
//             data.Add(new TextData { Id = i.ToString(), Text = documents[i] });
//         }

//         var dataView = mlContext.Data.LoadFromEnumerable(data);

//         // Define the data preprocessing pipeline
//         var preprocessingPipeline = mlContext.Transforms.Text.NormalizeText("NormalizedText", "Text")
//             .Append(mlContext.Transforms.Text.TokenizeWords("Tokens", "NormalizedText"))
//             .Append(mlContext.Transforms.Text.RemoveDefaultStopWords("Tokens"))
//             .Append(mlContext.Transforms.Text.FeaturizeText("Features", "Tokens"));

//         // Apply the data preprocessing pipeline
//         var preprocessedData = preprocessingPipeline.Fit(dataView).Transform(dataView);

//         // Define the LDA estimator
//         var ldaEstimator = mlContext.Transforms.Conversion.MapKeyToValue("Label")
//             .Append(mlContext.Transforms.NormalizeMinMax("Features"))
//             .Append(mlContext.Transforms.Text.LatentDirichletAllocation("TopicFeatures", "Features"));

//         // Train the LDA model
//         var ldaModel = ldaEstimator.Fit(preprocessedData);

//         // Get the inferred topics and their word distributions
//         var ldaTransformer = ldaModel.LastTransformer;
//         var topicDistributions = ldaTransformer.GetTopics();

//         // Print the topics and their top words
//         for (int topicId = 0; topicId < numTopics; topicId++)
//         {
//             Console.WriteLine($"Topic {topicId}:");
//             var wordDistributions = ldaTransformer.GetTopicTerms(topicId, topTerms: 5);
//             foreach (var wordDistribution in wordDistributions)
//             {
//                 Console.WriteLine($"  {wordDistribution.Term}: {wordDistribution.Score}");
//             }
//             Console.WriteLine();
//         }
//     }

//     public class TextData
//     {
//         [LoadColumn(0)]
//         public string Id { get; set; }

//         [LoadColumn(1)]
//         public string Text { get; set; }
//     }
// }



