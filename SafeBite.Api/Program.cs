using Microsoft.EntityFrameworkCore;
using SafeBite.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<SafeBiteDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SafeBiteDb")));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SafeBiteDbContext>();
    await DbSeeder.SeedAsync(db, Path.Combine(AppContext.BaseDirectory, "SeedData"));
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
