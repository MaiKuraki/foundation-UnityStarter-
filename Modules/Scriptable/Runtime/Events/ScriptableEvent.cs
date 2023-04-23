﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Pancake.Attribute;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.Scriptable
{
    [Serializable]
    [EditorIcon("scriptable_event")]
    public abstract class ScriptableEvent<T> : ScriptableEventBase, IDrawObjectsInInspector
    {
        [SerializeField] private bool debugLogEnabled;
        [SerializeField] protected T debugValue = default;

        private readonly List<EventListenerGeneric<T>> _eventListeners = new List<EventListenerGeneric<T>>();
        private readonly List<Object> _listenerObjects = new List<Object>();
        private Action<T> _onRaised;

        public event Action<T> OnRaised
        {
            add
            {
                _onRaised += value;
                var listener = value.Target as Object;
                if (listener != null && !_listenerObjects.Contains(listener)) _listenerObjects.Add(listener);
            }
            remove
            {
                _onRaised -= value;

                var listener = value.Target as Object;
                if (listener != null && _listenerObjects.Contains(listener)) _listenerObjects.Remove(listener);
            }
        }

        public void Raise(T param)
        {
            if (!Application.isPlaying) return;

            for (int i = _eventListeners.Count - 1; i >= 0; i--)
            {
                _eventListeners[i].OnEventRaised(this, param, debugLogEnabled);
            }

            _onRaised?.Invoke(param);

            // As this uses reflection, I only allow it to be called in Editor. So you need remember turnoff debug when build
            if (debugLogEnabled) Debug();
        }

        public void RegisterListener(EventListenerGeneric<T> listener)
        {
            if (!_eventListeners.Contains(listener)) _eventListeners.Add(listener);
        }

        public void UnregisterListener(EventListenerGeneric<T> listener)
        {
            if (_eventListeners.Contains(listener)) _eventListeners.Remove(listener);
        }

        public List<Object> GetAllObjects()
        {
            var allObjects = new List<Object>(_eventListeners);
            allObjects.AddRange(_listenerObjects);
            return allObjects;
        }

        private void Debug()
        {
            if (_onRaised == null) return;
            var delegates = _onRaised.GetInvocationList();
            foreach (var del in delegates)
            {
                var sb = new StringBuilder();
                sb.Append("<color=#52D5F2>[Event] </color>");
                sb.Append(name);
                sb.Append(" => ");
                sb.Append(del.GetMethodInfo().Name);
                sb.Append("()");
                var monoBehaviour = del.Target as MonoBehaviour;
                UnityEngine.Debug.Log(sb.ToString(), monoBehaviour?.gameObject);
            }
        }

        public override void Reset()
        {
            debugLogEnabled = false;
            debugValue = default;
        }
    }
}