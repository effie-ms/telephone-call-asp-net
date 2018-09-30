using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelephoneCallsWebApplication.Models
{
    [Table("T_EVENT")]
    public class Event
    {
        [Column("RECORD_ID")]
        public int ID { get; set; }

        [Column("CALL_ID")]
        public int CallID { get; set; }

        [Required]
        [Column("RECORD_EVENT_ID")]
        public string EventTypeID { get; set; }

        [Column("RECORD_DATE")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        public EventType EventType { get; set; } 
        
        public Call Call { get; set; }

        public Event()
        {

        }

        public Event(EventType type, DateTime timestamp, Call call)
        {
            EventType = type;
            Date = timestamp;
            Call = call;

            CallID = call.ID;
            EventTypeID = type.ID;
        }
    }
}
