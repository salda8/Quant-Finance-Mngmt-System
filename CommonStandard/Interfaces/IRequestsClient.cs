using Common.EntityModels;
using Common.Enums;

namespace Common.Interfaces
{
    public interface IRequestsClient : INetMQClient
    {
        void PushOrdersInfo(object objectToSend, PushMessageType messageType);

        Instrument RequestActiveInstrumentContract(int underlyingSymbol);

        Account RequestAccount(int strategyID);


    }
}