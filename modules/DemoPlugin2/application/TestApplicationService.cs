using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace DemoPlugin2.application
{
    public class TestApplicationService:ApplicationService
    {
        public string GetName() {
            return "name";
        }
    }
}
