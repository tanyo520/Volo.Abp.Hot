using DemoPlugin2.application;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Hot.Modularity;
using Volo.Abp.Modularity;

namespace DemoPlugin2
{
    public class DemoPluginModule:InBizModule
    {

        public override string Name => "测试Demo";

        public override string Uid => "89CF6E61-27F1-2748-1F12-72471DF45F04";

        public override string Version => "1.0.0";

        public override string Icon => "";

        public override string Description => "这是测试的demo";

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpAspNetCoreMvcOptions>(options =>
            {
                options.ConventionalControllers.Create(typeof(TestApplicationService).Assembly);
            });
        }
    }
}
