using DineNDash.Api.Extensions;
using DineNDash.Api.Middleware;
using DineNDash.Core.Interfaces;
using DineNDash.Data;
using DineNDash.Data.Repositories;
using DineNDash.Data.Storage;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using DineNDash.Core.Services;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "Frontend";

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IBookingAnalyticsService, BookingAnalyticsService>();

// Relative paths are resolved from the API project folder, so uploads stay next to the running backend
var imagesPath = Path.Combine(builder.Environment.ContentRootPath, builder.Configuration["Storage:ImagesPath"] ?? "uploads/images");
builder.Services.AddSingleton<IImageStorage>(new LocalImageStorage(imagesPath));
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins(builder.Configuration["Frontend:Url"] ?? "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Migrations run in every environment so a fresh production database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors(FrontendCorsPolicy);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();