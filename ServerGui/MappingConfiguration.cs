using Common.EntityModels;
using Common.Enums;
using ExpressMapper;

namespace ServerGui
{
    public class MappingConfiguration
    {
        public static void Register()
        {
            Mapper.Register<ExecutionMessage, LiveTrade >()
                .Member(dest => dest.AveragePrice, src => src.Price)
                .Member(dest => dest.TradeDirection, src => ConvertFromString(src.Side))
                .Member(dest=>dest.UpdateTime, src=>src.Time);

            Mapper.Compile();
        }

        private static TradeDirection ConvertFromString(string side)
            => side == "BUY" ? TradeDirection.Long : TradeDirection.Short;
    }
}