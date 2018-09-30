using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;


namespace TelephoneCallsWebApplication.Models
{
    [Table("T_CALL")]
    public class Call
    {
        [Column("RECORD_ID")]
        public int ID { get; set; }

        [Column("CALLER")]
        public int Caller { get; set; }

        [Column("RECEIVER")]
        [DisplayFormat(NullDisplayText = "No receiver")]
        public int? Receiver { get; set; }

        public ICollection<Event> Events { get; set; }

        public string TalkDuration
        {
            get
            {
                Event callEstablishedEvent = Events.FirstOrDefault(x => x.EventTypeID == EventType.EventCallEstablished.ID);
                if (callEstablishedEvent != null)
                {
                    DateTime startCallerCall = callEstablishedEvent.Date;
                    DateTime finishCallerCall = Events.FirstOrDefault(x => x.EventTypeID == EventType.EventCallEnd.ID).Date;
                    return Math.Round((finishCallerCall - startCallerCall).TotalMinutes, 2).ToString();
                }
                return "-";
            }
        }

        public string CallType
        {
            get
            {
                if (!Receiver.HasValue)
                {
                    return "Non-dialled call";
                }
                else if (Events.Where(x => x.EventTypeID == EventType.EventHangUp.ID).Count() > 0)
                {
                    return "Regular call";
                }
                else
                {
                    return "Cancelled call";
                }
            }
        }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime Timestamp
        {
            get
            {
                Event pickUpEvent = Events.Where(x => x.EventTypeID == EventType.EventPickUp.ID).FirstOrDefault();
                return pickUpEvent != null ? pickUpEvent.Date : DateTime.Today;
            }
        }

        public Call()
        {

        }

        public Call(int caller, int? receiver)
        {
            Caller = caller;
            Receiver = receiver;
            Events = new List<Event>();
        }    
    }
}
