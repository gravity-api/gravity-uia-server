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

// Setup
ControllerUtilities.RednerLogo();
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

// invoke
app.Run();