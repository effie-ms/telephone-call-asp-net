using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TelephoneCallsWebApplication.Models
{
    [Table("T_EVENT_TYPE")]
    public class EventType
    {
        public static readonly EventType EventPickUp = new EventType("EVENT_PICK_UP", "Pick-up");
        public static readonly EventType EventDial = new EventType("EVENT_DIAL", "Dialling");
        public static readonly EventType EventCallEstablished = new EventType("EVENT_CALL_ESTABLISHED", "Call Established");
        public static readonly EventType EventCallEnd = new EventType("EVENT_CALL_END", "Call End");
        public static readonly EventType EventHangUp = new EventType("EVENT_HANG_UP", "Hang-up");

        [Column("EVENT_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ID { get; set; }

        [Required]
        [Column("EVENT_NAME")]
        public string EventName { get; private set; }

        public EventType()
        {

        }

        public EventType(string id, string name)
        {
            ID = id;
            EventName = name;
        }

        public static string GetEventTypeById(string id)
        {
            switch (id)
            {
                case "EVENT_PICK_UP":
                    return EventPickUp.EventName;
                case "EVENT_DIAL":
                    return EventDial.EventName;
                case "EVENT_CALL_ESTABLISHED":
                    return EventCallEstablished.EventName;
                case "EVENT_CALL_END":
                    return EventCallEnd.EventName;
                case "EVENT_HANG_UP":
                    return EventHangUp.EventName;
                default:
                    return string.Empty;
            }
        }
    }
}
