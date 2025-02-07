using System.Net;
using CyclingStats.Logic.Configuration;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;

namespace CyclingStats.Logic.Services;

public class SecuredSession
{
    private readonly WcsOptions wcsSettings;
    private readonly HttpClient httpClient;
    //The cookies will be here.
    private static CookieContainer cookieContainer ;
    private bool isAuthenticated = false;
    public SecuredSession(IOptions<WcsOptions> wcsOptions)
    {
        wcsSettings = wcsOptions.Value;
        cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            UseCookies = true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        httpClient = new HttpClient(handler);
    }
    
    //In case you need to clear the cookies
    public void ClearCookies()
    {
        isAuthenticated = false;
        cookieContainer = new CookieContainer();
    }

    public async Task<HtmlDocument> GetAuthenticatedPageAsync(string url)
    {
        if (!isAuthenticated)
        {
            await AuthenticateAsync();
        }
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return doc;
    }
    
    private async Task<bool> AuthenticateAsync()
    {
        var loginData = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("user_name", wcsSettings.Username),
            new KeyValuePair<string, string>("user_pass", wcsSettings.Password),
            new KeyValuePair<string, string>("user_login", string.Empty),
        ]);
        
        var response = await httpClient.PostAsync(wcsSettings.LoginUrl, loginData);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        isAuthenticated = body.Contains(wcsSettings.Username, StringComparison.InvariantCultureIgnoreCase);
        return isAuthenticated;
    }
    

}