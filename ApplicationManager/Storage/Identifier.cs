using System;

namespace ApplicationManager.Storage
{
    internal class Identifier : ApplicationManager.Identifier.Manager
    {
        protected override string Context => "Storage";
        internal static Event Events = new Event();

        internal ApplicationManager.Identifier.Models.Identity GetNext(Func<Event, string> type)
        {
            return base.GetNext(type(Events));
        }

        internal class Event
        {
            internal string Install { get; } = "Install";
        }
    }
}
