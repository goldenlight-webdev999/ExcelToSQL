using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GroGroup.Startup))]
namespace GroGroup
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
