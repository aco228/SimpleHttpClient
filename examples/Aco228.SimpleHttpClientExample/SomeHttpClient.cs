using Aco228.SimpleHttpClient;

namespace Aco228.SimpleHttpClientExample;

public class SomeHttpClient : RequestClient, ISomeHttpClient
{
    public SomeHttpClient ()
    {
        // setting base url for future use
        SetBaseString("https://example.com/");
        
        // adding Bearer authorization
        AddAuthorization("some key");
        
        // Adding default headers that will be used in every next request
        AddDefaultHeader("Some default header", "header value");
        AddDefaultHeader("Some default header 2", "header value");
    }
    
    // adding some extra logic
}