using GPMS.INFRASTRUCTURE.DataContext;
using GPMS.INFRASTRUCTURE.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

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
       options.AddSecurityDefinition("Bearer" , new OpenApiSecurityScheme
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
builder.Services.AddScoped<GPMS.APPLICATION.Abstractions.IUserInterface, GPMS.INFRASTRUCTURE.Repositories.SqlServerUserRepository>();
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






    });

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
