namespace Volo.Abp.Hot.Core
{
    public interface ICollectibleAssemblyLoadContextProvider
    {
        CollectibleAssemblyLoadContext Get(string moduleName);
    }
}
