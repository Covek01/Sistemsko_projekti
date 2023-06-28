var definition = new {access_token = "", expires_in = 0, refresh_token = "", scope = ""};

public class TokenRefreshResponse
{
    public string Access_token {get; set;}
    public int Expires_in {get; set;}
    public string Refresh_token {get; set;}
    public string Scope { get; set; }

    public TokenRefreshResponse(string token, int expires, string refresh_token, string scope)
    {
        this.Access_token = token;
        this.Expires_in = expires;
        this.Refresh_token = refresh_token;
        this.Scope = scope;
    }
}