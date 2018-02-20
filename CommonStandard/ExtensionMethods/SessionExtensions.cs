﻿using Common.EntityModels;
using Common.Interfaces;
using System;
using System.Globalization;

namespace Common.ExtensionMethods
{
    public static class SessionExtensions
    {
        public static InstrumentSession ToInstrumentSession(this ISession session)
        {
            return new InstrumentSession
            {
                OpeningDay = session.OpeningDay,
                OpeningTime = TimeSpan.FromSeconds(session.OpeningTime.TotalSeconds),
                ClosingDay = session.ClosingDay,
                ClosingTime = TimeSpan.FromSeconds(session.ClosingTime.TotalSeconds),
                IsSessionEnd = session.IsSessionEnd
            };
        }

        public static bool Overlaps(this ISession session1, ISession session2)
        {
            //Create starting and ending DTs for the sessions
            var arbitraryStartPoint = new DateTime(2014, 1, 1, 0, 0, 0, 0, new GregorianCalendar(), DateTimeKind.Utc);
            SessionToDTs(session1, arbitraryStartPoint, out DateTime session1Start, out DateTime session1End);

            //to make sure all overlap scenarios are covered, the 2nd session is done both backwards and forwards
            SessionToDTs(session2, session1Start, out DateTime session2StartForward, out DateTime session2EndForward);
            SessionToDTs(session2, session1Start, out DateTime session2StartBack, out DateTime session2EndBack, false);

            if (DateTimePeriodsOverlap(session1Start, session1End, session2StartBack, session2EndBack))
                return true;
            if (DateTimePeriodsOverlap(session1Start, session1End, session2StartForward, session2EndForward))
                return true;

            return false;
        }

        private static bool DateTimePeriodsOverlap(DateTime p1start, DateTime p1end, DateTime p2start, DateTime p2end)
        {
            //engulfing
            if (p1start > p2start && p1end < p2end)
            {
                return true;
            }

            if (p2start > p1start && p2end < p1end)
            {
                return true;
            }

            //partial overlap
            if (p1start < p2end && p1end > p2end)
            {
                return true;
            }

            if (p2start < p1end && p2end > p1end)
            {
                return true;
            }

            return false;
        }

        private static void SessionToDTs(ISession session, DateTime startingPoint, out DateTime start, out DateTime end, bool forwards = true)
        {
            start = startingPoint;
            while (start.DayOfWeek.ToInt() != (int)session.OpeningDay)
            {
                start = start.AddDays(forwards ? 1 : -1);
            }

            end = start;
            while (end.DayOfWeek.ToInt() != (int)session.ClosingDay)
            {
                end = end.AddDays(1);
            }

            start = start.Date + session.OpeningTime;
            end = end.Date + session.ClosingTime;
        }
    }
}