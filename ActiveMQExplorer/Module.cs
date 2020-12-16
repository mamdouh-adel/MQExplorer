using ActiveMQExplorer.ViewModels;
using ActiveMQExplorer.Views;
using Autofac;
using MQProviders;
using MQProviders.Contracts;

namespace ActiveMQExplorer
{
    public class Module : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MainWindowViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<FirstHelloWorld>().As<IHelloWorld>();
          
            builder.RegisterType<MainWindowView>().PropertiesAutowired().SingleInstance();
            builder.RegisterType<SettingsWindowView>().PropertiesAutowired().SingleInstance();
        }
    }
}
