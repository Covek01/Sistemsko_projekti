using System.Net;
using System.Threading;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using Reddit;
using Reddit.Controllers;
using Reddit.Controllers.EventArgs;
using opennlp;
using opennlp.tools.tokenize;
using SharpEntropy;


public class  AuthorizationRedditApi
{
    public string ClientID{get; set;}
    public string Secret {get; set;}
    public static string RequestUrl = "https://www.reddit.com/api/v1/access_token";
    public static string RefreshTokenCode = "659780323952-AXILE13kRmZ_PtDqOZQ1oaempIdjnA";
    public string? Token {get; set;}
    //public const string key = "bzJ4bWlqVDR6MlBURjFTcGplS0UwQTpxc19NYVNaTzJRZERPcnduYV9Wb1VlUm4tOHdkYlE=";

    public const string UserAgent = "Topic modeler";
    public readonly static string BaseUrl = "https://oauth.reddit.com/"; 
    public RedditClient redditClient;
    public AuthorizationRedditApi(string id, string secret)
    {
        ClientID = id;
        Secret = secret;
        this.Token = null;
    }

    public AuthorizationRedditApi()
    {
        ClientID = null;
        Secret = null;
        this.Token = null;
    }

    public async Task SetRedditClient()
    {
        this.Token = (await this.RefreshToken()).Access_token;
        if (this.Token == null)
        {
            return;
        }
        redditClient = new RedditClient(ClientID, RefreshTokenCode, Secret,  this.Token, UserAgent);
    }

    public async Task<TokenRefreshResponse> RefreshToken()
    {
        HttpClient client = new HttpClient();
        //client.DefaultRequestHeaders.Add("Authorization", $"Basic {key}");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
        "Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{this.ClientID}:{this.Secret}")));
        client.DefaultRequestHeaders.Add("User-Agent", UserAgent);

        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),   
            new KeyValuePair<string, string>("refresh_token", RefreshTokenCode) 
        });

        formContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
        

        var response = await client.PostAsync(AuthorizationRedditApi.RequestUrl, formContent);
        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadAsStringAsync();
            TokenRefreshResponse data = JsonConvert.DeserializeObject<TokenRefreshResponse>(await response.Content.ReadAsStringAsync());
            this.Token = data.Access_token;
            return data;
        }
        else
        {
            Console.WriteLine("Error: " + response.StatusCode);
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Error Content: " + errorContent);
            return null;
        }
    }

    public async Task<string> ReturnAllComments(string subreddit)
    {
        try
        {
            string accessUrl = $"{AuthorizationRedditApi.BaseUrl}r/{subreddit}/api/info";

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.Token);
            client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
            var response = await client.GetAsync(accessUrl);
            response.EnsureSuccessStatusCode();

            var data = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
            Console.Write(await response.Content.ReadAsStringAsync());
            return "proba";
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.ToString());

        }
        return null;
    }

    public async Task<List<Comment>> ReturnAllCommentsBySubreddit(string subreddit, int postLimit, int commentsPerPostLimit)
    {
        try
        {
            string accessUrl = $"{AuthorizationRedditApi.BaseUrl}r/{subreddit}/api/info";

            var postsInSubreddit = redditClient.Subreddit(subreddit).Posts.GetNew(limit: postLimit);

            List<Comment> comments = new List<Comment>();
            foreach(var post in postsInSubreddit)
            {
                comments.AddRange(post.Comments.GetNew(limit: commentsPerPostLimit));
            }

            Console.WriteLine(comments);
            return comments;
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.ToString());

        }
        return null;
    }

    




}