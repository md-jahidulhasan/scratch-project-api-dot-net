using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ScratchProject.Api.DatabaseContext;
using ScratchProject.Api.Intefaces;
using ScratchProject.Api.Services;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ScratchProject.Api
{
    public static class WebApplicationServiceConfigurationExtension
    {
        public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
        {
            builder.Host.ConfigureLogging(loggingProvider =>
            {
                loggingProvider.ClearProviders();
                loggingProvider.AddConsole();
            });

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add(new ProducesAttribute("application/json"));
                options.Filters.Add(new ConsumesAttribute("application/json"));

                // to automatically add authorize attribute. we can assign allowanonymous in controller to avoid it.
                var authorizationPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                options.Filters.Add(new AuthorizeFilter(authorizationPolicy));
            }).AddXmlSerializerFormatters();

            builder.Services.AddApiVersioning(config =>
            {
                config.ApiVersionReader = new UrlSegmentApiVersionReader(); // read version from request url
                                                                            // config.ApiVersionReader = new HeaderApiVersionReader(); // read version from request header as "api-version". Eg, api-version: 1.0
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.AssumeDefaultVersionWhenUnspecified = true;
            });
            builder.Services.AddDbContext<ApplicationDatabaseContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnectionString"));
            });

            // Swagger
            builder.Services.AddEndpointsApiExplorer();  // Generates description for all endpoints
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Scratch Api", Version = "1.0" });
                options.SwaggerDoc("v2", new OpenApiInfo { Title = "Scratch Api", Version = "2.0" });
                // options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "api.xml"));
            }); // generates OpenAPI speciication

            builder.Services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV"; // v1
                options.SubstituteApiVersionInUrl = true;
            });
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policyBuilder =>
                {
                    policyBuilder
                    .WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>())
                    .WithHeaders("Authorization", "origin", "accpet", "content-type");
                    // policyBuilder.WithOrigins("*");
                });
            });

            builder.Services.AddTransient<IJwtService, JwtService>();
            builder.Services.AddSingleton<IMongoDbClientService, MongoDbClientService>();
            builder.Services.AddSingleton<IRedisService, RedisService>();
            // JWT

            builder.Services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateAudience = true,
                        ValidAudience = builder.Configuration["Jwt:Audiance"],
                        ValidateIssuer = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                    };
                });

            builder.Services.AddAuthorization(options => { });
            builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
            {
                var configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Workstation/Configurations", "Debug.json");
                config.AddJsonFile(configFilePath, optional: false, reloadOnChange: false);
            });

            return builder;
        }
    }
}
