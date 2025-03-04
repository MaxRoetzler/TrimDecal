using System;
using UnityEngine;

namespace TrimDecal.Editor
{
    public interface IHandle
    {
        public event Action onCompleted;

        public bool CanEnter(Event e);
        public void Preview(Event e);
        public void Perform(Event e);
        public void Start(Event e);
    }
}