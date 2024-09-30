using Hangfire.Dashboard;

namespace WebApiMinimalHangfire.Filters
{
    public class HangfireAuthorizatonFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            return httpContext.User.Identity.IsAuthenticated;
        }
    }
}
