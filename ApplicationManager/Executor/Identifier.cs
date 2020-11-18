using System;

namespace ApplicationManager.Executor
{
    internal class Identifier : ApplicationManager.Identifier.Manager
    {
        protected override string Context => "Executor";
        internal static Event Events = new Event();

        internal ApplicationManager.Identifier.Models.Identity GetNext(Func<Event, string> type)
        {
            return base.GetNext(type(Events));
        }

        internal class Event
        {
            internal string ExecuteHookBefore { get; } = "ExecuteHookBefore";
            internal string ExecuteHookAfter { get; } = "ExecuteHookAfter";
            internal string ExecuteMain { get; } = "ExecuteMain";
            internal string Terminate { get; } = "Terminate";
        }
    }
}
