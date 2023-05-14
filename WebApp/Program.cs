using Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using WebApp.ErrorHandling;

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

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// This sequence of using these two is important. First authentication, then authorization.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Startup.Run();

app.Run();
