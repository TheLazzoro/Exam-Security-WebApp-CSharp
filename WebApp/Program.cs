using WebApp.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using WebApp.ErrorHandling;
using WebApp;
using Microsoft.Extensions.FileProviders;
using WebApp.Utility;
using System.Security.AccessControl;
using System.IO;
using System.Diagnostics;

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
    x.TokenValidationParameters = Token.GetValidationParameters();
});

builder.Services.AddAuthorization();

string corsPolicy = "corsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:5173").AllowAnyMethod().AllowAnyHeader(); // Enables a single domain
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
// All requests are routed through the middleware before they get to the endpoint method.
app.UseMiddleware<Middleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    Globals.IsDevelopment = true;

    // In the production environment we use NGINX for HTTPS.
    app.UseHttpsRedirection();
}

app.UseCors(corsPolicy);

// Create directory for images, and set permissions.
string pathImages = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Images");
if(!Directory.Exists(pathImages)) {
    Directory.CreateDirectory(pathImages);
#if _WINDOWS
    string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name; // username of the current logged in user.
    var fileSecurity = new FileSecurity();
    var readRule = new FileSystemAccessRule(userName, FileSystemRights.Read, AccessControlType.Allow);
    var writeRule = new FileSystemAccessRule(userName, FileSystemRights.Write, AccessControlType.Allow);
    var execRule = new FileSystemAccessRule(userName, FileSystemRights.ExecuteFile, AccessControlType.Deny);
    fileSecurity.AddAccessRule(readRule);
    fileSecurity.AddAccessRule(writeRule);
    fileSecurity.AddAccessRule(execRule);
    //TODO: Still needs to apply for the folder. Unfinished.


#else
    using var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "/bin/bash",
            Arguments = $"-c chmod 644 \"{pathImages}\""
        }
    };

    process.Start();
    process.WaitForExit();
#endif
}
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(pathImages),
    RequestPath = new PathString("/Images")
});
app.UseRouting();

// This sequence of using these two is important. First authentication, then authorization.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Startup.Run();

app.Run();
