using System;
using System.Text.Json.Serialization;

// Creates the application builder and loads the application configuration.
var builder = WebApplication.CreateBuilder(args);

// Registers controller services and configures JSON serialization.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Converts enum values to strings when sending and receiving JSON.
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

// Registers services needed to explore API endpoints.
builder.Services.AddEndpointsApiExplorer();

// Adds Swagger services for API documentation and testing.
builder.Services.AddSwaggerGen();

// Registers authorization services to manage access to API resources.
builder.Services.AddAuthorization();

// Enables in-memory storage for session data.
builder.Services.AddDistributedMemoryCache();

// Configures session management.
builder.Services.AddSession(options =>
{
    // Sets the session to expire after 30 minutes of inactivity.
    options.IdleTimeout = TimeSpan.FromMinutes(30);

    // Prevents client-side scripts from accessing the session cookie.
    options.Cookie.HttpOnly = true;

    // Ensures the session cookie is included in requests.
    options.Cookie.IsEssential = true;
});

// Builds the application using the configured services.
var app = builder.Build();

// Enables Swagger and Swagger UI only in the development environment.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirects HTTP requests to HTTPS.
app.UseHttpsRedirection();

// Enables session management for incoming requests.
app.UseSession();

// Enables authorization middleware.
app.UseAuthorization();

// Maps controller routes so the API endpoints can receive requests.
app.MapControllers();

// Starts the application and begins listening for requests.
app.Run();
