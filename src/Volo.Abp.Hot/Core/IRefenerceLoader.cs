using System.Reflection;

namespace Volo.Abp.Hot.Core
{
    public interface IReferenceLoader
    {
        public void LoadStreamsIntoContext(CollectibleAssemblyLoadContext context, string moduleFolder, Assembly assembly);
    }
}
