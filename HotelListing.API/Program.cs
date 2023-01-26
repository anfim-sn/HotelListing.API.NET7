using System.Text;
using HotelListing.API.Configuration;
using HotelListing.API.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

//DbContext and connection
var connectionString =
  builder.Configuration.GetConnectionString("HotelListingDbConnectionString");
builder.Services.AddDbContext<HotelListingDbContext>(
  options => { options.UseSqlServer(connectionString); }
);

builder.Services.AddIdentityCore<ApiUser>()
  .AddRoles<IdentityRole>()
  .AddTokenProvider<DataProtectorTokenProvider<ApiUser>>("HotelListApi")
  .AddEntityFrameworkStores<HotelListingDbContext>()
  .AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddControllers();

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
builder.Services.AddScoped<IHotelsRepository, HotelsRepository>();
builder.Services.AddScoped<IAuthManager, AuthManager>();

builder.Services.AddAuthentication(
    options =>
    {
      options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }
  )
  .AddJwtBearer(
    options =>
    {
      options.TokenValidationParameters = new TokenValidationParameters
      {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
          Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"])
        )
      };
    }
  );

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
