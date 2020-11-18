using System;

namespace ApplicationManager.Identifier.Models
{
    /// <summary>
    /// Derived with <see cref="Identity"/> because of event handler in structure 
    /// unsubscribed yourself after call
    /// </summary>
    internal class Eventer
    {
        internal event EventHandler<Identity> NewChildren;


        internal void NotifyNewChildren(object sender, Identity id)
        {
            NewChildren?.Invoke(sender, id);
        }
    }
}
