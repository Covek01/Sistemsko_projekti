using Newtonsoft.Json;

public class TokenRefreshResponse
{
    [JsonProperty("access_token")]
    public string Access_token {get; set;}
    [JsonProperty("expires_in")]
    public int Expires_in {get; set;}
    [JsonProperty("refresh_token")]
    public string Refresh_token {get; set;}
    [JsonProperty("scope")]
    public string Scope { get; set; }

    public TokenRefreshResponse(string token, int expires, string refresh_token, string scope)
    {
        this.Access_token = token;
        this.Expires_in = expires;
        this.Refresh_token = refresh_token;
        this.Scope = scope;
    }
}