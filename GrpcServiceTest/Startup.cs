using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GrpcServiceTest
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddGrpc();

            services.AddCors(o => o.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
            }));

            /*
            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(60);
                options.ExcludedHosts.Add("www.skp-instructor.sbs");
                options.ExcludedHosts.Add("skp-instructor.sbs");
            });
            */
            services.AddHttpsRedirection(options =>
            {
                //options.RedirectStatusCode = (int)HttpStatusCode.PermanentRedirect;
                options.HttpsPort = 5001;
            });

            /*
            services.AddCors(o =>
            {
                o.AddPolicy("MyPolicy", builder =>
                {
                    //builder.WithOrigins("http://localhost:4200");
                    builder.WithMethods("POST, GET, OPTIONS");
                    builder.AllowAnyHeader();
                    builder.AllowAnyOrigin();
                    //builder.AllowCredentials();
                    builder.WithExposedHeaders("Grpc-Status", "Grpc-Message");
                });
            
            });
            
            */



        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
           else
            {
                app.UseExceptionHandler("/Error");
                //app.UseHsts();
            }

            app.UseHttpsRedirection();

            /*

            app.UseRouting();
            app.UseGrpcWeb(); // Must be added between UseRouting and UseEndpoints
            app.UseCors("MyPolicy");
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GreeterService>().RequireCors("MyPolicy");
                endpoints.MapGrpcService<GreeterService>().EnableGrpcWeb();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
            */

            app.UseRouting();

            app.UseGrpcWeb(); // Must be added between UseRouting and UseEndpoints

            app.UseCors("AllowAll");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GreeterService>().EnableGrpcWeb();
                endpoints.MapGrpcService<TransferService>().EnableGrpcWeb();
            });
        }
    }
}
