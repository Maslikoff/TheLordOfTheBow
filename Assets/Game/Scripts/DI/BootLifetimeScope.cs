using Game.Scripts.Levels;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.DI
{
    public class BootLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            ConfigureEntryPoint(builder);
        }

        private void ConfigureEntryPoint(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<Bootstrapper>();
        }
    }
}