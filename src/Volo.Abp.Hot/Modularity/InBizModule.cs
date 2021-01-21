using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace Volo.Abp.Hot.Modularity
{
    public class InBizModule : AbpModule, InBizModuleMeta
    {
        public virtual string Name => "InBiz模块父类";
        public virtual string Uid => "30633E78-0593-2926-3C73-0CFB671DF956";
        public virtual string Version => "1.0.0";
        public virtual string Icon => "";
        public virtual string Description => "所有InBiz模块的父类,InBiz模块继承它";

        public void SetSkipAutoServiceRegistration(bool val) {
            SkipAutoServiceRegistration = val;
        }
        public bool GetSkipAutoServiceRegistration() {
            return SkipAutoServiceRegistration;
        }
     
    }
}
