using Microsoft.EntityFrameworkCore;
using SafeBite.Api.Data;
using SafeBite.Api.Repositories;
using SafeBite.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson();

builder.Services.AddDbContext<SafeBiteDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SafeBiteDb")));

// register repostirories
builder.Services.AddScoped<IEstablishmentRepository, EstablishmentRepository>();
builder.Services.AddScoped<IInspectionRepository, InspectionRepository>();
builder.Services.AddScoped<IOwnerResponseRepository, OwnerResponseRepository>();

// register automapper
builder.Services.AddAutoMapper(cfg => { }, typeof(Program));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi("safebite", options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info.Title = "SafeBite TO API";
        document.Info.Version = "v1";
        document.Info.Description = "Food safety badges for Toronto restaurants, built from Toronto Public Health's DineSafe data.";
        return Task.CompletedTask;
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SafeBiteDbContext>();
    await DbSeeder.SeedAsync(db, Path.Combine(AppContext.BaseDirectory, "SeedData"));
}

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/safebite.json", "SafeBite TO API v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
