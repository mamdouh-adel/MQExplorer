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
            builder.RegisterType<MainWindowViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<SettingsWindowViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<PublisherMQModel>().AsSelf().SingleInstance();
            builder.RegisterType<ListenerMQModel>().AsSelf().SingleInstance();
            builder.RegisterType<MQModelsHandler>().AsSelf().SingleInstance();
            builder.RegisterType<ActiveMQPublisher>().As<IMQPublisher>();
            builder.RegisterType<ActiveMQListener>().As<IMQListener>();
        }
    }
}
