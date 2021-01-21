using Microsoft.AspNetCore.Razor.Hosting;
using System;

namespace Volo.Abp.Hot.Compiler
{
    public class InBizModuleViewCompiledItemLoader : RazorCompiledItemLoader
    {
        public string ModuleName { get; }

        public InBizModuleViewCompiledItemLoader(string moduleName)
        {
            ModuleName = moduleName;
        }

        protected override RazorCompiledItem CreateItem(RazorCompiledItemAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException(nameof(attribute));
            }

            return new InBizModuleViewCompiledItem(attribute, ModuleName);
        }

    }
}
