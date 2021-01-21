using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Hot;
using Volo.Abp.Hot.Modularity;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.Settings;
namespace TestAbp1
{
    [DependsOn(typeof(InBizHotModule))]
    public class TestAbpModule : InBizModule
    {
        public override string Uid => "ASDADAD1";
        public override string Icon => "";
        public override string Name => "测试模块";
        public override string Description => "这是个测试模块";
        public override string Version => "1.0.0";
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            ConfigureSwaggerServices(context.Services);
            Configure<AbpAspNetCoreMvcOptions>(options =>
            {
                options.ConventionalControllers.Create(typeof(TestApplitionService).Assembly, (options) =>
                {
                    options.RootPath = "modeules1";
                });
            });
        }
        private void ConfigureSwaggerServices(IServiceCollection services)
        {
            services.AddSwaggerGen(
                options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo { Title = "test API", Version = "v1" });
                    options.DocInclusionPredicate((docName, description) => true);
                    options.CustomSchemaIds(type => type.FullName);
                }
            );
        }
        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {

            
            var app = context.GetApplicationBuilder();
            var env = context.GetEnvironment();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

           // app.UseAbpRequestLocalization();


            app.UseCorrelationId();
            app.UseVirtualFiles();
          //  app.UseRouting();
            app.UseAuthentication();

            //if (MultiTenancyConsts.IsEnabled)
            //{
            //    app.UseMultiTenancy();
            //}

            app.UseAuthorization();

            app.UseSwagger();
            app.UseAbpSwaggerUI(options =>
            {
               // options.RoutePrefix = "swagger";
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "test API");
            });

            app.UseAuditing();
           // app.UseConfiguredEndpoints();
        }
    }
}
