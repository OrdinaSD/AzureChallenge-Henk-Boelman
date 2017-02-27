using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(HenkBoelmanSocialNetwork.Web.Startup))]
namespace HenkBoelmanSocialNetwork.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
