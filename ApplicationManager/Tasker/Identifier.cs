using System;

namespace ApplicationManager.Tasker
{
    internal class Identifier : ApplicationManager.Identifier.Manager
    {
        protected override string Context => "Tasker";
        internal static Event Events = new Event();

        internal ApplicationManager.Identifier.Models.Identity GetNext(Func<Event, string> type)
        {
            return base.GetNext(type(Events));
        }

        internal class Event
        {
            internal string Start { get; } = "Start";
            internal string Stop { get; } = "Stop";
        }
    }
}
