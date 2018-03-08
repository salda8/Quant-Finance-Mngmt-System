using Common.EntityModels;
using Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommonStandard.Interfaces
{
    public interface IMyDbContext
    {
        void UpdateEntryValues(IEntity entity, IEntity newValues);
        void SetEntryState(object entity, EntityState entityState);
        DbContext DbContext { get; set; }
        // DbSet<Account> Account { get; set; }
        // DbSet<AccountSummary> AccountSummary { get; set; }
        // DbSet<CommissionMessage> CommissionMessage { get; set; }
        // DbSet<Datasource> Datasources { get; set; }
        // DbSet<Equity> Equity { get; set; }
        // DbSet<Exchange> Exchanges { get; set; }
        // DbSet<ExecutionMessage> ExecutionMessage { get; set; }
        // DbSet<ExpirationRule> ExpirationRule { get; set; }
        // DbSet<Instrument> Instruments { get; set; }
        // DbSet<InstrumentSession> InstrumentSessions { get; set; }
        // DbSet<LiveTrade> LiveTrade { get; set; }
        // DbSet<OpenOrder> OpenOrder { get; set; }
        // DbSet<OrderStatusMessage> OrderStatusMessage { get; set; }
        // DbSet<SessionTemplate> SessionTemplates { get; set; }
        // DbSet<Strategy> Strategy { get; set; }
        // DbSet<Tag> Tags { get; set; }
        // DbSet<TemplateSession> TemplateSessions { get; set; }
        // DbSet<TradeHistory> TradeHistory { get; set; }
    }
}