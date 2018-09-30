using System;
using System.Collections.Generic;

namespace TelephoneCallsWebApplication.Models.DataGeneration
{
    public class EventGeneration
    {
        private static readonly Random getRandom = new Random();

        private const int CALLS_GENERATION_TIME_INTERVAL_HOURS = 5;
        private static DateTime CALLS_GENERATION_START_TIME = DateTime.Today + TimeSpan.FromHours(0);
        private static TimeSpan CALLS_GENERATION_DELAY_TIME = TimeSpan.FromSeconds(2);
        private static TimeSpan CALLS_GENERATION_MAX_CALL_TIME = TimeSpan.FromHours(2);
        private static TimeSpan CALLS_GENERATION_MAX_CALL_ESTABLISHED_WAITING_TIME = TimeSpan.FromMinutes(1);

        public static List<Event> GenerateFullCallEvents(Call call)
        {
            List<Event> events = new List<Event>();

            Event pickUpEvent = GeneratePickUpEvent(call);
            events.Add(pickUpEvent);

            Event dialEvent = GenerateDialEvent(call, pickUpEvent.Date);
            events.Add(dialEvent);

            Event callEstablishedEvent = GenerateCallEstablishedEvent(call, dialEvent.Date);
            events.Add(callEstablishedEvent);

            Event callEndEvent = GenerateCallEndEvent(call, GenerateTime(callEstablishedEvent.Date, CALLS_GENERATION_MAX_CALL_TIME.Hours, 0, 0));
            events.Add(callEndEvent);

            Event hangUpEvent = GenerateHangUpEvent(call, callEndEvent.Date);
            events.Add(hangUpEvent);

            return events;
        }

        public static List<Event> GenerateCancelledCallEvents(Call call)
        {
            List<Event> events = new List<Event>();

            Event pickUpEvent = GeneratePickUpEvent(call);
            events.Add(pickUpEvent);

            Event dialEvent = GenerateDialEvent(call, pickUpEvent.Date);
            events.Add(dialEvent);


            Event callEndEvent = GenerateCallEndEvent(call, GenerateTime(dialEvent.Date, 0, CALLS_GENERATION_MAX_CALL_ESTABLISHED_WAITING_TIME.Minutes, 0));
            events.Add(callEndEvent);

            return events;
        }

        public static List<Event> GenerateNonDialledCallEvents(Call call)
        {
            List<Event> events = new List<Event>();

            Event pickUpEvent = GeneratePickUpEvent(call);
            events.Add(pickUpEvent);

            call.Receiver = null;
            Event hangUpEvent = GenerateHangUpEvent(call, pickUpEvent.Date);
            events.Add(hangUpEvent);

            return events;
        }

        private static Event GenerateCallEndEvent(Call call, DateTime timestamp)
        {
            return new Event(EventType.EventCallEnd, timestamp, call);
        }

        private static Event GenerateCallEstablishedEvent(Call call, DateTime previousEventTimestamp)
        {
            DateTime callEstablishedTime = GenerateTime(previousEventTimestamp, 0, CALLS_GENERATION_MAX_CALL_ESTABLISHED_WAITING_TIME.Minutes, 0);
            return new Event(EventType.EventCallEstablished, callEstablishedTime, call);
        }

        private static Event GenerateHangUpEvent(Call call, DateTime previousEventTimestamp)
        {
            return new Event(EventType.EventHangUp, previousEventTimestamp.Add(CALLS_GENERATION_DELAY_TIME), call);
        }

        private static Event GenerateDialEvent(Call call, DateTime previousEventTimestamp)
        {
            return new Event(EventType.EventDial, previousEventTimestamp.Add(CALLS_GENERATION_DELAY_TIME), call);
        }

        private static Event GeneratePickUpEvent(Call call)
        {
            DateTime startTime = GenerateTime(CALLS_GENERATION_START_TIME, CALLS_GENERATION_TIME_INTERVAL_HOURS, 0, 0);
            return new Event(EventType.EventPickUp, startTime, call);
        }

        private static DateTime GenerateTime(DateTime start, int hoursInterval, int minutesInterval, int secondsInterval)
        {
            int maxSeconds = (int)(TimeSpan.FromHours(hoursInterval).TotalSeconds + TimeSpan.FromMinutes(minutesInterval).TotalSeconds + TimeSpan.FromSeconds(secondsInterval).TotalSeconds);
            return start.Add(TimeSpan.FromSeconds(getRandom.Next(maxSeconds)));
        }
    }
}
