/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using System.Text.Json.Serialization;

using System.Text.Json;

using UiaWebDriverServer.Extensions;
using UiaWebDriverServer.Domain.Formatters;
using UiaWebDriverServer.Domain.Converters;
using Microsoft.AspNetCore.Http;
using UiaWebDriverServer.Domain;
using System.Collections.Generic;
using UiaWebDriverServer.Contracts;
using System.Collections.Concurrent;
using UiaWebDriverServer.Domain.Application;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Text;
using System;
using Gravity.Abstraction.Cli;

// constatns
const string Configuration = "config";
const string Hub = "hub";
const string HubPort = "hubPort";
const string Host = "host";
const string Port = "port";
const string Register = "register";
const string Tags = "tags";
const string BrowserName = "browserName";

// Setup
ControllerUtilities.RednerLogo();
var cli = "{{$ " + string.Join(" ", args) + "}}";
var arguments = new CliFactory().Parse(cli);
var builder = WebApplication.CreateBuilder(args);

#region *** Url & Kestrel ***
builder.WebHost.UseUrls();
#endregion

#region *** Service       ***
// application
builder.Services.AddRouting(i => i.LowercaseUrls = true);

// formats & serialization
builder.Services
    .AddControllers(i => i.InputFormatters.Add(new TextPlainInputFormatter()))
    .AddJsonOptions(i =>
    {
        i.JsonSerializerOptions.WriteIndented = true;
        i.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        i.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        i.JsonSerializerOptions.Converters.Add(new UiaWebDriverServer.Domain.Converters.TypeConverter());
        i.JsonSerializerOptions.Converters.Add(new ExceptionConverter());
    });

// open api
builder.Services.AddSwaggerGen(i =>
{
    i.SwaggerDoc("v1", new OpenApiInfo { Title = "UIA Driver Server", Version = "v1" });
    i.OrderActionsBy(a => a.HttpMethod);
    i.EnableAnnotations();
});

// cookies & CORS
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = _ => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});
builder
    .Services
    .AddCors(o => o.AddPolicy("CorsPolicy", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
#endregion

#region *** Dependencies  ***
// server state
builder.Services.AddSingleton<IDictionary<string, Session>>(new ConcurrentDictionary<string, Session>());

// domain
builder.Services.AddTransient<IElementRepository, ElementRepository>();
builder.Services.AddTransient<ISessionRepository, SessionRepository>();
builder.Services.AddTransient<IUiaDomain, UiaDomain>();
#endregion

#region *** Configuration ***
// build
var app = builder.Build();

// development settings
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// build
app.UseCookiePolicy();
app.UseCors("CorsPolicy");
app.UseSwagger();
app.UseSwaggerUI(i =>
{
    i.SwaggerEndpoint("/swagger/v1/swagger.json", "UIA Driver Server v1");
    i.DisplayRequestDuration();
    i.EnableFilter();
    i.EnableTryItOutByDefault();
});
app.UseRouting();
app.MapDefaultControllerRoute();
app.MapControllers();
#endregion

#region *** Register Grid ***
// register
if (arguments.ContainsKey(Register) || arguments.ContainsKey(Configuration))
{
    RegisterNode(arguments);
}

static void RegisterNode(IDictionary<string, string> arguments)
{
    // setup
    var options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // build
    var host = arguments.TryGetValue(Host, out string hostOut) ? hostOut : "localhost";
    var port = arguments.TryGetValue(Port, out string portValue) && int.TryParse(portValue, out int portOut)
        ? portOut
        : 5555;
    var hub = arguments.TryGetValue(Hub, out string hubOut) ? hubOut : "localhost";
    var hubPort = arguments.TryGetValue(HubPort, out string hubPortValue) && int.TryParse(hubPortValue, out int hubPortOut)
        ? hubPortOut
        : 4444;
    var tags = arguments.TryGetValue(Tags, out string tagsOut)
        ? GetTags(tagsOut)
        : new Dictionary<string, string>();
    var browserName = arguments.TryGetValue(BrowserName, out string browserNameOut) ? browserNameOut : "UIA";
    var nodeConfiguration = GetNodeConfiguration(port, hubPort, host, browserName, tags);

    // setup
    var content = JsonSerializer.Serialize(nodeConfiguration, options);
    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
    var request = new HttpRequestMessage
    {
        Content = stringContent,
        Method = HttpMethod.Post,
        RequestUri = new Uri($"http://{hub}:{hubPort}/grid/register/")
    };

    // invoke
    using var clinet = new HttpClient();
    var response = clinet.SendAsync(request).GetAwaiter().GetResult();

    // assert
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine($"Register-Node -Host {host}:{port} -Hub {hub}:{hubPort} = Ok");
        return;
    }
    var message = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
    throw new ArgumentException(message, nameof(arguments));
}

static IDictionary<string, string> GetTags(string tags)
{
    // setup
    var _tags = tags.Split(";");
    var outcome = new Dictionary<string, string>();

    // build
    foreach (var tag in _tags)
    {
        var _tag = tag.Split("=");
        outcome[_tag[0].Trim()] = _tag[1].Trim();
    }

    // get
    return outcome;
}

static object GetNodeConfiguration(int port, int hubPort, string host, string browserName, IDictionary<string, string> tags)
{
    // setup
    var capabilities = new Dictionary<string, object>
    {
        ["browserName"] = browserName,
        ["browserVersion"] = "1.0",
        ["platform"] = "WINDOWS",
        ["maxInstances"] = 1,
        ["role"] = "WebDriver"
    };
    foreach (var tag in tags)
    {
        capabilities[tag.Key] = tag.Value;
    }

    // get
    return new
    {
        Capabilities = new[]
        {
                    capabilities
                },
        Configuration = new
        {
            _comment = "Configuration for Windows, UIAutomation based Node.",
            CleanUpCycle = 2000,
            Timeout = 30000,
            Port = port,
            Host = host,
            Register = true,
            HubPort = hubPort,
            MaxSessions = 1
        }
    };
}
#endregion

// invoke
app.Run();