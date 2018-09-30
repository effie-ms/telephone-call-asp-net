using System;
using System.Collections.Generic;
using System.Linq;

namespace TelephoneCallsWebApplication.Models.DataGeneration
{
    public class CallGeneration
    {
        private const int TOTAL_CALLS_NUMBER = 100;
        private const double FULL_CALLS_PERCENTAGE = 0.8;
        private const double CANCELLED_CALLS_PERCENTAGE = 0.15;
        private const double NON_DIALED_CALLS_PERCENTAGE = 0.05;

        private static Call GenerateNonDialledCall(List<int> phoneNumbersList, List<Call> calls)
        {
            do
            {
                int callerSequenceNumber, receiverSequenceNumber;
                PhoneNumberGeneration.GenerateInterlocutorsNumbers(out callerSequenceNumber, out receiverSequenceNumber);
                Call call = new Call(phoneNumbersList[callerSequenceNumber], phoneNumbersList[receiverSequenceNumber]);
                List<Event> events = EventGeneration.GenerateNonDialledCallEvents(call);
                call.Events = events;
                if (!CallerIsBusy(call.Caller, GetAllCallersBusyTimeIntervals(calls), GetCallerTimeInterval(call)))
                {
                    return call;
                }
            } while (true);
        }

        private static Call GenerateCancelledCall(List<int> phoneNumbersList, List<Call> calls)
        {
            do
            {
                int callerSequenceNumber, receiverSequenceNumber;
                PhoneNumberGeneration.GenerateInterlocutorsNumbers(out callerSequenceNumber, out receiverSequenceNumber);
                Call call = new Call(phoneNumbersList[callerSequenceNumber], phoneNumbersList[receiverSequenceNumber]);
                List<Event> events = EventGeneration.GenerateCancelledCallEvents(call);
                call.Events = events;
                if (!CallerIsBusy(call.Caller, GetAllCallersBusyTimeIntervals(calls), GetCallerTimeInterval(call)) &&
                    !CallerIsBusy(call.Receiver.Value, GetAllCallersBusyTimeIntervals(calls), GetReceiverTimeInterval(call)))
                {
                    return call;
                }
            } while (true);
        }

        private static Call GenerateFullCall(List<int> phoneNumbersList, List<Call> calls)
        {
            do
            {
                int callerSequenceNumber, receiverSequenceNumber;
                PhoneNumberGeneration.GenerateInterlocutorsNumbers(out callerSequenceNumber, out receiverSequenceNumber);
                Call call = new Call(phoneNumbersList[callerSequenceNumber], phoneNumbersList[receiverSequenceNumber]);
                List<Event> events = EventGeneration.GenerateFullCallEvents(call);
                call.Events = events;
                if (!CallerIsBusy(call.Caller, GetAllCallersBusyTimeIntervals(calls), GetCallerTimeInterval(call)) &&
                    !CallerIsBusy(call.Receiver.Value, GetAllCallersBusyTimeIntervals(calls), GetReceiverTimeInterval(call)))
                {
                    return call;
                }
            } while (true);
        }

        public static List<Call> GenerateCalls()
        {
            int nonDialledCallsNumber = (int)(TOTAL_CALLS_NUMBER * NON_DIALED_CALLS_PERCENTAGE);
            int cancelledCallsNumber = (int)(TOTAL_CALLS_NUMBER * CANCELLED_CALLS_PERCENTAGE);
            int fullCallsNumber = TOTAL_CALLS_NUMBER - (cancelledCallsNumber + nonDialledCallsNumber);

            List<int> phoneNumbersList = PhoneNumberGeneration.GeneratePhoneNumbers();

            List<Call> callsList = new List<Call>();

            for (int i = 0; i < nonDialledCallsNumber; i++)
            {
                callsList.Add(GenerateNonDialledCall(phoneNumbersList, callsList));
            }

            for (int i = 0; i < cancelledCallsNumber; i++)
            {
                callsList.Add(GenerateCancelledCall(phoneNumbersList, callsList));
            }

            for (int i = 0; i < fullCallsNumber; i++)
            {
                callsList.Add(GenerateFullCall(phoneNumbersList, callsList));
            }
            return callsList;
        }

        private struct TimeInverval
        {
            public DateTime Start { get; private set; }
            public DateTime End { get; private set; }

            public TimeInverval(DateTime start, DateTime end)
            {
                Start = start;
                End = end;
            }
        }

