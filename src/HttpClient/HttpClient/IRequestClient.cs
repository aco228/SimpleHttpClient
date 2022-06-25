using System.Net.Http.Headers;

namespace HttpClient;

public interface IRequestClient
{
    Task<HttpResponseMessage> GetResponse(string url);
    Task<string> Get(string url);
    Task<string> Get<T>(string url, T obj);
    Task<T> Get<T>(string url, object obj);
    Task<T> Get<T>(string url);
        
    Task<string> Patch<T>(string url, T obj);
    Task<T> Patch<T>(string url, object obj);
        
    Task<string> Put<T>(string url, T obj);
    Task<T> Put<T>(string url, object obj);

    Task Delete(string url);
        
    Task<string> Post<T>(string url, T obj);
    Task<T> Post<T>(string url, object obj);

    void AddAuthorization(string value);
    void AddAuthorization(AuthenticationHeaderValue value);
    void AddDefaultHeader(string key, string value);
    string ParamsToStringAsync<T>(T obj);
    string ParamsToStringAsync(Dictionary<string, string>? urlParams);
}