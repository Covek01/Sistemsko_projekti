using ResponseClasses;
using System.Net;
using System.Threading;
using System.IO;
using System.Text;
using System.Runtime;
using System.Net;

public class SubredditCommentObserver : IObserver<SubredditEvaluatedResult>
{
    private SubredditEvaluatedResult Result {get; set;}
    private string HTMLResult {get; set;}
    private string HTMLResponse {get; set;}
    private async Task PrintObservedValuesToConsole(SubredditEvaluatedResult result)
    {
        lock(ConsoleLogLocker.Locker)
        {
            Console.WriteLine(result.Text);
            //Console.WriteLine();
        }
        
        result.Context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(HttpServer.createParagraph(result.Text)));
    }
    public void OnNext(SubredditEvaluatedResult result)
    {
        this.PrintObservedValuesToConsole(result);
    }
    public void OnError(Exception e)
    {
        Console.WriteLine($"Error: {e.Message}");
    }
    public void OnCompleted()
    {
        Console.WriteLine($"Finished with topic modeling results for specified subreddits");
        
    }

    public void SetResponse(string result)
    {
        this.HTMLResponse = result;
    }
}