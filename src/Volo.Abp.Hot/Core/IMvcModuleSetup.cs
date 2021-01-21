using Microsoft.Extensions.DependencyInjection;

namespace Volo.Abp.Hot.Core
{
    public interface IMvcModuleSetup
    {
        void DisableModule(string moduleName);
        void EnableModule(string moduleName);
        void DeleteModule(string moduleName);
    }
}
