<img src="https://github.com/PortiaNet/HealthCheck.Reporter/blob/master/Assets/banner.png" width="400">


[![Nuget](https://img.shields.io/nuget/v/PortiaNet.HealthCheck.Writer.HTTP?label=NuGet)](https://www.nuget.org/packages/PortiaNet.HealthCheck.Writer.HTTP/)

This library is the writer for the [PortiaNet.HealthCheck.Reporter](https://github.com/PortiaNet/HealthCheck.Reporter) and pushes the API call reports to a specific URL using HTTP methods.

### Installation

You can install this tool from [Nuget](https://www.nuget.org/packages/PortiaNet.HealthCheck.Writer.HTTP) using Package Manager Console:

```
PM> Install-Package PortiaNet.HealthCheck.Writer.HTTP
```

### How do I get started?

After adding the [HealthCheck](https://github.com/PortiaNet/HealthCheck.Reporter/blob/master/PortiaNet.HealthCheck.Reporter/HealthCheckMiddleware.cs) middleware to the application pipeline, you need to add this library to DI and configure it as you prefer.

``` C#
using PortiaNet.HealthCheck.Writer;

var builder = WebApplication.CreateBuilder(args);
...
builder.Services.AddHTTPWriter(options =>
{
    options.ListenerAddress = new Uri("Target API to push the report");
    options.MuteOnError = false;
    options.AuthenticationType = AuthenticationType.None;
    options.NodeName = "Main Node without authentication";
});

builder.Services.AddHTTPWriter(options =>
{
    options.ListenerAddress = new Uri("Target API to push the report");
    options.MuteOnError = false;
    options.AuthenticationType = AuthenticationType.StaticBearerToken;
    options.NodeName = "Main Node by Static Bearer Token";
    options.AuthenticationConfig = new StaticBearerTokenAuthentication
    {
        Token = "A very hard and long super secret token!!!"
    };
});

builder.Services.AddHTTPWriter(options =>
{
    options.ListenerAddress = new Uri("Target API to push the report");
    options.MuteOnError = false;
    options.AuthenticationType = AuthenticationType.ClientSecretBearerToken;
    options.NodeName = "Main Node by ClientSecret";
    options.AuthenticationConfig = new ClientSecretBearerTokenAuthentication
    {
        AuthenticationAPIPath = new Uri("Authentication API"),
        ClientSecret = "***ClientSecretText&&&"
    };
});

builder.Services.AddHTTPWriter(options =>
{
    options.ListenerAddress = new Uri("Target API to push the report");
    options.MuteOnError = false;
    options.AuthenticationType = AuthenticationType.UsernamePasswordBearerToken;
    options.NodeName = "Main Node by Username and Password";
    options.AuthenticationConfig = new UsernamePasswordBearerTokenAuthentication
    {
        AuthenticationAPIPath = new Uri("Authentication API"),
        Username = "TestUser",
        Password = "P@ssvor3d"
    };
});
...
```

This library supports 4 types of communication with the target service:
1. Without Authentication

In the first method, the library posts the report to the target API without setting the Authorization header.

2. Static Bearer Token Authentication (Doesn't need to call authentication API)

In the second method, the library sets the Authorization header with the *Bearer* prefix and the exact value of the specified token for all API calls.

3. Client Secret Authentication

In the third method, the library posts the specified Client Secret to the target authentication API to receive the bearer token. after receiving the token, it sets the Authorization header with the *Bearer* prefix and the exact value of the received token for all API calls. In this case, you need to have a **POST** API endpoint to receive the Client Secret from the request body and send the token back as the method result with a 200 status code.
``` C#
[HttpPost]
public IActionResult AuthenticateByClientSecret([FromBody]string clientSecret)
{
    if(string.IsNullOrEmpty(clientSecret))
        return Unauthorized();

    return Ok("Bearer Token");
}
```

4. Username and Password Authentication 

In the fourth method, the library posts the specified Username and Password to the target authentication API to receive the bearer token. after receiving the token, it sets the Authorization header with the *Bearer* prefix and the exact value of the received token for all API calls. In this case, you need to have a **POST** API endpoint to receive the credentials from the request body and send the token back as the method result with a 200 status code.
``` C#
public class UsernamePasswordModel
{
    public string? Username { get; set; }

    public string? Password { get; set; }
}

[HttpPost]
public IActionResult AuthenticateByUsernamePassword([FromBody] UsernamePasswordModel credential)
{
    if (credential == null || credential.Username != "Username" && credential.Password != "Password")
        return Unauthorized();

    return Ok("Bearer Token");
}
```

After specifying the authentication method, you need to configure the target API to receive and save the reports. This API should be a **POST** method and receives the Reporter model.
``` C#
public class RequestDetail
{
    public string? IpAddress { get; set; }

    public string? Username { get; set; }

    public string? Host { get; set; }

    public string? Method { get; set; }

    public string? Path { get; set; }

    public string? QueryString { get; set; }

    public string? UserAgent { get; set; }

    public double Duration { get; set; }

    public bool HadError { get; set; }

    public string? NodeName { get; set; }

    public DateTime EventDateTime { get; set; }
}

[HttpPost]
public IActionResult SaveReport([FromBody] RequestDetail report)
{
    Debugger.Log(0, null, JsonSerializer.Serialize(report));
    return Ok();
}
```

There is an option in the configuration to dump a set of logs instead of sending them one by one. To use this functionality, you need to set the **BulkDataDumpingEnabled** to *true*. Then the report writer will create a set of records based on the limitation has been set by **DataDumpingSize** and sends it in one package. In this case, the target API should receive a set of _RequestDetail_ instead of an individual item.
The following sample shows the configuration for the bulk data dumping:
``` c#
builder.Services.AddHTTPWriter(options =>
{
    options.ListenerAddress = new Uri("Target API to push the report");
    options.MuteOnError = false;
    options.AuthenticationType = AuthenticationType.None;
    options.NodeName = "Main Node Bulk Reprot";
    options.BulkDataDumpingEnabled = true;
    options.DataDumpingSize = 100;
});
```

And the following sample shows an instance of the report listener:
``` c#
[HttpPost]
public IActionResult SaveBulkReport([FromBody] List<RequestDetail> report)
{
    Debugger.Log(0, null, JsonSerializer.Serialize(report));
    return Ok();
}
```