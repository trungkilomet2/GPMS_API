using GPMS.INFRASTRUCTURE.DataContext;
using GPMS.INFRASTRUCTURE.Mappers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Dependency Injection for Services and Repositories

 builder.Services.AddAutoMapper(typeof(SqlServerToEntityProfile).Assembly);
builder.Services.AddDbContext<GPMS_SYSTEMContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("GPMSDB")));
builder.Services.AddScoped<GPMS.APPLICATION.Abstractions.IUserInterface, GPMS.INFRASTRUCTURE.Repositories.SqlServerUserRepository>();

//------------------------------------------------------
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
