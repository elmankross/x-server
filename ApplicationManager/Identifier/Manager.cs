using ApplicationManager.Identifier.Models;

namespace ApplicationManager.Identifier
{
    abstract internal class Manager
    {
        protected abstract string Context { get; }


        protected Identity GetNext(string type)
        {
            return new Identity(Context, type);
        }
    }
}
