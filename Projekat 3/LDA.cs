using Microsoft.ML;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Data
{
    public string Text { get; set; }
}

public class TransformedData
{
    public string Text { get; set; }
    public float[] Features { get; set; }
}

public class LDA
{
    private static ConcurrentBag<string> _comments= new ConcurrentBag<string>();
    private static int count = 0;
    public static PredictionEngine<Data, TransformedData> PredictionEngine { get; set; }
    public static object consoleLogLocker = new object();

    public static void ProccessData(ConcurrentBag<string> coms)
    {
        _comments.Clear();
        _comments = coms;
        count= _comments.Count;
        
    }

    public static void TrainModel(int topicNum)
    {
        var mlContext = new MLContext();

        var samples = new List<Data>();
        for(int i = 0;i < count / 2; i++)
        {
            samples.Add(new Data{Text= _comments.ElementAt(i) });
        }

        var dataview = mlContext.Data.LoadFromEnumerable(samples);

        var pipeline = mlContext.Transforms.Text
            .NormalizeText("NormalizedText", "Text")
            .Append(mlContext.Transforms.Text.TokenizeIntoWords("Tokens","NormalizedText"))
            .Append(mlContext.Transforms.Text.RemoveDefaultStopWords("Tokens"))
            .Append(mlContext.Transforms.Conversion.MapValueToKey("Tokens"))
            .Append(mlContext.Transforms.Text.ProduceNgrams("Tokens"))
            .Append(mlContext.Transforms.Text.LatentDirichletAllocation("Features", "Tokens", numberOfTopics: topicNum));

        var transformer = pipeline.Fit(dataview);
        
        var predictionEngine = mlContext.Model.CreatePredictionEngine<Data,
            TransformedData>(transformer);
        PredictionEngine = predictionEngine;
    }


    public static string Predict(string text)
    {
        var prediction = PredictionEngine.Predict(new Data() { Text = text });

        string result = WriteResultsInConsole(prediction);
        return result;
    }

    private static string WriteResultsInConsole(TransformedData prediction)
    {
        string result = "";
        lock(ConsoleLogLocker.Locker)
        {
            for (int i = 0; i < prediction.Features.Length; i++)
            {
                //Console.Write($"{prediction.Features[i]:F4}  ");
                result += $"{prediction.Features[i]:F4}  ";
            }
            result += "\n";
            //Console.WriteLine();
        }
        
        return result;
    }

}