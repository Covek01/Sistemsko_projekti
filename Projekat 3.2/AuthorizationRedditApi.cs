// using System.Net;
// using System.Threading;
// using System.IO;
// using System.Text;
// using Newtonsoft.Json;


// public class  AuthorizationRedditApi
// {
//     public string ClientID{get; set;}
//     public string Secret {get; set;}
//     public static string RequestUrl = "https://www.reddit.com/api/v1/access_token";
//     public static string RefreshTokenCode = "659780323952-AXILE13kRmZ_PtDqOZQ1oaempIdjnA";
//     public string Token {get; set;}

//     public AuthorizationRedditApi(string id = null, string secret = null)
//     {
//         ClientID = id;
//         Secret = secret;
//     }


//     public async Task<string> RefreshToken()
//     {
//         HttpClient client = new HttpClient();
//         var formContent = new FormUrlEncodedContent(new[]
//         {
//             new KeyValuePair<string, string>("grant_type", "refresh_token"),   
//             new KeyValuePair<string, string>("refresh_token", RefreshTokenCode) 
//         });
//         HttpResponseMessage response = await client.PostAsync(AuthorizationRedditApi.RequestUrl, formContent);

//         string responseString = await response.Content.ReadAsStringAsync();
//         TokenRefreshResponse data = JsonConvert.DeserializeObject<TokenRefreshResponse>(responseString);

//         return data.Access_token;
//     }
// }