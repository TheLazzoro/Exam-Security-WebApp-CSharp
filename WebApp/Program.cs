using WebApp.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using WebApp.ErrorHandling;
using WebApp;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var configurationString = builder.Configuration.GetConnectionString("Default");
var configurationString_Startup = builder.Configuration.GetConnectionString("Startup");
SQLConnection.connectionString = configurationString;
SQLConnection.connectionString_Startup = configurationString_Startup;

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<MySqlConnection>(_ =>
    new MySqlConnection(configurationString));

// JWT
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        //ValidIssuer = config["Jwt:Issuer"],
        //ValidAudience = config["Jwt:Audience"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(SharedSecret.GetSharedKey()),
    };
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("corspolicy", build =>
    {
        build.WithOrigins("http://localhost:5173").AllowAnyMethod().AllowAnyHeader(); // Enables a single domain
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
// All requests are routed through the middleware before they get to the endpoint method.
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    Globals.IsDevelopment = true;

    // In the production environment we use NGINX for HTTPS.
    app.UseHttpsRedirection();
}

app.UseCors("corspolicy");


app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Images")),
    RequestPath = new PathString("/Images")
});
app.UseRouting();

// This sequence of using these two is important. First authentication, then authorization.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Startup.Run();

app.Run();
