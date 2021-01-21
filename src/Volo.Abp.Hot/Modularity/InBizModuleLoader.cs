using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Modularity;
using Volo.Abp.Modularity.PlugIns;

namespace Volo.Abp.Hot.Modularity
{
    public class InBizModuleLoader
    {
        private readonly IEnumerable<IModuleLifecycleContributor> _lifecycleContributors;
        private readonly ServiceConfigurationContext _serviceConfigurationContext;
        private IAbpModuleDescriptor[] Modules;
        public InBizModuleLoader(IOptions<AbpModuleLifecycleOptions> options,
            IServiceProvider serviceProvider,
            ServiceConfigurationContext serviceConfigurationContext) {
            _serviceConfigurationContext = serviceConfigurationContext;
            _lifecycleContributors = options.Value
                .Contributors
                .Select(serviceProvider.GetRequiredService)
                .Cast<IModuleLifecycleContributor>()
                .ToArray();
        }
        public IAbpModuleDescriptor[] LoadModules(
            IServiceCollection services,
            Type startupModuleType,
            PlugInSourceList plugInSources)
        {
            Check.NotNull(services, nameof(services));
            Check.NotNull(startupModuleType, nameof(startupModuleType));
            Check.NotNull(plugInSources, nameof(plugInSources));

            var modules = GetDescriptors(services, startupModuleType, plugInSources);

            modules = SortByDependency(modules, startupModuleType);
            Modules = modules.ToArray();
            return Modules;
        }

        private List<IAbpModuleDescriptor> GetDescriptors(
            IServiceCollection services, 
            Type startupModuleType,
            PlugInSourceList plugInSources)
        {
            var modules = new List<AbpModuleDescriptor>();

            FillModules(modules, services, startupModuleType, plugInSources);
            SetDependencies(modules);

            return modules.Cast<IAbpModuleDescriptor>().ToList();
        }
        private void SetVal(AbpModule abpModule, ServiceConfigurationContext context) {
            var serviceCnt = abpModule.GetType().GetField("_serviceConfigurationContext",
                 BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.ExactBinding);
            serviceCnt.SetValue(abpModule, context);
        }
        private void SetContext(InBizModule inBizModule, ServiceConfigurationContext context)
        {
            Type abpModuleType = null;
            var type = inBizModule.GetType();
            if (type.BaseType.Name == "AbpModule") {
                abpModuleType = type.BaseType;
            } else { 
               if(type.BaseType.BaseType.Name == "AbpModule")
                {
                    abpModuleType = type.BaseType.BaseType;
                }
            }
            var serviceCnt = abpModuleType.GetField("_serviceConfigurationContext",
                 BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.ExactBinding);
            if (serviceCnt != null) {
                serviceCnt.SetValue(inBizModule, context);
            }
        }
        public void ConfigureServices(IServiceCollection services)
        {
            foreach (var module in Modules)
            {
                if (module.Instance is InBizModule inBizModule)
                {
                    SetContext(inBizModule,_serviceConfigurationContext);
                }
            }

            //PreConfigureServices
            foreach (var module in Modules.Where(m => m.Instance is IPreConfigureServices))
            {
                try
                {
                    ((IPreConfigureServices)module.Instance).PreConfigureServices(_serviceConfigurationContext);
                }
                catch (Exception ex)
                {
                    throw new AbpInitializationException($"An error occurred during {nameof(IPreConfigureServices.PreConfigureServices)} phase of the module {module.Type.AssemblyQualifiedName}. See the inner exception for details.", ex);
                }
            }

            //ConfigureServices
            foreach (var module in Modules)
            {
                if (module.Instance is InBizModule inBiZModule)
                {
                    if (!inBiZModule.GetSkipAutoServiceRegistration())
                    {
                        services.AddAssembly(module.Type.Assembly);
                    }
                }
                try
                {
                    module.Instance.ConfigureServices(_serviceConfigurationContext);
                }
                catch (Exception ex)
                {
                    throw new AbpInitializationException($"An error occurred during {nameof(IAbpModule.ConfigureServices)} phase of the module {module.Type.AssemblyQualifiedName}. See the inner exception for details.", ex);
                }
            }

            //PostConfigureServices
            foreach (var module in Modules.Where(m => m.Instance is IPostConfigureServices))
            {
                try
                {
                    ((IPostConfigureServices)module.Instance).PostConfigureServices(_serviceConfigurationContext);
                }
                catch (Exception ex)
                {
                    throw new AbpInitializationException($"An error occurred during {nameof(IPostConfigureServices.PostConfigureServices)} phase of the module {module.Type.AssemblyQualifiedName}. See the inner exception for details.", ex);
                }
            }

            foreach (var module in Modules)
            {
                if (module.Instance is InBizModule inBizModule)
                {
                    SetContext(inBizModule, null);
                }
            }
        }
        public void InitializeModules(ApplicationInitializationContext context)
        {

            foreach (var contributor in _lifecycleContributors)
            {
                foreach (var module in Modules)
                {
                    try
                    {
                        contributor.Initialize(context, module.Instance);
                    }
                    catch (Exception ex)
                    {
                        throw new AbpInitializationException($"An error occurred during the initialize {contributor.GetType().FullName} phase of the module {module.Type.AssemblyQualifiedName}: {ex.Message}. See the inner exception for details.", ex);
                    }
                }
            }

        }

