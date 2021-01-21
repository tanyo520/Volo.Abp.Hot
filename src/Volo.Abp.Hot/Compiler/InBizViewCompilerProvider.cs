using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.Logging;

namespace Volo.Abp.Hot.Compiler
{
    public class InBizViewCompilerProvider : IViewCompilerProvider
    {
        private InBizViewCompiler _compiler;
        private ApplicationPartManager _applicationPartManager;
        private ILoggerFactory _loggerFactory;

        public InBizViewCompilerProvider(
            ApplicationPartManager applicationPartManager,
            ILoggerFactory loggerFactory)
        {
            _applicationPartManager = applicationPartManager;
            _loggerFactory = loggerFactory;
            Refresh();
        }

        public void Refresh()
        {
            var feature = new ViewsFeature();
            var controller = new ControllerFeature();
            var contrrolerProvider = new ControllerFeatureProvider();
            _applicationPartManager.PopulateFeature(feature);
            _applicationPartManager.PopulateFeature(controller);
            _compiler = new InBizViewCompiler(feature.ViewDescriptors, _loggerFactory.CreateLogger<InBizViewCompiler>());
        }

        public IViewCompiler GetCompiler() => _compiler;
    }
}
