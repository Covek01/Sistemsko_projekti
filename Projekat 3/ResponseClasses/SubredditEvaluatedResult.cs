
namespace ResponseClasses
{
    public class SubredditEvaluatedResult
    {
        public Dictionary<string, double> TopicResults;

        public SubredditEvaluatedResult()
        {
            TopicResults = new Dictionary<string, double>();
        }
    }
}
