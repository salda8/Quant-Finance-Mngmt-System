using Common.EntityModels;
using CommonStandard.Interfaces;
using Microsoft.EntityFrameworkCore;
using Server.Repositories;
using System;

public class UnitOfWork : IDisposable
{
    private readonly IMyDbContext context;
    private readonly DbContext dbContext;
    private GenericRepository<Account> accountRepository;

    private GenericRepository<AccountSummary> accountSummaryRepository;

    private GenericRepository<CommissionMessage> commissionMessageRepository;

    private bool disposed;

    private GenericRepository<Equity> equityRepository;

    private GenericRepository<Exchange> exchangeRepository;

    private GenericRepository<ExecutionMessage> executionMessageRepository;

    private InstrumentRepository instrumentRepository;

    private GenericRepository<OHLCBar> ohlcBarRepository;

    private GenericRepository<OpenOrder> openOrderRepository;

    private GenericRepository<OrderStatusMessage> orderStatusMessageRepository;

    private GenericRepository<SessionTemplate> sessionTemplateRepository;

    private GenericRepository<Strategy> strategyRepository;

    private GenericRepository<TradeHistory> tradeHistoryRepository;

    public UnitOfWork(IMyDbContext context)
    {
        this.context = context;
        dbContext = context.DbContext;
    }

    public GenericRepository<AccountSummary> AccountSummaryRepository => accountSummaryRepository ?? (accountSummaryRepository= new GenericRepository<AccountSummary>(context));

    public GenericRepository<CommissionMessage> CommissionMessageRepository => commissionMessageRepository ?? (commissionMessageRepository=new GenericRepository<CommissionMessage>(context));

    public GenericRepository<Account> DepartmentRepository => accountRepository ?? (accountRepository= new GenericRepository<Account>(context));

    public GenericRepository<Equity> EquityRepository => equityRepository ?? (equityRepository=new GenericRepository<Equity>(context));

    public GenericRepository<Exchange> ExchangeRepository => exchangeRepository ?? (exchangeRepository=new GenericRepository<Exchange>(context));

    public GenericRepository<ExecutionMessage> ExecutionMessageRepository => executionMessageRepository ?? (executionMessageRepository= new GenericRepository<ExecutionMessage>(context));

    public InstrumentRepository InstrumentRepository => instrumentRepository ?? (instrumentRepository=new InstrumentRepository(context));

    public GenericRepository<OHLCBar> OhlcBarRepository => ohlcBarRepository ?? (ohlcBarRepository=new GenericRepository<OHLCBar>(context));

    public GenericRepository<OpenOrder> OpenOrderRepository => openOrderRepository ?? (openOrderRepository=new GenericRepository<OpenOrder>(context));

    public GenericRepository<OrderStatusMessage> OrderStatusMessageRepository => orderStatusMessageRepository ?? (orderStatusMessageRepository=new GenericRepository<OrderStatusMessage>(context));

    // private GenericRepository<Server> serverRepository; public GenericRepository<Server>
    // ServerRepository { get { return this.serverRepository ?? new
    // GenericRepository<Server>(context); } }
    public GenericRepository<SessionTemplate> SessionTemplateRepository => sessionTemplateRepository ?? (sessionTemplateRepository=new GenericRepository<SessionTemplate>(context));

    public GenericRepository<Strategy> StrategyRepository => strategyRepository ?? (strategyRepository=new GenericRepository<Strategy>(context));

    public GenericRepository<TradeHistory> TradeHistoryRepository => tradeHistoryRepository ?? (tradeHistoryRepository=new GenericRepository<TradeHistory>(context));

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Save()
    {
        dbContext.SaveChanges();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                dbContext.Dispose();
            }
        }
        disposed = true;
    }
}