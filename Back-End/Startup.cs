using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Back_End.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Rest;

namespace Back_End
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure Database Context
            services.AddDbContext<OneMoreTreeContext>(options => {
                options.UseSqlServer(Configuration.GetConnectionString("DATABASE_CONNECTION_STRING"));
            });

            // Configure Azure Blob Storage 
            BlobServiceClient blobServiceClient =
                new BlobServiceClient(Configuration.GetConnectionString("STORAGE_CONNECTION_STRING"));
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("trees");

            if (!containerClient.Exists())
            {
                containerClient.Create();
            }

            services.AddSingleton<BlobServiceClient>(blobServiceClient);

            // Configure Azure Content Moderator
            string contentModeratorKey = Configuration["CONTENT_MODERATOR_KEY"];
            string contentModeratorEndpoint = Configuration["CONTENT_MODERATOR_ENDPOINT"];
            
            ServiceClientCredentials contentModeratorCredentials = 
                new Microsoft.Azure.CognitiveServices.ContentModerator.ApiKeyServiceClientCredentials(contentModeratorKey);

            ContentModeratorClient contentModeratorClient =
               new ContentModeratorClient(contentModeratorCredentials);
            
            contentModeratorClient.Endpoint = contentModeratorEndpoint;

            services.AddSingleton<ContentModeratorClient>(contentModeratorClient);

            // Configure Computer Vision for Image Recognition
            string computerVisionKey = Configuration["COMPUTER_VISION_KEY"];
            string computerVisionEndpoint = Configuration["COMPUTER_VISION_ENDPOINT"];

            ServiceClientCredentials computerVisionCredentials =
                new Microsoft.Azure.CognitiveServices.Vision.ComputerVision.ApiKeyServiceClientCredentials(computerVisionKey);

            ComputerVisionClient computerVisionClient =
               new ComputerVisionClient(computerVisionCredentials);

            computerVisionClient.Endpoint = computerVisionEndpoint;

            services.AddSingleton<ComputerVisionClient>(computerVisionClient);

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<OneMoreTreeContext>();
                context.Database.Migrate();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
