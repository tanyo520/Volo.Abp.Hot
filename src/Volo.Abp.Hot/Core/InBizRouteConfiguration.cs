using Microsoft.AspNetCore.Builder;

namespace Volo.Abp.Hot.Core
{
    public static class InBizRouteConfiguration
    {
        public static IApplicationBuilder InBizModuleRoute(this IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseEndpoints(routes =>
            {
                routes.MapControllerRoute(
                    name: "areaRoute",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                routes.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                routes.MapControllerRoute(
                       name: "modules",
                       pattern: "Modules/{area}/{controller=Home}/{action=Index}/{id?}");
            });

            return app;
        }
    }
}
