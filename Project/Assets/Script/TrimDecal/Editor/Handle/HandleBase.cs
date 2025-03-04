using UnityEngine;
using System;

namespace TrimDecal.Editor
{
    public abstract class HandleBase : IHandle
    {
        protected const float k_DottedLineSpace = 2.0f;

        protected bool m_IsActive;
        protected HandleData m_Data;
        protected TrimSerializer m_Serializer;

        /////////////////////////////////////////////////////////////////

        public HandleBase(HandleData data, TrimSerializer serializer)
        {
            m_Data = data;
            m_Serializer = serializer;
        }

        /////////////////////////////////////////////////////////////////

        public event Action onCompleted;

        /////////////////////////////////////////////////////////////////

        public abstract bool CanEnter(Event e);
        public abstract void Preview(Event e);
        public abstract void Perform(Event e);
        public abstract void Start(Event e);

        /////////////////////////////////////////////////////////////////
        
        protected void NotifyHandleCompleted()
        {
            onCompleted();
        }
    }
}