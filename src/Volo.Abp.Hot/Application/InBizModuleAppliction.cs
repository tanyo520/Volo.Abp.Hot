using Volo.Abp.Hot.Core;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Volo.Abp.Hot.Application
{
  public class InBizModuleAppliction:ApplicationService
    {

        private IMvcModuleSetup _mvcModuleSetup;
        public InBizModuleAppliction(IMvcModuleSetup mvcModuleSetup) {
            _mvcModuleSetup = mvcModuleSetup;
        }

        public JsonResult Enable(string name)
        {
            _mvcModuleSetup.EnableModule(name);
            return new JsonResult( new { ok = true });
        }

        public JsonResult Disable(string name)
        {
            // _pluginManager.DisablePlugin(name);
            _mvcModuleSetup.DisableModule(name);
            return new JsonResult(new { ok = true });
        }
    }
}
