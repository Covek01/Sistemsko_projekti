using ResponseClasses;

public class SubredditCommentObserver : IObserver<SubredditEvaluatedResult>
{
    

    public void OnNext(SubredditEvaluatedResult result)
    {
        
    }
    public void OnError(Exception e)
    {
        Console.WriteLine($"Doslo je do greske: {e.Message}");
    }
    public void OnCompleted()
    {

    }
}