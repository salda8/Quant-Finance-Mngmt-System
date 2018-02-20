using Common.EntityModels;
using Common.Enums;
using Common.ExtensionMethods;
using Common.Interfaces;
using Common.Utils;
using NLog;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CommonStandard.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Server.Repositories
{
    /// <summary>
    ///     This class is used to add, remove, search for, and modify instruments.
    /// </summary>
    public class InstrumentRepository : IInstrumentRepository
    {
        public readonly IMyDbContext Context;
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public InstrumentRepository(IMyDbContext context)
        {
            Context = context;
        }

        #region IInstrumentSource Members

        /// <summary>
        ///     Add a new instrument
        /// </summary>
        /// <param name="instrument"></param>
        /// <param name="saveChanges">Set to true if saving to db should be done.</param>
        /// <returns>True if the insertion or update succeeded. False if it did not.</returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<Instrument> AddInstrument(Instrument instrument, bool saveChanges = true)
        {
            //Check if an instrument with these unique constraints already exists
            var existingInstrument = Context.Instruments.SingleOrDefault(x =>
                x.ID == instrument.ID ||
                x.Symbol == instrument.Symbol &&
                x.DatasourceID == instrument.DatasourceID &&
                x.ExchangeID == instrument.ExchangeID &&
                x.Expiration == instrument.Expiration &&
                x.Type == instrument.Type);

            if (existingInstrument != null)
            {
                throw new ArgumentException("Unique constraint violation");
            }

            ValidateInstrument(instrument);

            instrument.Datasource = Context.GetAttachedEntity(instrument.Datasource);
            instrument.Exchange = Context.GetAttachedEntity(instrument.Exchange);

            instrument.Tags = new List<Tag>() { new Tag() { Name = GetDescriptionHelper.GetDescription(instrument.Type, "FUTURE") } };
            instrument.ExpirationRuleID = 1;

            AddSessionToInstrument(instrument);

            Context.Instruments.Add(instrument);
            if (saveChanges)
            {
                await Context.DbContext.SaveChangesAsync().ConfigureAwait(false);
            }

            logger.Info($"Instrument Manager: successfully added instrument {instrument}");

            return instrument;
        }

        private void AddSessionToInstrument(Instrument instrument)
        {
            if (instrument.SessionsSource == SessionsSource.Template)
            {
                SessionTemplate template = Context.SessionTemplates.Include(x => x.Sessions)
                    .FirstOrDefault(x => x.ID == instrument.SessionTemplateID);

                if (template?.Sessions != null)
                {
                    //todo
                    //instrument.InstrumentInstrumentSessions.Sessions = new List<InstrumentSession>();
                    //foreach (TemplateSession s in template.Sessions)
                    //{
                    //    instrument.Sessions.Add(s.ToInstrumentSession());
                    //}
                }
            }
        }

        /// <summary>
        ///     Updates the instrument with new values.
        /// </summary>
        public async Task UpdateInstrument(Instrument attachedInstrument, Instrument newValues)
        {
            if (attachedInstrument == null) {throw new ArgumentNullException(nameof(attachedInstrument));}
            if (newValues == null) {throw new ArgumentNullException(nameof(newValues));}

            ValidateInstrument(newValues);

            Context.UpdateEntryValues(attachedInstrument, newValues);

            attachedInstrument.Tags.UpdateCollection(newValues.Tags, Context);

            if (newValues.SessionsSource == SessionsSource.Custom)
            {
                //todo
                //attachedInstrument.InstrumentInstrumentSessions.UpdateCollectionAndElements(newValues.Sessions, Context);
            }

            await Context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<List<Instrument>> FindInstruments(Expression<Func<Instrument, bool>> pred)
        {
            return await GetIQueryable().Where(pred).ToListAsync().ConfigureAwait(false);
        }

        /// <summary>
        ///     Search for instruments.
        /// </summary>
        /// <param name="search">
        ///     Any properties set on this instrument are used as search parameters.
        ///     If null, all instruments are returned.
        /// </param>
        /// <returns>A list of instruments matching the criteria.</returns>
        public async Task<List<Instrument>> FindInstruments(Instrument search = null)
        {
            IQueryable<Instrument> query = GetIQueryable();

            if (search == null)
            {
                return await FindAllInstruments(query).ConfigureAwait(false);
            }
            //first handle the cases where there is a single, unique instrument to return
            if (search.ID > 0)
            {
                query = query.Where(x => x.ID == search.ID);
            }
            else if (search.Symbol != null && search.DatasourceID != default && search.Type != InstrumentType.Undefined)
            {
                query = query.Where(x => x.Symbol == search.Symbol
                                         && x.DatasourceID == search.DatasourceID
                                         && x.ExchangeID == search.ExchangeID
                                         && x.Expiration == search.Expiration

                                         && x.Type == search.Type);
            }
            else
            {
                BuildQueryFromSearchInstrument(search, ref query);
            }

            return await query.ToListAsync().ConfigureAwait(false);
        }

        /// <summary>
        ///     Delete an instrument and all locally stored data.
        /// </summary>
        public async Task RemoveInstrument(Instrument instrument, IDataStorage localStorage)
        {
            Context.Instruments.Attach(instrument);
            Context.Instruments.Remove(instrument);
            await Context.DbContext.SaveChangesAsync().ConfigureAwait(false);

            localStorage.Connect();

            localStorage.DeleteAllInstrumentData(instrument);
        }

        #endregion IInstrumentSource Members

        /// <summary>
        /// </summary>
        /// <param name="instrument"></param>
        /// <exception cref="ArgumentException"></exception>
        private void ValidateInstrument(Instrument instrument)
        {
            //make sure data source is set and exists
            if (instrument.Datasource == null || Context.Datasources.Find(instrument.DatasourceID) == null)
                throw new ArgumentException("Invalid datasource.");

            //make sure exchange exists, if it is set
            if (instrument.Exchange != null && Context.Exchanges.Find(instrument.ExchangeID) == null)
                throw new ArgumentException("Exchange does not exist.");
        }

        private IQueryable<Instrument> GetIQueryable()
        {
            return Context.Instruments
                .Include(x => x.Tags)
                .Include(x => x.Exchange)
                .Include(x => x.ExpirationRule)
                .Include(x => x.Datasource)
                .Include(x => x.Sessions)

                .AsQueryable();
        }

        // ReSharper disable once FunctionComplexityOverflow
        // ReSharper disable once CyclomaticComplexity
        private static void BuildQueryFromSearchInstrument(Instrument search, ref IQueryable<Instrument> query)
        {
            if (!string.IsNullOrEmpty(search.Symbol))
                query = query.Where(x => x.Symbol.Contains(search.Symbol));

            if (!string.IsNullOrEmpty(search.UnderlyingSymbol))
                query = query.Where(x => x.UnderlyingSymbol.Contains(search.UnderlyingSymbol));

            if (!string.IsNullOrEmpty(search.Name))
                query = query.Where(x => x.Name.Contains(search.Name));

            query = query.Where(x => x.ExchangeID == search.ExchangeID);

            if (search.DatasourceID!=(default))
                query = query.Where(x => x.DatasourceID == search.DatasourceID);

            if (search.Type != InstrumentType.Undefined)
                query = query.Where(x => x.Type == search.Type);

            if (!string.IsNullOrEmpty(search.Currency))
                query = query.Where(x => x.Currency == search.Currency);

            if (!string.IsNullOrEmpty(search.ValidExchanges))
                query = query.Where(x => x.ValidExchanges.Contains(search.ValidExchanges));
        }

        private static async Task<List<Instrument>> FindAllInstruments(IQueryable<Instrument> query)
        {
            return await query.ToListAsync().ConfigureAwait(false);
        }
    }
}