using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common;
using Common.EntityModels;
using Common.Enums;
using DataAccess;
using ExpressMapper.Extensions;

namespace Server.Repositories
{
    public class MessagesRepository : Repository//, IMessagesRepository
    {
        //private readonly MyDBContext context;

        private Func<LiveTrade, bool> accountIdAndSymbolForOrdersEqualFunc;
        private Dictionary<int, string> openOrdersDictionary = new Dictionary<int, string>();
        private bool triedAgain = false;

        public MessagesRepository()
        {
        }

        public void AddExecutionMessage(ExecutionMessage executionMessage)
        {
            using (var dbContext = new MyDBContext())
            {
                Add(executionMessage, dbContext);
                SaveChanges(dbContext);
            }
        }

        public void AddCommissionMessage(CommissionMessage commissionMessage)
        {
            using (var dbContext = new MyDBContext())
            {
                Add(commissionMessage, dbContext);
                SaveChanges(dbContext);

                if (commissionMessage.RealizedPnL != 1000000)
                {
                    //this commission message is the one which comes with opening of new trade
                 
                    AddNewTradeHistory(commissionMessage);
                }
            }
        }

        public void AddNewOrderStatusMessage(OrderStatusMessage orderStatusMessage)
        {
           
            using (var dbContext = new MyDBContext())
            {

                if (orderStatusMessage.Status.Contains("Cancel"))
                    
                {
                    var openorder =
                        dbContext.OpenOrder.Where(
                            x => x.PermanentId == orderStatusMessage.PermanentId).ToList();
                    if (openorder?.Count > 0)
                    {
                        DeleteRange(openorder, dbContext);
                        SaveChanges(dbContext);
                    }
                }

                Add(orderStatusMessage, dbContext);
                SaveChanges(dbContext);

            }
        }

        public void AddNewOpenOrderMessage(OpenOrder openOrder)
        {
            using (var dbContext = new MyDBContext())
            {
                openOrder.UpdateTime = DateTime.Now;
                if (openOrder.Status == "Cancelled" || openOrder.Status == "Filled")
                {
                    var openorder =
                        dbContext.OpenOrder.Where(x => x.PermanentId == openOrder.PermanentId).ToList();
                    if (openorder.Count >0)
                    {
                        DeleteAndSaveOpenOrder(openorder, dbContext);
                    }
                }
                
                else
                {

                    openOrdersDictionary.TryGetValue(openOrder.OrderId, out string status);

                    if (string.IsNullOrWhiteSpace(status))
                    {

                        openOrdersDictionary.Add(openOrder.OrderId, openOrder.Status);
                        Add(openOrder, dbContext);
                        SaveChanges(dbContext);
                    }
                    else
                    {
                        if (status == openOrder.Status)
                        {
                            return;
                        }
                        else if (status != openOrder.Status)
                        {
                            openOrdersDictionary[openOrder.OrderId] = openOrder.Status;
                            //not important to update database on this instance
                            

                        }
                    }
                    
                    
                }
            }
        }

        private void DeleteAndSaveOpenOrder(List<OpenOrder> openorder, MyDBContext dbContext)
        {
            DeleteRange(openorder, dbContext);
            SaveChanges(dbContext);
        }

        private void AddNewTradeHistory(CommissionMessage commissionMessage)
        {
            using (var context = new MyDBContext())
            {
                var executionMessage =
                    this.SingleOrDefault<ExecutionMessage>(x => x.ExecutionId == commissionMessage.ExecutionId,
                        context);
                if (executionMessage != null)
                {
                    var tradeHistory = new TradeHistory
                    {
                        ID = commissionMessage.ID,
                        AccountID = executionMessage.AccountID,
                        ExecutionID = executionMessage.ExecutionId,
                        ExecutionTime = executionMessage.Time,
                        Side = ConvertFromString(executionMessage.Side),
                        Quantity = executionMessage.Quantity,
                        InstrumentID = executionMessage.InstrumentID,
                        Price = executionMessage.Price,
                        Commission = commissionMessage.Commission,
                        RealizedPnL = new decimal(commissionMessage.RealizedPnL)
                    };
                    Add(tradeHistory, context);
                    SaveChanges(context);
                }
                else
                {
                    Thread.Sleep(1000);
                    AddNewTradeHistory(commissionMessage);
                }
            }
        }

