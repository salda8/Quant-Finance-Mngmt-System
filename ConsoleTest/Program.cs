using System;
using System.Linq;
using Common.EntityModels;
using Common.Interfaces;
using Server.Repositories;
using Unity;
using Unity.Injection;
using Unity.Lifetime;


namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var container = new UnityContainer();
            container.RegisterType(typeof(GenericRepository<Account>),typeof(IGenericRepository<>), new TransientLifetimeManager());
            container.RegisterType(typeof(IGenericRepository<>), typeof(GenericRepository<AccountSummary>), new TransientLifetimeManager());
            
            var registrations = container.Registrations
                   .Where(x => x.RegisteredType.IsGenericType &&
                               x.RegisteredType.GetGenericTypeDefinition() == typeof(IGenericRepository<>))
                   .ToList();

            foreach (var registration in registrations)
            {
                Console.WriteLine(registration.Name);
                var classObject = container.Resolve(registration.RegisteredType);
                Console.WriteLine(classObject.GetType());

            }

            Console.ReadKey();


        }
    }
}
