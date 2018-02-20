using ProtoBuf;
using System;

namespace Common.EntityModels
{
    [ProtoContract]
    public class Tick
    {
        [ProtoMember(1)]
        public DateTime Dt { get; set; }

        [ProtoMember(2)]
        public decimal Price { get; set; }

        [ProtoMember(3)]
        public int Contracts { get; set; }

        public Tick()
        {
        }

        public Tick(DateTime dt, decimal price, int contracts)
        {
            Dt = dt;
            Price = price;
            Contracts = contracts;
        }
    }
}