        private void ProcessNewExecutionMessage(string commissionMessageExecutionId)
        {
            using (var dbContext = new MyDBContext())
            {
                var liveTradesList = GetAll<LiveTrade>(dbContext).ToList();

                var executionMessage =
                    SingleOrDefault<ExecutionMessage>(x => x.ExecutionId == commissionMessageExecutionId, dbContext);
                accountIdAndSymbolForOrdersEqualFunc = x => x.AccountID == executionMessage.AccountID &&
                                                            x.InstrumentID ==
                                                            executionMessage.InstrumentID;

                if (executionMessage != null)
                {
                    var liveTrade = new LiveTrade {UpdateTime = DateTime.Now};

                    var item = liveTradesList.LastOrDefault(accountIdAndSymbolForOrdersEqualFunc);
                    if (item == null)
                    {
                        executionMessage.Map(liveTrade);

                        Add(liveTrade, dbContext);
                        SaveChanges(dbContext);
                    }
                    else if (ItIsSameSideTrade(executionMessage, liveTradesList))
                    {
                        var newFillPrice = (item.AveragePrice * item.Quantity +
                                            executionMessage.Price * executionMessage.Quantity) /
                                           (item.Quantity + executionMessage.Quantity);
                        var newQuantity = item.Quantity + executionMessage.Quantity;
                        executionMessage.Map(liveTrade);
                        liveTrade.Quantity = newQuantity;
                        liveTrade.AveragePrice = newFillPrice;

                        RemoveAndAddToDatabase(liveTrade, executionMessage, dbContext);
                    }
                    else //different side trade
                    {
                        if (executionMessage.Quantity > item.Quantity)
                        {
                            var newFillPrice = (item.AveragePrice * item.Quantity -
                                                executionMessage.Price * executionMessage.Quantity) /
                                               (item.Quantity - executionMessage.Quantity);
                            var newQuantity = executionMessage.Quantity - item.Quantity;
                            executionMessage.Map(liveTrade);
                            liveTrade.AveragePrice = newFillPrice;
                            liveTrade.Quantity = newQuantity;

                            RemoveAndAddToDatabase(liveTrade, executionMessage, dbContext);
                        }
                        else
                        {
                            DeleteRange(dbContext.LiveTrade.Where(
                                x => x.AccountID == executionMessage.AccountID), dbContext);
                            SaveChanges(dbContext);
                        }
                    }
                }
                else if (!triedAgain) //wait and try again
                {
                    Thread.Sleep(1000);
                    triedAgain = true;
                    ProcessNewExecutionMessage(commissionMessageExecutionId);
                }
            }
        }

        private void RemoveAndAddToDatabase(LiveTrade liveTrade, ExecutionMessage message, MyDBContext dbContext)
        {
            DeleteRange(dbContext.LiveTrade.Where(
                x => x.AccountID == message.AccountID), dbContext);
            Add(liveTrade, dbContext);
            SaveChanges(dbContext);
        }

        private static bool ItIsSameSideTrade(ExecutionMessage message, List<LiveTrade> liveTrades)
        {
            return liveTrades.Any(
                x =>
                    x.TradeDirection == ConvertFromString(message.Side) &&
                    x.InstrumentID == message.InstrumentID);
        }

        private static TradeDirection ConvertFromString(string side)
            => side == "BOT" ? TradeDirection.Long : TradeDirection.Short;



        public void AddOrUpdateLiveTrade(LiveTrade liveTrade)
        {
            using (var dbContext = new MyDBContext())
            {
                var foundLiveTrade = dbContext.LiveTrade.SingleOrDefault(x => x.AccountID == liveTrade.AccountID);
                if (foundLiveTrade == null)
                {
                    dbContext.LiveTrade.Add(liveTrade);
                }
                else
                {
                    dbContext.UpdateEntryValues(foundLiveTrade, liveTrade);
                }

                dbContext.SaveChanges();

            }
        }

        public void UpdateAccountSummary(AccountSummaryUpdate accountSummaryUpdate)
        {

            using (var dbContext = new MyDBContext())
            {
                var accountToChange =
                    dbContext.AccountSummary.SingleOrDefault(x => x.AccountID == accountSummaryUpdate.AccountID);
                if (accountToChange != null)
                {
                    switch (accountSummaryUpdate.Key)
                    {
                        case "NetLiquidation":
                            accountToChange.NetLiquidation = Convert.ToDecimal(accountSummaryUpdate.Value);
                            dbContext.Equity.Add(new Equity()
                            {
                                AccountID = accountSummaryUpdate.AccountID,
                                UpdateTime = DateTime.Now,
                                Value = Convert.ToDecimal(accountSummaryUpdate.Value)
                            });
                            dbContext.SaveChanges();
                            break;

                        case "CashBalance":
                            accountToChange.CashBalance = Convert.ToDecimal(accountSummaryUpdate.Value);
                            dbContext.SaveChanges();
                            break;

                        case "DayTradesRemaining":
                            Convert.ToDecimal(accountSummaryUpdate.Value);
                            dbContext.SaveChanges();
                            break;

                        case "EquityWithLoanValue":
                            Convert.ToDecimal(accountSummaryUpdate.Value);
                            dbContext.SaveChanges();
                            break;

                        case "InitMarginReq":
                            Convert.ToDecimal(accountSummaryUpdate.Value);
                            dbContext.SaveChanges();
                            break;

                        case "MaintMarginReq":
                            Convert.ToDecimal(accountSummaryUpdate.Value);
                            dbContext.SaveChanges();
                            break;

                        case "UnrealizedPnL":
                            accountToChange.UnrealizedPnL = Convert.ToDecimal(accountSummaryUpdate.Value);
                            dbContext.SaveChanges();
                            break;

                    }
                }
                
            }
        }

        public void UpdateEquity(Equity equity)
        {
            using (var dbContext = new MyDBContext())
            {
                dbContext.Equity.Add(equity);
                dbContext.SaveChangesAsync();
            }
        }
    }
}