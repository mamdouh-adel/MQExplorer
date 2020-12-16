using ActiveMQExplorer.ViewModels;
using Autofac;
using MQProviders.ActiveMQ;
using MQProviders.Common;

namespace ActiveMQExplorer
{
    public class MQModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SettingsWindowViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<ActiveMQModel>().As<IMQModel>();
            builder.RegisterType<ActiveMQPublisher>().As<IMQPublisher>();
            builder.RegisterType<ActiveMQListener>().As<IMQListener>();
        }
    }
}
