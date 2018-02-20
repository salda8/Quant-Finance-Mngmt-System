// This class is used to filter data, removing any bars that happen outside of
// regular trading hours.

using Common.EntityModels;
using Common.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server
{
    public static class RthFilter
    {
        /// <summary>
        ///     Removes bars outside of regular trading hours, as defined by the provided sessions.
        /// </summary>
        /// <param name="data">The data to be filtered, ordered with the earliest bar first.</param>
        /// <param name="sessions"></param>
        /// <returns></returns>
        public static void Filter(List<OHLCBar> data, List<InstrumentSession> sessions)
        {
            if (data == null) throw new NullReferenceException("data");
            if (sessions == null) throw new NullReferenceException("sessions");
            if (sessions.Count == 0) return;

            sessions = sessions.OrderBy(x => x.OpeningDay).ThenBy(x => x.OpeningTime).ToList();

            //start by grabbing the first session
            var firstBar = data[0];
            var currentSession = FirstSessionAfter(firstBar.DateTimeClose, sessions);
            var sessionIndex = sessions.IndexOf(currentSession);

            bool inSession = firstBar.DateTimeClose.InSession(currentSession);
            SessionToDt(firstBar.DateTimeClose, currentSession, out DateTime nextOpeningDt, out DateTime nextClosingDt);

            var i = 0;
            while (i < data.Count)
            {
                if (inSession)
                {
                    //Currently in a session. Check if we have moved outside it
                    if (data[i].DateTimeClose > nextClosingDt)
                    {
                        //find next session
                        sessionIndex = sessionIndex < sessions.Count - 1 ? sessionIndex + 1 : 0;
                        currentSession = sessions[sessionIndex];
                        // Use the previous bar here, because if we jump over a session and we used the current bar,
                        // it gives us next week's times instead of this week's
                        SessionToDt(data[i - 1].DateTimeClose, currentSession, out nextOpeningDt, out nextClosingDt);

                        //Is this bar already inside the next session?
                        if (data[i].DateTimeClose > nextOpeningDt)
                        {
                            //check if we have "overshot", if not then we're in the session
                            if (data[i].DateTimeClose < nextClosingDt)
                            {
                                i++;
                                continue;
                            }

                            //we have overshot the next session, start from scratch
                            currentSession = FirstSessionAfter(data[i].DateTimeClose, sessions);
                            sessionIndex = sessions.IndexOf(currentSession);
                            SessionToDt(data[i].DateTimeClose, currentSession, out nextOpeningDt, out nextClosingDt);

                            if (data[i].DateTimeClose.InSession(currentSession))
                            {
                                i++;
                                continue;
                            }
                        }

                        inSession = false;
                        data.RemoveAt(i);
                        continue;
                    }

                    i++;
                }
                else
                {
                    //Currently not in a session, check if this bar is inside the next session
                    if (data[i].DateTimeClose > nextOpeningDt)
                    {
                        if (data[i].DateTimeClose > nextClosingDt)
                        {
                            //the bar overshoots the next session, start from scratch
                            currentSession = FirstSessionAfter(data[i].DateTimeClose, sessions);
                            sessionIndex = sessions.IndexOf(currentSession);
                            SessionToDt(data[i].DateTimeClose, currentSession, out nextOpeningDt, out nextClosingDt);

                            if (data[i].DateTimeClose.InSession(currentSession))
                            {
                                inSession = true;
                                i++;
                                continue;
                            }
                        }
                        else
                        {
                            inSession = true;
                            i++;
                            continue;
                        }
                    }

                    data.RemoveAt(i);
                }
            }
        }

        private static InstrumentSession FirstSessionAfter(DateTime date, List<InstrumentSession> sessions)
        {
            return
                sessions
                    .FirstOrDefault(x =>
                        (int)x.ClosingDay >= date.DayOfWeek.ToInt() &&
                        x.ClosingTime >= date.TimeOfDay)
                ?? sessions.First();
        }

        /// <summary>
        ///     Given a DateTime, calculates the DateTime of the next open and close of a given session.
        /// </summary>
        private static void SessionToDt(DateTime startingPoint, InstrumentSession session, out DateTime startingDt,
            out DateTime endingDt)
        {
            startingDt = SessionToDt(startingPoint, session, false);
            endingDt = SessionToDt(startingPoint, session, true);
        }

        private static DateTime SessionToDt(DateTime startingPoint, InstrumentSession session, bool closing)
        {
            var currentDt = startingPoint;

            var targetDay = closing ? (int)session.ClosingDay : (int)session.OpeningDay;

            var time = closing ? session.ClosingTime : session.OpeningTime;

            if (currentDt.DayOfWeek.ToInt() == targetDay && currentDt.TimeOfDay > session.ClosingTime)
                currentDt = currentDt.AddDays(7);

            while (currentDt.DayOfWeek.ToInt() != targetDay)
            {
                currentDt = currentDt.AddDays(1);
            }

            return currentDt.Date + time;
        }

        /// <summary>
        ///     Determine if a datetime falls inside a given session or not.
        /// </summary>
        public static bool InSession(this DateTime dt, InstrumentSession session)
        {
            var dotw = dt.DayOfWeek.ToInt();

            if (session.OpeningDay > session.ClosingDay)
            {
                //the session spans multiple weeks
                if (dotw > (int)session.OpeningDay || dotw < (int)session.ClosingDay)
                    return true;

                if (dotw == (int)session.OpeningDay && dt.TimeOfDay > session.OpeningTime)
                    return true;

                if (dotw == (int)session.ClosingDay && dt.TimeOfDay <= session.ClosingTime)
                    return true;
            }
            else
            {
                //session is intraweek
                if (dotw < (int)session.OpeningDay || dotw > (int)session.ClosingDay)
                    return false;

                if (dotw == (int)session.OpeningDay && dt.TimeOfDay <= session.OpeningTime)
                    return false;

                if (dotw == (int)session.ClosingDay && dt.TimeOfDay > session.ClosingTime)
                    return false;

                return true;
            }

            return false;
        }

        /// <summary>
        ///     Determine if a datetime falls inside a given session list or not.
        /// </summary>
        /// <returns>
        ///     Returns true, if the datetime falls into one of the given session list.
        /// </returns>
        public static bool InSession(this DateTime dt, IEnumerable<InstrumentSession> sessions)
        {
            return sessions.Any(session => InSession(dt, session));
        }
    }
}