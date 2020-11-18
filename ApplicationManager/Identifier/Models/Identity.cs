using System;
using System.Collections.Generic;

namespace ApplicationManager.Identifier.Models
{
    public struct Identity
    {
        public Guid Number { get; }
        public string Context { get; }
        public string Type { get; }
        public DateTime CreatedAt { get; }
        public IReadOnlySet<Identity> Childs => _childs;

        private Eventer _eventer;
        public event EventHandler<Identity> NewChildren
        {
            add
            {
                _eventer.NewChildren += value;
                foreach (var child in _childs)
                {
                    value(this, child);
                }
            }
            remove
            {
                _eventer.NewChildren -= value;
            }
        }

        private HashSet<Identity> _childs;

        internal Identity(string context, string type)
        {
            Number = Guid.NewGuid();
            Context = context;
            Type = type;
            CreatedAt = DateTime.Now;
            _childs = new HashSet<Identity>();
            _eventer = new Eventer();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        internal void AddChild(Identity id)
        {
            _childs.Add(id);
            _eventer.NotifyNewChildren(this, id);
        }


        public static bool operator ==(Identity id1, Identity id2)
        {
            return id1.Number == id2.Number;
        }


        public static bool operator !=(Identity id1, Identity id2)
        {
            return !(id1 == id2);
        }
    }
}
