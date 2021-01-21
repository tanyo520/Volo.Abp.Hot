using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.Reflection;

namespace Volo.Abp.Hot.Compiler
{
    public class InBizAssemblyPart : AssemblyPart
    {
        public InBizAssemblyPart(Assembly assembly) : base(assembly) { }
    }
}
