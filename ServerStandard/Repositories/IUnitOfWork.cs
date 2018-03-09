using Common.EntityModels;
using Server.Repositories;

namespace ServerStandard.Repositories
{
    public interface IUnitOfWork
    {
        GenericRepository<AccountSummary> AccountSummaryRepository { get; }
        GenericRepository<CommissionMessage> CommissionMessageRepository { get; }
        GenericRepository<Account> AccountRepository { get; }
        GenericRepository<Equity> EquityRepository { get; }
        GenericRepository<Exchange> ExchangeRepository { get; }
        GenericRepository<ExecutionMessage> ExecutionMessageRepository { get; }
        InstrumentRepository InstrumentRepository { get; }
        GenericRepository<OHLCBar> OhlcBarRepository { get; }
        GenericRepository<OpenOrder> OpenOrderRepository { get; }
        GenericRepository<OrderStatusMessage> OrderStatusMessageRepository { get; }
        GenericRepository<SessionTemplate> SessionTemplateRepository { get; }
        GenericRepository<Strategy> StrategyRepository { get; }
        GenericRepository<TradeHistory> TradeHistoryRepository { get; }

        void Save();
    }
}