        protected virtual void FillModules(
            List<AbpModuleDescriptor> modules,
            IServiceCollection services,
            Type startupModuleType,
            PlugInSourceList plugInSources)
        {
            //All modules starting from the startup module
            foreach (var moduleType in AbpModuleHelper.FindAllModuleTypes(startupModuleType))
            {
                modules.Add(CreateModuleDescriptor(services, moduleType));
            }
            var allModules = plugInSources.SelectMany(pluginSource => pluginSource.GetModulesWithAllDependencies())
            .Distinct()
            .ToArray();
            //Plugin modules
            foreach (var moduleType in allModules)
            {
                if (modules.Any(m => m.Type == moduleType))
                {
                    continue;
                }

                modules.Add(CreateModuleDescriptor(services, moduleType, isLoadedAsPlugIn: true));
            }
        }

        protected virtual void SetDependencies(List<AbpModuleDescriptor> modules)
        {
            foreach (var module in modules)
            {
                SetDependencies(modules, module);
            }
        }

        protected virtual List<IAbpModuleDescriptor> SortByDependency(List<IAbpModuleDescriptor> modules, Type startupModuleType)
        {
            var sortedModules = modules.SortByDependencies(m => m.Dependencies);
            sortedModules.MoveItem(m => m.Type == startupModuleType, modules.Count - 1);
            return sortedModules;
        }

        protected virtual AbpModuleDescriptor CreateModuleDescriptor(IServiceCollection services, Type moduleType, bool isLoadedAsPlugIn = false)
        {
            return new AbpModuleDescriptor(moduleType, CreateAndRegisterModule(services, moduleType), isLoadedAsPlugIn);
        }

        protected virtual IAbpModule CreateAndRegisterModule(IServiceCollection services, Type moduleType)
        {
            var module = (IAbpModule)Activator.CreateInstance(moduleType);
            services.AddSingleton(moduleType, module);
            return module;
        }

        protected virtual void SetDependencies(List<AbpModuleDescriptor> modules, AbpModuleDescriptor module)
        {
            foreach (var dependedModuleType in AbpModuleHelper.FindDependedModuleTypes(module.Type))
            {
                var dependedModule = modules.FirstOrDefault(m => m.Type == dependedModuleType);
                if (dependedModule == null)
                {
                    throw new AbpException("Could not find a depended module " + dependedModuleType.AssemblyQualifiedName + " for " + module.Type.AssemblyQualifiedName);
                }

                module.AddDependency(dependedModule);
            }
        }
    }
}