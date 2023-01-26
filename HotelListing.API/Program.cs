using HotelListing.API.Configuration;
using HotelListing.API.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Repository;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

//DbContext and connection
var connectionString =
  builder.Configuration.GetConnectionString("HotelListingDbConnectionString");
builder.Services.AddDbContext<HotelListingDbContext>(
  options => { options.UseSqlServer(connectionString); }
);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//CORS
builder.Services.AddCors(
  options =>
  {
    options.AddPolicy(
      "AllowAll",
      b => { b.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod(); }
    );
  }
);

builder.Host.UseSerilog(
  (ctx, lc) => lc.WriteTo.Console().ReadFrom.Configuration(ctx.Configuration)
);

//DI
builder.Services.AddAutoMapper(typeof(MapperConfig));
builder.Services.AddScoped(
  typeof(IGenericRepository<>),
  typeof(GenericRepository<>)
);

builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();

//Build
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
