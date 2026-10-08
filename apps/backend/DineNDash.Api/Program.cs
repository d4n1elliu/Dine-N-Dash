using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Name of the CORS policy that lets the React app call this API
const string FrontendCorsPolicy = "Frontend";

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Allow requests from the React app, which runs on a different port
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins(builder.Configuration["Frontend:Url"] ?? "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Only show the API docs while developing
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors(FrontendCorsPolicy);
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();