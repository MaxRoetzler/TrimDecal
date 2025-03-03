using UnityEditor;
using UnityEngine;

namespace TrimDecal.Editor
{
    public abstract class HandleBase
    {
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

        public bool isActive
        {
            get => m_IsActive;
        }

        /////////////////////////////////////////////////////////////////

        public abstract bool CanEnter(Event e);
        public abstract void Preview(Event e);

        public void Enter(Event e)
        {
            m_IsActive = true;
            OnEnter(e);
        }

        public void Exit(Event e)
        {
            OnExit(e);
        }

        public void Perform(Event e)
        {
            if (e.type == EventType.MouseDown)
            {
                OnMouseDown(e);
                e.Use();
                return;
            }

            if (e.type == EventType.MouseUp)
            {
                OnMouseUp(e);
                OnExit(e);
                m_IsActive = false;
                e.Use();
                return;
            }

            if (e.type == EventType.MouseMove)
            {
                OnMouseMove(e);
                e.Use();
                return;
            }
        }

        /////////////////////////////////////////////////////////////////

        protected abstract void OnExit(Event e);
        protected abstract void OnEnter(Event e);
        protected virtual void OnMouseUp(Event e) { }
        protected virtual void OnMouseDown(Event e) { }
        protected virtual void OnMouseMove(Event e) { }
    }
}