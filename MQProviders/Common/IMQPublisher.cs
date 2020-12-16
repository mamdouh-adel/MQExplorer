namespace MQProviders.Common
{
    public interface IMQPublisher
    {
        void StartTransaction();
        void SetPublisherModel(IMQModel publisherModel);
        IMQModel GetPublisherModel();
    }
}