using UnityEngine;
using System;

namespace TrimDecal.Editor
{
    public abstract class Handle : IHandle
    {
        protected const float k_DottedLineSpace = 2.0f;

        protected int m_ControlID;
        protected HandleData m_Data;
        protected TrimDecalSerializer m_Serializer;

        /////////////////////////////////////////////////////////////////

        public Handle(HandleData data, TrimDecalSerializer serializer)
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

        public virtual void Start(Event e)
        {
            m_ControlID = GUIUtility.GetControlID(FocusType.Passive);
            GUIUtility.hotControl = m_ControlID;

            m_Data.Setup();
        }

        /////////////////////////////////////////////////////////////////

        protected void NotifyHandleCompleted()
        {
            GUIUtility.hotControl = 0;
            onCompleted();
        }
    }
}