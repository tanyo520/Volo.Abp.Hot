using Volo.Abp.Hot.Core;
using Volo.Abp.Hot.Core.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Volo.Abp;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Hot.Infrastructure;
using System.Runtime.Loader;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Volo.Abp.Hot.Compiler;
using System.Linq;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Hot.Modularity;
using Volo.Abp.Swashbuckle;
using Volo.Abp.Hot.Application;

namespace Volo.Abp.Hot
{
    [DependsOn(typeof(AbpAspNetCoreMvcModule), typeof(AbpSwashbuckleModule))]
    public class InBizHotModule : InBizModule
    {

        /// <summary>
        /// 注册方法，注册的顺序决定了界面中排列的顺序
        /// </summary>
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.TryAddSingleton<IActionDescriptorChangeProvider>(InBizActionDescriptorChangeProvider.Instance);
            context.Services.TryAddSingleton(InBizActionDescriptorChangeProvider.Instance);
        }
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.TryAddSingleton<InBizModuleLoader>();
            context.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            context.Services.TryAddSingleton<IMvcModuleSetup, MvcModuleSetup>();
            context.Services.TryAddSingleton<IReferenceContainer, DefaultReferenceContainer>();
            context.Services.TryAddSingleton<IReferenceLoader, DefaultReferenceLoader>();
         
            ServiceProvider provider = context.Services.BuildServiceProvider();
            var partManager = context.Services.GetSingletonInstance<ApplicationPartManager>();
            using (IServiceScope scope = provider.CreateScope())
            {
                var contextProvider = new CollectibleAssemblyLoadContextProvider();
                var pluginContext = contextProvider.Get("DemoPlugin2", context.Services, partManager, scope);
                PluginsLoadContexts.Add("DemoPlugin2", pluginContext);
            }
            AssemblyLoadContextResoving();
            Configure<AbpAspNetCoreMvcOptions>(options =>
            {
                options.ConventionalControllers.Create(typeof(InBizModuleAppliction).Assembly);
            });
            Configure<RazorViewEngineOptions>(o =>
            {
                o.AreaViewLocationFormats.Add("/Modules/{2}/Views/{1}/{0}" + RazorViewEngine.ViewExtension);
                o.AreaViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
            });
            context.Services.Replace<IViewCompilerProvider, InBizViewCompilerProvider>();
        }

        private static void AssemblyLoadContextResoving()
        {
            AssemblyLoadContext.Default.Resolving += (context, assembly) =>
            {
                Func<CollectibleAssemblyLoadContext, bool> filter = p => p.Assemblies.Any(p => p.GetName().Name == assembly.Name
                                                        && p.GetName().Version == assembly.Version);
                if (PluginsLoadContexts.All().Any(filter))
                {
                    Assembly ass = PluginsLoadContexts.All().First(filter)
                        .Assemblies.First(p => p.GetName().Name == assembly.Name
                        && p.GetName().Version == assembly.Version);
                    return ass;
                }
                return null;
            };
        }
        public override void PostConfigureServices(ServiceConfigurationContext context)
        {
        }
        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            //var app = context.GetApplicationBuilder();
            //app.InBizModuleRoute();
        }
    }
}
