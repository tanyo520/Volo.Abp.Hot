using Volo.Abp.Hot.Compiler;
using Volo.Abp.Hot.Core;
using Volo.Abp.Hot.Core.Mvc;
using Volo.Abp.Hot.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace Volo.Abp.Hot
{
    public static class InBizHotStartup
    {
        private static IServiceCollection _serviceCollection;

        public static IServiceCollection Services => _serviceCollection;

        public static void HotSetup(this IServiceCollection services, IMvcBuilder mvcBuilder)
        {
            _serviceCollection = services;
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IMvcModuleSetup, MvcModuleSetup>();
            services.AddSingleton<IServiceCollection>(services);
            // services.AddScoped<IDbHelper, DbHelper>();
            services.AddSingleton<IActionDescriptorChangeProvider>(InBizActionDescriptorChangeProvider.Instance);
            services.AddSingleton<IReferenceContainer, DefaultReferenceContainer>();
            services.AddSingleton<IReferenceLoader, DefaultReferenceLoader>();
            services.AddSingleton(InBizActionDescriptorChangeProvider.Instance);
            // IMvcBuilder mvcBuilder = services.AddMvc();
            ServiceProvider provider = services.BuildServiceProvider();
            using (IServiceScope scope = provider.CreateScope())
            {
                var contextProvider = new CollectibleAssemblyLoadContextProvider();
                var context = contextProvider.Get("DemoPlugin1", services,mvcBuilder.PartManager, scope);
                if (context != null) {
                    PluginsLoadContexts.Add("DemoPlugin1", context);
                }
            }
            AssemblyLoadContextResoving();
            services.Configure<RazorViewEngineOptions>(o =>
            {
                o.AreaViewLocationFormats.Add("/Modules/{2}/Views/{1}/{0}" + RazorViewEngine.ViewExtension);
                o.AreaViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
            });

            services.Replace<IViewCompilerProvider, InBizViewCompilerProvider>();
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
    }
}
