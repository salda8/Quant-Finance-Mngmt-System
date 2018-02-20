namespace Common.Enums
{
    public enum PushMessageType : byte
    {
        OpenOrderPush = 0, CommissionPush = 1, ExecutionPush = 2, OrderStatusPush = 3, MatlabValuePush=4, AccountUpdatePush=5, EquityUpdatePush=6,
        LiveTradePush=7,
        Pong = 9,
        Error = 7,
        Success = 10,
        Ping=8,
       
    }
}