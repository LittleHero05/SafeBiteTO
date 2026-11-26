using Amazon;
using Amazon.Extensions.NETCore.Setup;
using SafeBite.Client.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- Configuration: where is the API, and what is the key? ----------
// Locally (Development) the values come from appsettings.Development.json or user secrets.
// On Elastic Beanstalk they come from AWS Systems Manager Parameter Store, under /SafeBiteTO/.
// A parameter named /SafeBiteTO/SafeBiteApi/BaseUrl becomes the setting SafeBiteApi:BaseUrl.
if (builder.Configuration.GetValue<bool>("UseParameterStore"))
{
    builder.Configuration.AddSystemsManager(source =>
    {
        source.Path = "/SafeBiteTO";
        source.AwsOptions = new AWSOptions { Region = RegionEndpoint.USEast1 };
    });
}

var apiBaseUrl = builder.Configuration["SafeBiteApi:BaseUrl"]
    ?? throw new InvalidOperationException("SafeBiteApi:BaseUrl is not configured.");
var apiKey = builder.Configuration["SafeBiteApi:ApiKey"];
var apiKeyHeader = builder.Configuration["SafeBiteApi:ApiKeyHeader"] ?? "x-apikey";

// ---------- Services ----------
builder.Services.AddControllersWithViews();

// Typed HttpClient: every SafeBiteApiClient gets an HttpClient with the base address and the API key.
builder.Services.AddHttpClient<SafeBiteApiClient>(client =>
{
    // A trailing slash matters: "https://host/safebite/" + "api/establishments"
    client.BaseAddress = new Uri(apiBaseUrl.EndsWith('/') ? apiBaseUrl : apiBaseUrl + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
    if (!string.IsNullOrWhiteSpace(apiKey))
    {
        client.DefaultRequestHeaders.Add(apiKeyHeader, apiKey);   // Apigee's Verify API Key policy reads this
    }
});

var app = builder.Build();

// ---------- HTTP pipeline ----------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
