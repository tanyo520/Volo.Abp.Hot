using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace TestAbp1
{
    public class TestApplitionService: ApplicationService
    {

        public string GetName() {
            return "name";
        }
    }
}
