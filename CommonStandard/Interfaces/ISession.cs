using Common.Enums;
using System;

namespace Common.Interfaces
{
    public interface ISession
    {
        int ID { get; set; }

        TimeSpan OpeningTime { get; set; }
        TimeSpan ClosingTime { get; set; }

        double OpeningAsSeconds { get; set; }

        double ClosingAsSeconds { get; set; }

        bool IsSessionEnd { get; set; }

        DayOfTheWeek OpeningDay { get; set; }

        DayOfTheWeek ClosingDay { get; set; }
    }
}