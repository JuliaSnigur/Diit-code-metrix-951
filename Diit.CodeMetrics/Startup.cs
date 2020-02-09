﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Diit.CodeMetrics.Data;
using Diit.CodeMetrics.Data.Source;
using Diit.CodeMetrics.Services;
using Diit.CodeMetrics.Services.Analyzer;
using Diit.CodeMetrics.Services.Source;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace Diit.CodeMetrics
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddSingleton<ISourceLoader<FileSourceViewModel>, FileSourceLoader>();
            services.AddSingleton<ISourceLoader<StringSourceViewModel>, StringSourceLoader>();
            services.AddSingleton<IFileProjectWorker, FileProjectWorker>();
            services.AddSingleton<IPatternFactory, PatternFactory>();

            services.AddSingleton<IMetricsCreator<IHalstedMetrics>, HalstedMetricsCreator>();
            services.AddSingleton<IMetricsCreator<IMcCeibMetrics>, McCeibMetricsCreator>();
            services.AddSingleton<IMetricsCreator<ICommentMetrics>, CommentMetricsCreator>();
            services.AddSingleton<IMetricsCreator<IChepinaMetrics>, ChepinaMetricsCreator>();
            services.AddSingleton<IMetricsCreator<IGilbMetrics>, GilbMetricsCreator>();
            services.AddSingleton<IMetricsCreator<IMetrics>, MetricsCreator>();

            services.AddSingleton<ILexicalAnalyzer<IMcCeibMetrics>, SharpSimpleLexicalAnalyzer2>();
            services.AddSingleton<ILexicalAnalyzer<IHalstedMetrics>, SHarpLexicalAnalyzer>();
            services.AddSingleton<ILexicalAnalyzer<ICommentMetrics>, SharpCommentAnalyzer>();
            services.AddSingleton<ILexicalAnalyzer<IChepinaMetrics>, SharpChepiraAnalyzer>();
            services.AddSingleton<ILexicalAnalyzer<IGilbMetrics>, GilbSharpAnalyzer>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "Code-metrics API",
                    Version = "v1",
                });
            
                //Locate the XML file being generated by ASP.NET...
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            
                //... and tell Swagger to use those XML comments.
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}");
            });
            
            app.UseSwagger();
        
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Code-metrics API v1");
            });
        }
    }
}
