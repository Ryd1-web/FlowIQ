using System.Text;
using FlowIQ.Application.Interfaces;
using FlowIQ.Domain.Interfaces;
using FlowIQ.Infrastructure.Data;
using FlowIQ.Infrastructure.Repositories;
using FlowIQ.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace FlowIQ.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<FlowIQDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBusinessRepository, BusinessRepository>();
        services.AddScoped<IIncomeRepository, IncomeRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();

        // Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddHttpClient<IAIServiceClient, AIServiceClient>();

        // JWT Authentication
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secret = jwtSettings["Secret"]!;
        var key = Encoding.UTF8.GetBytes(secret);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.FromMinutes(1),
                ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Extract token from Authorization header, stripping any extra "bearer" prefix
                    var auth = context.Request.Headers.Authorization.ToString();
                    if (!string.IsNullOrEmpty(auth))
                    {
                        // Remove all occurrences of "bearer " (case-insensitive) to get the raw token
                        var token = auth;
                        while (token.StartsWith("bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            token = token.Substring(7).TrimStart();
                        }
                        // Also handle case where "Bearer" is there without a space
                        while (token.StartsWith("bearer", StringComparison.OrdinalIgnoreCase) && token.Length > 6 && token[6] != '.')
                        {
                            token = token.Substring(6).TrimStart();
                        }
                        context.Token = token;
                    }
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                        .CreateLogger("JwtBearer");
                    logger.LogError(context.Exception, "JWT authentication failed. Token: {Token}",
                        context.Request.Headers.Authorization.ToString());
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();

        return services;
    }
}
