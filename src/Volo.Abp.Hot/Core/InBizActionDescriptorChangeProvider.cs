using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;
using System.Threading;

namespace Volo.Abp.Hot.Core
{
    public class InBizActionDescriptorChangeProvider : IActionDescriptorChangeProvider
    {
        public static InBizActionDescriptorChangeProvider Instance { get; } = new InBizActionDescriptorChangeProvider();

        public CancellationTokenSource TokenSource { get; private set; }
       
        public bool HasChanged { get; set; }

        public IChangeToken GetChangeToken()
        {
            TokenSource = new CancellationTokenSource();
            return new CancellationChangeToken(TokenSource.Token);
        }
    }
}
