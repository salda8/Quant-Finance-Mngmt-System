using System;
using Common.EntityModels;
using CommonStandard.Interfaces;
using Microsoft.EntityFrameworkCore;
using Server.Repositories;

namespace ServerStandard.Repositories
{
    public class UnitOfWork : IDisposable, IUnitOfWork
    {
        private readonly IMyDbContext dbContext;
        private bool disposed;


        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork" /> class.
        /// </summary>
        /// <param name="dbcontext">The dbcontext.</param>
        public UnitOfWork(IMyDbContext dbcontext)
        {
            this.dbContext = dbcontext;
        }

        

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        

        public void Save()
        {
            dbContext.DbContext.SaveChanges();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    dbContext.DbContext.Dispose();
                }
            }
            disposed = true;
        }
    }
}