using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Ecommerce.APIUI.Models;
using Ecommerce.APIUI.Extensions;
using Ecommerce.Shared.Repositories.Interfaces;
using Ecommerce.Shared.Repositories;
using Microsoft.AspNetCore.Identity;
using Stripe;
using Ecommerce.Shared.Services.ServiceInterfaces;
using Ecommerce.Shared.Services;

namespace Ecommerce.APIUI
{
    public class Startup
    {
        readonly string MyAllowedSpecificOrigins = "_MyAllowedSpecificOrigins";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("MSSQL"));
            });
            StripeConfiguration.ApiKey = Configuration["Stripe:Secret"];
            services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowedSpecificOrigins,
                    builder =>
                    {
                        builder.WithOrigins(
                            "http://localhost:4200",
                            "http://localhost:8080")
                        // builder.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    });
            });
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();


            services.AddCustomAuthConfigurations(Configuration);
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    // This prevents some crazy unending loops ;
                    // https://stackoverflow.com/questions/59199593/net-core-3-0-possible-object-cycle-was-detected-which-is-not-supported
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ecommerce.APIUI", Version = "v1" });
            });

            services.AddAutoMapper(typeof(EcommerceMappingProfile));
            
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IShippingInfoRepository, ShippingInfoRepository>();

            services.AddScoped<IOrderHelperService , OrderHelperService>();
            services.AddScoped<IPaymentService,StripePaymentService>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ecommerce.APIUI v1"));
            }


            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors(MyAllowedSpecificOrigins);

            // custom Auth with jwt
            app.UseCustomAuthConfigurations(env);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
