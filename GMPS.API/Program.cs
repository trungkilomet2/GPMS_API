using GPMS.INFRASTRUCTURE.DataContext;
using GPMS.INFRASTRUCTURE.Mappers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using GPMS.APPLICATION.Repositories;
using GPMS.APPLICATION.Services;
using GPMS.INFRASTRUCTURE.Repositories;
using GPMS.DOMAIN.Entities;
using GPMS.APPLICATION.ContextRepo;

var builder = WebApplication.CreateBuilder(args);

//--------------------------- Controller Config ---------------------------
builder.Services.AddControllers(
    options =>
    {
        // Setting No Cache
        options.CacheProfiles.Add("NoCache",
        new CacheProfile() { NoStore = true });
        // Setting Cache
        options.CacheProfiles.Add("Any-60",
            new CacheProfile() { Location = ResponseCacheLocation.Any, Duration = 60 });
    });

builder.Services.AddEndpointsApiExplorer();
//--------------------------- Bearer Token for Swagger ---------------------------
builder.Services.AddSwaggerGen(
   options =>
   {
       options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
       {
           In = ParameterLocation.Header,
           Description = "Please enter token",
           Name = "Authorization",
           Type = SecuritySchemeType.Http,
           BearerFormat = "JWT",
           Scheme = "bearer"
       });
   });
// Dependency Injection for Services and Repositories

builder.Services.AddAutoMapper(typeof(SqlServerToEntityProfile).Assembly);
builder.Services.AddDbContext<GPMS_SYSTEMContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("GPMSDB")));

builder.Services.AddScoped<IBaseRepositories<User>, SqlServerUserRepository>();
builder.Services.AddScoped<IBaseAccountRepositories, SqlServerUserRepository>();
builder.Services.AddScoped<IUserRepositories, UserService>();
builder.Services.AddScoped<IAccountRepositories, AccountService>();



//----------------------CORS-------------------------------

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(cfg =>
    {
        cfg.WithOrigins(builder.Configuration["AllowedOrigins"]);
        cfg.AllowAnyHeader();
        cfg.AllowAnyMethod();
    });
    options.AddPolicy(name: "AnyOrigin",
        cfg =>
        {
            cfg.AllowAnyOrigin();
            cfg.AllowAnyHeader();
            cfg.AllowAnyMethod();
        });
});

//-------------------------------------------------------
builder.Services.AddAuthentication(
    options =>
    {
        // Xac thuc tu request
        options.DefaultAuthenticateScheme =
        // Xac thuc khi user chua dang nhap
        options.DefaultChallengeScheme =
        // Author
        options.DefaultForbidScheme =
        // Sign in va sign out
        options.DefaultScheme =
        options.DefaultSignInScheme =
        options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"])
            )
        };
    }); ;

builder.Services.AddAuthorization();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
