namespace Common.Enums
{
    public enum DataRequestMessageType : byte
    {
        DefaultOne,
        Pong,
        Error,
        Success,
        RTDCanceled,
        CancelRTD,
        RTDRequest,
        HistPush,
        Ping,
        AvailableDataReply,
        HistReply,
        HistPushReply,
        HistRequest,
        AvailableDataRequest,
        ErrorPush
    }
}