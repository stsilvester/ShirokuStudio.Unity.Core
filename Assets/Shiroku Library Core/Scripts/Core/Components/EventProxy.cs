using System;
using UltEvents;
using UnityEngine;

namespace ShirokuStudio.Core.Components
{
    [AddComponentMenu("Common/事件代理 EventProxy")]
    public class EventProxy : MonoBehaviour
    {
        [SerializeField]
        private EventMap events;

        [SerializeField]
        private bool debug = false;

        public void InvokeEvent(string eventName)
        {
            if (debug)
                UnityEngine.Debug.Log($"InvokeEvent: {eventName}");

            if (events.TryGetValue(eventName, out var value))
            {
                try
                {
                    value.Invoke();
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError(ex);
                }
            }
        }

        [Serializable]
        public class EventMap : SerializableDictionary<string, UltEvent>
        { }
    }
}