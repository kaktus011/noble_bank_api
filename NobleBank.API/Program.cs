using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NobleBank.API.Middleware;
using NobleBank.Application;
using NobleBank.Infrastructure;
using NobleBank.Infrastructure.Settings;
using NobleBank.Infrastructure.Seed;
using System.Text;
using NobleBank.Domain.Common;

namespace NobleBank.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                // Добавя JWT поле в Swagger UI
                options.AddSecurityDefinition("Bearer", new()
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Enter your JWT token"
                });

                options.AddSecurityRequirement(new()
                {
                    {
                        new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
                        []
                    }
                });

                options.UseInlineDefinitionsForEnums();
            });

            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);

            // JWT Authentication
            JwtSettings jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;

            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                                                   Encoding.UTF8.GetBytes(jwtSettings.Secret))
                };
            });

            builder.Services.AddCors(options =>
                options.AddPolicy("ReactApp", policy =>
                    policy.WithOrigins("http://localhost:5173")
                          .AllowAnyHeader()
                          .AllowAnyMethod()));

            WebApplication app = builder.Build();

            bool runDatabaseSeeding = app.Environment.IsDevelopment() ||
                                      builder.Configuration.GetValue<bool>("RunDatabaseSeeding");

            if (runDatabaseSeeding)
            {
                using (IServiceScope scope = app.Services.CreateScope())
                {
                    IServiceProvider services = scope.ServiceProvider;

                    try
                    {
                        await DatabaseSeeder.SeedAsync(services);
                    }
                    catch (Exception ex)
                    {
                        ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();
                        logger.LogCritical(ex, Constants.Exceptions.CannotSeedDatabase);
                        throw;
                    }
                }
            }
            else
            {
                app.Logger.LogInformation("Database seeding skipped. Enable 'RunDatabaseSeeding' to run seeding outside Development.");
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseHttpsRedirection();
            app.UseCors("ReactApp");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
