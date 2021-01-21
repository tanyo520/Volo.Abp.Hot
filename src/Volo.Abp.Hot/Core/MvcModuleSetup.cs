
using Volo.Abp.Hot.Compiler;
using Volo.Abp.Hot.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Volo.Abp.Modularity;

namespace Volo.Abp.Hot.Core.Mvc
{
    public class MvcModuleSetup : IMvcModuleSetup
    {
        private readonly ApplicationPartManager _partManager;
        private readonly IReferenceLoader _referenceLoader = null;
        private readonly IHttpContextAccessor _context;
        private readonly ServiceConfigurationContext _serviceContext;

        public MvcModuleSetup(ApplicationPartManager partManager, ServiceConfigurationContext serviceContext,IReferenceLoader referenceLoader, IHttpContextAccessor httpContextAccessor)
        {
            _partManager = partManager;
            _referenceLoader = referenceLoader;
            _context = httpContextAccessor;
            _serviceContext = serviceContext;
        }

        public void EnableModule(string moduleName)
        {
            try
            {
                var controller = _partManager.ApplicationParts.FirstOrDefault(p => p.Name == moduleName);
                if (controller == null)
                {
                    ServiceProvider provider = _serviceContext.Services.BuildServiceProvider();
                    var contextProvider = new CollectibleAssemblyLoadContextProvider();
                    using (IServiceScope scope = provider.CreateScope())
                    {
                        var context = contextProvider.Get(moduleName, _serviceContext.Services, _partManager, scope);
                        PluginsLoadContexts.Add(moduleName, context);
                        context.Enable();
                    }
                    ResetControllActions();
                }
            } catch (Exception e) {
                Console.WriteLine(e);
            }
          
           
            //if (!PluginsLoadContexts.Any(moduleName))
            //{
            //    ServiceProvider provider = CoolCatStartup.Services.BuildServiceProvider();
            //    var contextProvider = new CollectibleAssemblyLoadContextProvider();

            //    using (IServiceScope scope = provider.CreateScope())
            //    {
            //        var dataStore = scope.ServiceProvider.GetService<IDataStore>();
            //        var documentation = scope.ServiceProvider.GetService<IQueryDocumentation>();

            //        var context = contextProvider.Get(moduleName, _partManager, scope, dataStore, documentation);
            //        PluginsLoadContexts.Add(moduleName, context);
            //    }

            //    ResetControllActions();
            //}
        }

        public void DisableModule(string moduleName)
        {
            
            var controller = _partManager.ApplicationParts.FirstOrDefault(p => p.Name == moduleName);
            if (controller != null) {
                _partManager.ApplicationParts.Remove(controller);
                var ui = _partManager.ApplicationParts.FirstOrDefault(p => p.Name == $"{moduleName}.Views");
                if (ui != null)
                {
                    _partManager.ApplicationParts.Remove(ui);
                }
                var items = new List<ServiceDescriptor>();
                foreach (var item in _serviceContext.Services) {
                    if (item.ServiceType.Assembly.GetName().Name == moduleName) {
                        items.Add(item);
                    }
                }
                foreach (var service in items) {
                    _serviceContext.Services.Remove(service);
                }
                var context = PluginsLoadContexts.Get(moduleName);
                context.Disable();

                PluginsLoadContexts.Remove(moduleName);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                ResetControllActions();
            }
        }

        public void DeleteModule(string moduleName)
        {
            PluginsLoadContexts.Remove(moduleName);
            var directory = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules", moduleName));
            directory.Delete(true);
        }

        private void ResetControllActions()
        {
            var provider = _context.HttpContext.RequestServices.GetService(typeof(IViewCompilerProvider)) as InBizViewCompilerProvider;
            provider.Refresh();
            
            FieldInfo filed = _context.HttpContext.RequestServices.GetType().GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance);
            if (filed != null) {
                filed.SetValue(_context.HttpContext.RequestServices, false);
            }
            MethodInfo method = _context.HttpContext.RequestServices.GetType().GetMethod("Dispose", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (method != null)
            {
                method.Invoke(_context.HttpContext.RequestServices,null);
            }
            
            InBizActionDescriptorChangeProvider.Instance.HasChanged = true;
            InBizActionDescriptorChangeProvider.Instance.TokenSource?.Cancel();
        }
    }
}
