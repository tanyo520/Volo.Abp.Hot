﻿using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Razor.Hosting;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Volo.Abp.Hot.Compiler
{
    public class InBizRazorAssemblyPart : ApplicationPart, IRazorCompiledItemProvider
    {
        public InBizRazorAssemblyPart(Assembly assembly, string areaName)
        {
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            AreaName = areaName;
        }

        public string AreaName { get; }

        public Assembly Assembly { get; }

        public override string Name => Assembly.GetName().Name;

        IEnumerable<RazorCompiledItem> IRazorCompiledItemProvider.CompiledItems
        {
            get
            {
                var loader = new InBizModuleViewCompiledItemLoader(AreaName);
                return loader.LoadItems(Assembly);
            }
        }
    }
}
