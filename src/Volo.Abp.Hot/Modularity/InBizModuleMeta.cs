using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volo.Abp.Hot.Modularity
{
   public interface InBizModuleMeta
    {
        public string Name { get; }
        public string Uid { get; }
        public string Version { get; }
        public string Icon { get; }
        public string Description { get; }
    }
}
