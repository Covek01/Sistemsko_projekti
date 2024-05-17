

public class SubredditRoll
{
    public string SubredditName {get; set;}

    public static string clientId = "";
    public static string clientSecret = "";
    public AuthorizationRedditApi? RedditApi;

    public SubredditRoll(string name = null)
    {
        this.SubredditName = name;
        this.RedditApi = new AuthorizationRedditApi(clientId, clientSecret);
        
    }

    public async Task SetRedditClientProps()
    {
        await RedditApi.SetRedditClient();
        
    }

    public async Task<List<Reddit.Controllers.Comment>> ReturnAllCommentsPerNumber(int postNumber, int commentPerPostNumber)
    {
        var data = await RedditApi.ReturnAllCommentsBySubreddit(this.SubredditName, postNumber, commentPerPostNumber);

        return data;
    }
}
