using System;
using System.Text;

using AzureMapsToolkit;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Rest;

using BackEnd.Models;

namespace BackEnd
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
            services.AddDbContext<DatabaseContext>(options => {
                options.UseLazyLoadingProxies().UseSqlServer(Configuration.GetConnectionString("DATABASE_CONNECTION_STRING"));
            });

            // Configure Azure Blob Storage 
            BlobServiceClient blobServiceClient =
                new BlobServiceClient(Configuration.GetConnectionString("STORAGE_CONNECTION_STRING"));
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("trees");

            if (!containerClient.Exists())
            {
                containerClient.Create();
                containerClient.SetAccessPolicy(PublicAccessType.Blob);
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

            // Configure Azure Maps client
            AzureMapsServices mapsClient = new AzureMapsServices(Configuration["MAPS_KEY"]);

            services.AddSingleton<AzureMapsServices>(mapsClient);

            // Add authentication
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["JWT_ISSUER"],
                        ValidAudience = Configuration["JWT_AUDIENCE"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT_SIGN_KEY"]))
                    };
                })
                .AddFacebook(options =>
                {
                    options.ClientId = Configuration["FACEBOOK_APP_ID"];
                    options.ClientSecret = Configuration["FACEBOOK_APP_SECRET"];
                    options.SaveTokens = true;
                });

            // Add controllers
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<DatabaseContext>();
                context.Database.Migrate();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
