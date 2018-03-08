using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DemoQuiniela.Startup))]
namespace DemoQuiniela
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