        private static TimeInverval GetCallerTimeInterval(Call call)
        {
            List<Event> events = call.Events.ToList();
            DateTime startCallerCall = events.FirstOrDefault(x => x.EventTypeID == EventType.EventPickUp.ID).Date;

            DateTime finishCallerCall = (events.Where(x => x.EventTypeID == EventType.EventHangUp.ID).Count() > 0)
                ? events.FirstOrDefault(x => x.EventTypeID == EventType.EventHangUp.ID).Date
                : events.FirstOrDefault(x => x.EventTypeID == EventType.EventCallEnd.ID).Date;

            return new TimeInverval(startCallerCall, finishCallerCall);
        }

        private static TimeInverval GetReceiverTimeInterval(Call call)
        {
            List<Event> events = call.Events.ToList();

            DateTime startReceiverCall = events.FirstOrDefault(x => x.EventTypeID == EventType.EventDial.ID).Date;
            DateTime finishReceiverCall = (events.Where(x => x.EventTypeID == EventType.EventHangUp.ID).Count() > 0)
                ? events.FirstOrDefault(x => x.EventTypeID == EventType.EventHangUp.ID).Date
                : events.FirstOrDefault(x => x.EventTypeID == EventType.EventCallEnd.ID).Date;

            return new TimeInverval(startReceiverCall, finishReceiverCall);
        }

        private static Dictionary<long, List<TimeInverval>> GetAllCallersBusyTimeIntervals(List<Call> calls)
        {
            Dictionary<long, List<TimeInverval>> callerIsBusyTimeIntervals = new Dictionary<long, List<TimeInverval>>();
            foreach (Call call in calls)
            {
                callerIsBusyTimeIntervals = GetBusyTimeIntervals(call, callerIsBusyTimeIntervals);
            }
            return callerIsBusyTimeIntervals;
        }

        private static bool CallerIsBusy(long phoneNumber, Dictionary<long, List<TimeInverval>> callerIsBusyTimeIntervals, TimeInverval callInterval)
        {
            if (callerIsBusyTimeIntervals.ContainsKey(phoneNumber))
            {
                List<TimeInverval> busyTimeIntervals = callerIsBusyTimeIntervals[phoneNumber];
                foreach (TimeInverval busyTimeInterval in busyTimeIntervals)
                {
                    if ((callInterval.Start >= busyTimeInterval.Start && callInterval.Start <= busyTimeInterval.End) ||
                            (callInterval.End >= busyTimeInterval.Start && callInterval.End <= busyTimeInterval.End))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static Dictionary<long, List<TimeInverval>> GetBusyTimeIntervals(Call call, Dictionary<long, List<TimeInverval>> callerIsBusyTimeIntervals)
        {
            long caller = call.Caller;
            long? receiver = call.Receiver;
            List<Event> events = call.Events.ToList();
            DateTime startCallerCall = events.FirstOrDefault(x => x.EventTypeID == EventType.EventPickUp.ID).Date;

            DateTime finishCallerCall = (events.Where(x => x.EventTypeID == EventType.EventHangUp.ID).Count() > 0)
                ? events.FirstOrDefault(x => x.EventTypeID == EventType.EventHangUp.ID).Date
                : events.FirstOrDefault(x => x.EventTypeID == EventType.EventCallEnd.ID).Date;

            if (callerIsBusyTimeIntervals.ContainsKey(caller))
            {
                callerIsBusyTimeIntervals[caller].Add(new TimeInverval(startCallerCall, finishCallerCall));
            }
            else
            {
                callerIsBusyTimeIntervals.Add(caller, new List<TimeInverval>());
                callerIsBusyTimeIntervals[caller].Add(new TimeInverval(startCallerCall, finishCallerCall));
            }

            if (receiver.HasValue)
            {
                DateTime startReceiverCall = events.FirstOrDefault(x => x.EventTypeID == EventType.EventDial.ID).Date;
                DateTime finishReceiverCall = (events.Where(x => x.EventTypeID == EventType.EventHangUp.ID).Count() > 0)
                    ? events.FirstOrDefault(x => x.EventTypeID == EventType.EventHangUp.ID).Date
                    : events.FirstOrDefault(x => x.EventTypeID == EventType.EventCallEnd.ID).Date;

                if (callerIsBusyTimeIntervals.ContainsKey(receiver.Value))
                {
                    callerIsBusyTimeIntervals[receiver.Value].Add(new TimeInverval(startReceiverCall, finishReceiverCall));
                }
                else
                {
                    callerIsBusyTimeIntervals.Add(receiver.Value, new List<TimeInverval>());
                    callerIsBusyTimeIntervals[receiver.Value].Add(new TimeInverval(startReceiverCall, finishReceiverCall));
                }
            }

            return callerIsBusyTimeIntervals;
        }
    }
}
