using System;
using Ecommerce.APIUI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Ecommerce.APIUI.Extensions
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddCustomAuthConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JWTSettingsModel>(configuration.GetSection("JWT"));
            var jwtSettings = configuration.GetSection("JWT").Get<JWTSettingsModel>();

            services.AddAuthorization()
                .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                .AddJwtBearer(
                    options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters()
                        {
                            ValidIssuers = jwtSettings.ValidIssuers,
                            ValidAudience = jwtSettings.Issuer,
                            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                            ClockSkew = TimeSpan.Zero ,// You can change it later
                        };
                    }
                );
            return services;
        }


        // ApplicationBuilder
        public static IApplicationBuilder UseCustomAuthConfigurations(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            return app;
        }

    }
}