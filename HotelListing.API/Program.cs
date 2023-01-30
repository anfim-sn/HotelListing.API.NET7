using System.Text;
using System.Text.Json;
using HotelListing.API.Core.Configuration;
using HotelListing.API.Core.Contracts;
using HotelListing.API.Core.Middleware;
using HotelListing.API.Core.Repository;
using HotelListing.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
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
  .AddTokenProvider<DataProtectorTokenProvider<ApiUser>>("HotelListingApi")
  .AddEntityFrameworkStores<HotelListingDbContext>()
  .AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();

//Swagger Doc
builder.Services.AddSwaggerGen(
  options =>
  {
    options.SwaggerDoc(
      "v1",
      new OpenApiInfo { Title = "Hotel Listing API", Version = "v1" }
    );
    options.AddSecurityDefinition(
      JwtBearerDefaults.AuthenticationScheme,
      new OpenApiSecurityScheme
      {
        Description = @"JWT Authorization header using the Bearer scheme.
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme
      }
    );
    options.AddSecurityRequirement(
      new OpenApiSecurityRequirement
      {
        {
          new OpenApiSecurityScheme
          {
            Reference =
              new OpenApiReference
              {
                Type = ReferenceType.SecurityScheme,
                Id = JwtBearerDefaults.AuthenticationScheme
              },
            Scheme = "0auth2",
            Name = JwtBearerDefaults.AuthenticationScheme,
            In = ParameterLocation.Header
          },
          new List<string>()
        }
      }
    );
  }
);

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

builder.Services.AddApiVersioning(
  options =>
  {
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
      new QueryStringApiVersionReader("api-version"),
      new HeaderApiVersionReader("X-Version"),
      new MediaTypeApiVersionReader("ver")
    );
  }
);

builder.Services.AddVersionedApiExplorer(
  options =>
  {
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
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

builder.Services.AddResponseCaching(
  options =>
  {
    options.MaximumBodySize = 1024;
    options.UseCaseSensitivePaths = true;
  }
);

builder.Services.AddHealthChecks()
  .AddCheck<CustomHealthCheck>(
    "Custom Health Check",
    HealthStatus.Degraded,
    new[] { "custom" }
  )
  .AddSqlServer(connectionString, tags: new[] { "database" })
  .AddDbContextCheck<HotelListingDbContext>(tags: new[] { "database" });

builder.Services.AddControllers()
  .AddOData(options => { options.Select().Filter().OrderBy(); });

//Build
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHealthChecks("/health");

app.UseHealthChecks(
  "/healthcustom",
  new HealthCheckOptions
  {
    Predicate = healthCheck => healthCheck.Tags.Contains("custom"),
    ResultStatusCodes =
    {
      [HealthStatus.Healthy] = StatusCodes.Status200OK,
      [HealthStatus.Degraded] = StatusCodes.Status200OK,
      [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    },
    ResponseWriter = WriteResponse
  }
);

app.UseHealthChecks(
  "/dbhealthcheck",
  new HealthCheckOptions
  {
    Predicate = healthCheck => healthCheck.Tags.Contains("database"),
    ResultStatusCodes =
    {
      [HealthStatus.Healthy] = StatusCodes.Status200OK,
      [HealthStatus.Degraded] = StatusCodes.Status200OK,
      [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    },
    ResponseWriter = WriteResponse
  }
);

app.UseHealthChecks(
  "/healthall",
  new HealthCheckOptions
  {
    ResultStatusCodes =
    {
      [HealthStatus.Healthy] = StatusCodes.Status200OK,
      [HealthStatus.Degraded] = StatusCodes.Status200OK,
      [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    },
    ResponseWriter = WriteResponse
  }
);

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseResponseCaching();
app.Use(
  async (context, next) =>
  {
    context.Response.GetTypedHeaders().CacheControl =
      new CacheControlHeaderValue
      {
        Public = true, MaxAge = TimeSpan.FromSeconds(10)
      };
    context.Response.Headers[HeaderNames.Vary] =
      new[] { "Accept-Encoding" };

    await next();
  }
);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

/*UTILS*/

static Task WriteResponse(HttpContext context, HealthReport healthReport)
{
  context.Response.ContentType = "application/json; charset=utf-8";

  var options = new JsonWriterOptions { Indented = true };

  using var memoryStream = new MemoryStream();
  using (var jsonWriter = new Utf8JsonWriter(memoryStream, options))
  {
    jsonWriter.WriteStartObject();
    jsonWriter.WriteString("status", healthReport.Status.ToString());
    jsonWriter.WriteStartObject("results");

    foreach (var healthReportEntry in healthReport.Entries)
    {
      jsonWriter.WriteStartObject(healthReportEntry.Key);
      jsonWriter.WriteString(
        "status",
        healthReportEntry.Value.Status.ToString()
      );
      jsonWriter.WriteString(
        "description",
        healthReportEntry.Value.Description
      );
      jsonWriter.WriteStartObject("data");

      foreach (var item in healthReportEntry.Value.Data)
      {
        jsonWriter.WritePropertyName(item.Key);

        JsonSerializer.Serialize(
          jsonWriter,
          item.Value,
          item.Value?.GetType() ?? typeof(object)
        );
      }

      jsonWriter.WriteEndObject();
      jsonWriter.WriteEndObject();
    }

    jsonWriter.WriteEndObject();
    jsonWriter.WriteEndObject();
  }

  return context.Response.WriteAsync(
    Encoding.UTF8.GetString(memoryStream.ToArray())
  );
}

internal class CustomHealthCheck : IHealthCheck
{
  public Task<HealthCheckResult> CheckHealthAsync(
    HealthCheckContext context,
    CancellationToken cancellationToken = default
  )
  {
    var isHealthy = true;

    /*Some checking*/

    if (isHealthy)
      return Task.FromResult(
        HealthCheckResult.Healthy("All systems are looking good")
      );

    return Task.FromResult(
      new HealthCheckResult(
        context.Registration.FailureStatus,
        "System Unhealthy"
      )
    );
  }
}
