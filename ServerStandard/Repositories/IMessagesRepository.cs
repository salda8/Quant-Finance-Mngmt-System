using Common.EntityModels;

namespace Server.Repositories
{
    public interface IMessagesRepository
    {
        void AddCommissionMessage(CommissionMessage commissionMessage);
        void AddExecutionMessage(ExecutionMessage executionMessage);
        void AddNewOpenOrderMessage(OpenOrder openOrder);
        void AddNewOrderStatusMessage(OrderStatusMessage orderStatusMessage);
        void UpdateAccountSummary(AccountSummaryUpdate accountSummaryUpdate);
        void UpdateEquity(Equity equity);
    }
}