using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Volo.Abp.Modularity;

namespace Volo.Abp.Hot.Modularity
{
    internal static class AbpModuleHelper
    {
        public static List<Type> FindAllModuleTypes(Type startupModuleType)
        {
            var moduleTypes = new List<Type>();
            AddModuleAndDependenciesResursively(moduleTypes, startupModuleType);
            return moduleTypes;
        }

        public static List<Type> FindDependedModuleTypes(Type moduleType)
        {
            if (!AbpModule.IsAbpModule(moduleType))
            {
                throw new ArgumentException("Given type is not an ABP module: " + moduleType.AssemblyQualifiedName);
            }
            var dependencies = new List<Type>();

            var dependencyDescriptors = moduleType
                .GetCustomAttributes()
                .OfType<IDependedTypesProvider>();

            foreach (var descriptor in dependencyDescriptors)
            {
                foreach (var dependedModuleType in descriptor.GetDependedTypes())
                {
                    dependencies.AddIfNotContains(dependedModuleType);
                }
            }

            return dependencies;
        }

        private static void AddModuleAndDependenciesResursively(List<Type> moduleTypes, Type moduleType)
        {
            if (!AbpModule.IsAbpModule(moduleType))
            {
                throw new ArgumentException("Given type is not an ABP module: " + moduleType.AssemblyQualifiedName);
            }
            if (moduleTypes.Contains(moduleType))
            {
                return;
            }

            moduleTypes.Add(moduleType);

            foreach (var dependedModuleType in FindDependedModuleTypes(moduleType))
            {
                AddModuleAndDependenciesResursively(moduleTypes, dependedModuleType);
            }
        }
    }
}
