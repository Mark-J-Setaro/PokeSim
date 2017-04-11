using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PokeSim.Startup))]
namespace PokeSim
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
