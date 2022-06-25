using Aco228.SimpleHttpClientExample.DummyModels;

namespace Aco228.SimpleHttpClientExample;

public class DerivateOfSomeHttpClient : SomeHttpClient
{
    public Task<SomeModel> GetRequest(string url)
        => Get<SomeModel>(url);
    
    // this request will be translated to query string
    public Task<SomeModel> GetRequest(string url, SomeRequest request)
        => Get<SomeModel>(url, request);

    public Task<SomeModel> PostRequest(string url, SomeRequest request)
        => Post<SomeModel>(url, request);
    
    public Task<SomeModel> PostRequest(string url)
        => Post<SomeModel>(url, (object)null);
    
    public Task<SomeModel> PutRequest(string url, SomeRequest request)
        => Put<SomeModel>(url, request);
}