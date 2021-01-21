using Volo.Abp.Hot.Compiler;
using Volo.Abp.Hot.Core;
using Volo.Abp.Hot.Modularity;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Volo.Abp.Modularity;
using Volo.Abp.Modularity.PlugIns;

namespace Volo.Abp.Hot.Infrastructure
{
    public class CollectibleAssemblyLoadContextProvider
    {

        public CollectibleAssemblyLoadContext Get(string moduleName, IServiceCollection services, ApplicationPartManager apm, IServiceScope scope,InBizModuleEnum inBizModuleEnum = InBizModuleEnum.Boot)
        {
            string folderName = inBizModuleEnum == InBizModuleEnum.Boot ? "boot" : "hot";
            CollectibleAssemblyLoadContext context = new CollectibleAssemblyLoadContext(moduleName);
            IReferenceLoader loader = scope.ServiceProvider.GetService<IReferenceLoader>();
            InBizModuleLoader inBizModuleLoader = scope.ServiceProvider.GetService<InBizModuleLoader>();
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "modules", folderName, moduleName, $"{moduleName}.dll");
            string viewFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "modules", folderName, moduleName, $"{moduleName}.Views.dll");
            string referenceFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "modules", folderName, moduleName);
            if (File.Exists(filePath)) {
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    Assembly assembly = context.LoadFromStream(fs);
                    context.SetEntryPoint(assembly);
                    loader.LoadStreamsIntoContext(context, referenceFolderPath, assembly);
                    AssemblyPart controllerAssemblyPart = new AssemblyPart(assembly);
                    apm.ApplicationParts.Add(controllerAssemblyPart);
                    //services.AddAssembly(assembly);
                    foreach (var type in assembly.GetTypes())
                    {
                        if (AbpModule.IsAbpModule(type))
                        {
                          inBizModuleLoader.LoadModules(services, type, new PlugInSourceList());
                          inBizModuleLoader.ConfigureServices(services);
                          inBizModuleLoader.InitializeModules(new Volo.Abp.ApplicationInitializationContext(scope.ServiceProvider));
                        }
                    }
                    var resources = assembly.GetManifestResourceNames();
                    if (resources.Any())
                    {
                        foreach (var item in resources)
                        {
                            var stream = new MemoryStream();
                            var source = assembly.GetManifestResourceStream(item);
                            source.CopyTo(stream);
                            context.RegisterResource(item, stream.ToArray());
                        }
                    }
                }
            }
            else{
                return null;
            }
            if (File.Exists(viewFilePath)) {
                using (FileStream fsView = new FileStream(viewFilePath, FileMode.Open))
                {
                    Assembly vwAssembly = context.LoadFromStream(fsView);
                    loader.LoadStreamsIntoContext(context, referenceFolderPath, vwAssembly);

                    InBizRazorAssemblyPart moduleView = new InBizRazorAssemblyPart(vwAssembly, moduleName);
                    apm.ApplicationParts.Add(moduleView);
                }
            }
            context.Enable();
            return context;
        }
    }
}
