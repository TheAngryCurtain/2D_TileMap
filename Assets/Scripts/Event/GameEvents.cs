using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents
{
    public class RequestWorldGenEvent : VSGameEvent
    {
        public System.Action Callback;

        public RequestWorldGenEvent(System.Action callback)
        {
            Callback = callback;
        }
    }

    public class PlayerSpawnedEvent : VSGameEvent
    {
        public Transform PlayerTransform;

        public PlayerSpawnedEvent(Transform t)
        {
            PlayerTransform = t;
        }
    }

    public class PlayerPositionUpdateEvent : VSGameEvent
    {
        public Vector3 WorldPosition; // updated separately

        public PlayerPositionUpdateEvent()
        {
            
        }
    }
}
