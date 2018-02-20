using System.Collections.Generic;
using Common.EntityModels;
using Common.Enums;
using ProtoBuf;

namespace Common.Requests
{
    [ProtoContract]
    public class DataAdditionRequest
    {
        [ProtoMember(1)]
        public BarSize Frequency { get; set; }

        [ProtoMember(2)]
        public Instrument Instrument { get; set; }

        [ProtoMember(3)]
        public List<OHLCBar> Data { get; set; }

        [ProtoMember(4)]
        public bool Overwrite { get; set; }

        public DataAdditionRequest()
        {
            
        }
        public DataAdditionRequest(BarSize frequency, Instrument instrument, List<OHLCBar> data, bool overwrite = true)
        {
            Data = data;
            Frequency = frequency;
            Instrument = instrument;
            Overwrite = overwrite;
        }

        public override string ToString()
        {
            return ($"{Data.Count} bars @ {Frequency}, instrument: {Instrument}. {(Overwrite ? "Overwrite" : "")}");
        }
    }
}
