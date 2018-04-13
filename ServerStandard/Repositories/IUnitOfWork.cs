using Common.EntityModels;
using Server.Repositories;

namespace ServerStandard.Repositories
{
    public interface IUnitOfWork
    {
        
        void Save();
    }
}