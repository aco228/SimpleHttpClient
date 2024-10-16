using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Aco228.SimpleHttpClient.Exceptions;

namespace Aco228.SimpleHttpClient;

public class RequestClient : IRequestClient, IDisposable
{
    private string BaseUrl { get;  set; }
    private string? _contentType = null;
    private HttpClient _client = new();

    public RequestClient() { }
    
    public RequestClient(string baseUrl)
    {
        BaseUrl = baseUrl;
    }
    
    protected virtual void OnDispose(){}

    public void Dispose()
    {
        OnDispose();
        _client?.Dispose();
    }

    protected void SetBaseString(string baseString)
    {
        BaseUrl = baseString;
    }

    protected void ReplaceHttpClient(HttpClient httpClient)
    {
        _client = httpClient;
    }
    
    protected HttpClient GetHttpClient() => _client;

    protected void SetContentType(string type) => _contentType = type;
    protected string? GetContentType() => _contentType;

    protected virtual string GetUrl(string url)
        => string.IsNullOrEmpty(BaseUrl) ? url :
            url.StartsWith(BaseUrl) ? url : $"{BaseUrl}{url}";

    protected virtual (string, StringContent) GetRequestData(string url, object data)
        => (GetUrl(url), GetContent(data));

    protected virtual void OnResponseReceived(HttpResponseMessage responseMessage)
    {
    }

    protected virtual Task OnBeforeRequest(string url, object? data = null)
        => Task.FromResult(true);

    protected virtual StringContent OnAddingHeaders(StringContent content)
    {
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return content;
    }

    public Task<HttpResponseMessage> GetResponse(string url)
        => _client.GetAsync(url);

    private StringContent GetContent(object obj)
    {
        var data = string.Empty;

        if (obj != null)
        {
            if (obj is string)
                data = obj as string;
            else
            {
                data = JsonConvert.SerializeObject(obj);
                if (data == null)
                    throw new ArgumentException("Object could not be translated to dictionary");
            }
        }

        var content = OnAddingHeaders(new StringContent(data, Encoding.UTF8, _contentType));
        return content;
    }


    #region # GET #

    public Task<string> Get(string url)
        => Get<string>(url, null);

    public async Task<T> Get<T>(string url, object obj)
    {
        var response = await Get(GetUrl(url), obj);
        return JsonConvert.DeserializeObject<T>(response);
    }

    public async Task<T> Get<T>(string url)
    {
        var response = await Get(GetUrl(url));
        return JsonConvert.DeserializeObject<T>(response);
    }

    public async Task<string> Get<T>(string url, T obj)
    {
        await OnBeforeRequest(url, obj);

        var query = string.Empty;
        if (obj != null)
        {
            var data = JObject.FromObject(obj).ToObject<Dictionary<string, string>>();
            query = ParamsToStringAsync(data);
        }

        if (!string.IsNullOrEmpty(query))
            query = (url.Contains("?") ? "&" : "?") + query;

        HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, GetUrl(url) + query);
        httpRequest.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);

        var response = await _client.SendAsync(httpRequest);
        OnResponseReceived(response);
        
        StringContent requestContent = OnAddingHeaders(new StringContent("", Encoding.UTF8, _contentType));
        EnsureSuccessStatusCode(response, GetUrl(url) + query, requestContent);
        return await response.Content.ReadAsStringAsync();
    }

    #endregion

    #region # POST

    public async Task<string> Post<T>(string url, T obj)
    {
        await OnBeforeRequest(url, obj);

        (string requestUrl, StringContent requestContent) = GetRequestData(url, obj);
        var response = await _client.PostAsync(requestUrl, requestContent);
        OnResponseReceived(response);
        EnsureSuccessStatusCode(response, requestUrl, requestContent);
        var stringResponse = await response.Content.ReadAsStringAsync();
        return stringResponse;
    }

    public async Task<T> Post<T>(string url, object obj)
    {
        var response = await Post(url, obj);
        if (string.IsNullOrEmpty(response))
            return default(T);
        
        return JsonConvert.DeserializeObject<T>(response);
    }

    #endregion

    #region # PATCH

    public async Task<string> Patch<T>(string url, T obj)
    {
        await OnBeforeRequest(url, obj);
        (string requestUrl, StringContent requestContent) = GetRequestData(url, obj);
        var response = await _client.PatchAsync(requestUrl, requestContent);
        OnResponseReceived(response);
        EnsureSuccessStatusCode(response, requestUrl, requestContent);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<T> Patch<T>(string url, object obj)
    {
        var response = await Patch(url, obj);
        if (string.IsNullOrEmpty(response))
            return default(T);
        
        return JsonConvert.DeserializeObject<T>(response);
    }

    #endregion

    #region # PUT

    public async Task<string> Put<T>(string url, T obj)
    {
        await OnBeforeRequest(url, obj);
        (string requestUrl, StringContent requestContent) = GetRequestData(url, obj);
        var response = await _client.PutAsync(requestUrl, requestContent);
        OnResponseReceived(response);
        EnsureSuccessStatusCode(response, requestUrl, requestContent);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<T> Put<T>(string url, object obj)
    {
        var response = await Put(url, obj);
        return JsonConvert.DeserializeObject<T>(response);
    }

    #endregion

    #region # DELETE

    public async Task Delete(string url)
    {
        await OnBeforeRequest(url, null);
        var response = await _client.DeleteAsync(GetUrl(url));
        EnsureSuccessStatusCode(response, GetUrl(url), null);
    }

    #endregion

    public void AddAuthorization(string value)
        => _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", value);

    public void AddAuthorization(AuthenticationHeaderValue value)
        => _client.DefaultRequestHeaders.Authorization = value;

    public void AddDefaultHeader(string key, string value)
    {
        if (_client.DefaultRequestHeaders.Any(x => x.Key.Equals(key)))
            _client.DefaultRequestHeaders.Remove(key);
        
        _client.DefaultRequestHeaders.Add(key, value);
    }

    public string ParamsToStringAsync<T>(T obj)
    {
        if (obj == null)
            return string.Empty;
        
        var data = JObject.FromObject(obj).ToObject<Dictionary<string, string>>();
        return ParamsToStringAsync(data);
    }

    public string ParamsToStringAsync(Dictionary<string, string>? urlParams)
    {
        if (urlParams == null || urlParams.Count == 0)
            return string.Empty;

        var query = new List<string>();
        foreach (var param in urlParams)
            if (!string.IsNullOrEmpty(param.Value))
                query.Add($"{param.Key}={HttpUtility.HtmlEncode(param.Value)}");

        return string.Join("&", query);
    }

    protected virtual void OnException(RequestException exception)
    {
        throw exception;
    }
    
    private void EnsureSuccessStatusCode(HttpResponseMessage response, string url, StringContent request)
    {
        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (Exception exception)
        {
            OnException(new RequestException(exception, url, request, response));
        }
    }
}
