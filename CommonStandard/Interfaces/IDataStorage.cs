using System;
using System.Collections.Generic;
using Common.EntityModels;
using Common.Enums;

namespace Common.Interfaces
{
    public interface IDataStorage : IHistoricalDataSource
    {
        void AddData(List<OHLCBar> data, Instrument instrument, BarSize frequency, bool overwrite = false, bool adjust = true);
        void AddData(OHLCBar data, Instrument instrument, BarSize frequency, bool overwrite = false);
        void UpdateData(List<OHLCBar> data, Instrument instrument, BarSize frequency, bool adjust = false);
        List<OHLCBar> GetData(Instrument instrument, DateTime startDate, DateTime endDate, BarSize barSize = BarSize.OneDay);
        void DeleteAllInstrumentData(Instrument instrument);
        void DeleteData(Instrument instrument, BarSize frequency);
        void DeleteData(Instrument instrument, BarSize frequency, List<OHLCBar> bars);

        List<StoredDataInfo> GetStoredDataInfo(int instrumentID);
        StoredDataInfo GetStoredDataInfo(int instrumentID, BarSize barSize);
    }
}
