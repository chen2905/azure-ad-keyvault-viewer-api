using azure_ad_keyvault_viewer_api.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on both HTTP (port 80) and HTTPS (port 7003)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80); // HTTP on port 80
    options.ListenAnyIP(7003, listenOptions => // HTTPS on port 7003
    {
        listenOptions.UseHttps(); // Ensure HTTPS is enabled
    });
});

// Configure the application to use specific URLs
builder.WebHost.UseUrls("http://*:80");

// Add services to the container
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});
var configuration = builder.Configuration;
string ClientId = configuration["AzureKeyVault:ClientId"];
string TenantId = configuration["AzureKeyVault:TenantId"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://login.microsoftonline.com/{TenantId}";
        options.Audience = ClientId;
    });


builder.Services.AddAuthorization();


builder.Services.AddControllers();
// Register the custom middleware

builder.Services.AddEndpointsApiExplorer(); // Required for API explorer (Swagger)
builder.Services.AddSwaggerGen(); // Adds Swagger generation

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Enable Swagger
    app.UseSwaggerUI(); // Enable Swagger UI
}


app.UseCors();
app.UseMiddleware<AzureAdJwtMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

// Add your custom middleware

app.MapControllers();

app.Run();
