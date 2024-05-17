using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Z.Event
{
    public class EventsSystem 
    {
        public delegate void EventDelegate<T>(T param);
        public delegate void EventDelegate();

        private static Dictionary<string, Delegate> eventDictionary = new Dictionary<string, Delegate>();

        // 添加一个泛型方法用于订阅带参数的事件
        public static void StartListening<T>(string eventName, EventDelegate<T> listener)
        {
            Delegate thisEvent;
            if (eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                eventDictionary[eventName] = Delegate.Combine(thisEvent, listener);
            }
            else
            {
                eventDictionary.Add(eventName, listener);
            }
        }

        public static void StartListening(string eventName, EventDelegate listener)
        {
            Debug.Log($"StartListening :{eventName}");
            Delegate thisEvent;
            if (eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                eventDictionary[eventName] = Delegate.Combine(thisEvent, listener);
            }
            else
            {
                eventDictionary.Add(eventName, listener);
            }
        }

        // 添加一个泛型方法用于触发带参数的事件
        public static void TriggerEvent<T>(string eventName, T param)
        {
            Delegate thisEvent;
            if (eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                (thisEvent as EventDelegate<T>).Invoke(param);
            }
        }

        public static void TriggerEvent(string eventName)
        {
            Delegate thisEvent;
            if (eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                var ed = thisEvent as EventDelegate;
                ed.Invoke();
                //(thisEvent as EventDelegate).Invoke();
            }
        }


        public static void StopListening<T>(string eventName, EventDelegate<T> listener)
        {
            Delegate thisEvent;
            if (eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent = Delegate.Remove(thisEvent, listener);
                eventDictionary[eventName] = thisEvent;
            }
        }

        public static void StopListening(string eventName, EventDelegate listener)
        {

            Debug.Log($"StopListening :{eventName}");
            Delegate thisEvent;
            if (eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent = Delegate.Remove(thisEvent, listener);
                eventDictionary[eventName] = thisEvent;
            }
        }

    }
}
