using CloudinaryDotNet;
using GMPS.API.Mapper;
using GPMS.APPLICATION.ContextRepo;
using GPMS.APPLICATION.Repositories;
using GPMS.APPLICATION.Services;
using GPMS.DOMAIN.Entities;
using GPMS.INFRASTRUCTURE.CloudinaryAPI;
using GPMS.INFRASTRUCTURE.DataContext;
using GPMS.INFRASTRUCTURE.EmailAPI;
using GPMS.INFRASTRUCTURE.Mappers;
using GPMS.INFRASTRUCTURE.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders().AddSimpleConsole().AddDebug();
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

       // Default Bearer for token
       options.AddSecurityRequirement(new OpenApiSecurityRequirement
         {
              {
                new OpenApiSecurityScheme
                {
                     Reference = new OpenApiReference
                     {
                          Type = ReferenceType.SecurityScheme,
                          Id = "Bearer"
                     }
                },
                Array.Empty<string>()
              }
         });
   });
// add cache
builder.Services.AddMemoryCache();

// builder.Services.AddAutoMapper(typeof(SqlServerToEntityProfile).Assembly);
builder.Services.AddAutoMapper(typeof(SqlServerToEntityProfile).Assembly, typeof(MapperProfile).Assembly);
builder.Services.AddDbContext<GPMS_SYSTEMContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("GPMSDB")));



builder.Services.AddScoped<IBaseAccountRepositories, SqlServerUserRepository>();
builder.Services.AddScoped<IBaseRepositories<User>, SqlServerUserRepository>();
builder.Services.AddScoped<IUserRepositories, UserService>();
builder.Services.AddScoped<IAccountRepositories, AccountService>();
builder.Services.AddScoped<IBaseUserRoleRepo, SqlServerUserRoleRepository>();
builder.Services.AddScoped<IEmailRepositories, EmailService>();
builder.Services.AddScoped<IWorkerRepositories, WorkerService>();
builder.Services.AddScoped<IBaseWorkerRepository, SqlServerWorkerRepository>();

builder.Services.AddScoped<IBaseRepositories<UserStatus>, SqlServerUserStatusRepository>();

builder.Services.AddScoped<IBaseRepositories<Role>, SqlServerRoleRepository>();
builder.Services.AddScoped<IBaseRepositories<WorkerRole>, SqlServerWorkerRoleRepository>();
builder.Services.AddScoped<IBaseWorkerRoleRepositories, SqlServerWorkerRoleRepository>();
builder.Services.AddScoped<IWorkerRoleRepositories, WorkerRoleService>();

builder.Services.AddScoped<IBaseOrderRepositories, SqlServerOrderRepository>();
builder.Services.AddScoped<IBaseRepositories<Order>, SqlServerOrderRepository>();
builder.Services.AddScoped<IOrderRepositories, OrderService>();
builder.Services.AddScoped<IBaseOrderStatusRepositories, SqlServerOrderRepository>();
builder.Services.AddScoped<IBaseRepositories<OrderRejectReason>, SqlServerOrderRejectRepository>();
builder.Services.AddScoped<IOrderRejectRepositories, OrderRejectService>();

builder.Services.AddScoped<IBaseRepositories<OMaterial>, SqlServerMaterialRepository>();

builder.Services.AddScoped<IBaseRepositories<Comment>, SqlServerCommentRepository>();
builder.Services.AddScoped<ICommentRepositories, CommentServices>();

builder.Services.AddScoped<IBaseRepositories<LeaveRequest>, SqlServerLeaveRequestRepository>();
builder.Services.AddScoped<ILeaveRequestRepositories, LeaveRequestService>();

builder.Services.AddScoped<IUnitOfWork, DbContextUnitOfWork>();

builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
//onworking
builder.Services.AddScoped<IPermissionRepositories, PermissionService>();

builder.Services.AddScoped<IBaseRepositories<Production>, SqlServerProductionRepository>();


builder.Services.AddScoped<IProductionRepositories, ProductionService>();



builder.Services.AddScoped<IBaseRepositories<ProductionPart>, SqlServerProductionPartRepository>();

builder.Services.AddScoped<IProductionPartRepositories, ProductionPartService>();

//----------------------Identity-----------------------------
//builder.Services.AddIdentity<User,Role>().AddEntityFrameworkStores<GPMS_SYSTEMContext>();



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

//app.UseHttpsRedirection();

app.UseCors("AnyOrigin");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
