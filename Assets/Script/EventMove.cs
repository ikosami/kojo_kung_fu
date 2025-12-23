using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventMove : MonoBehaviour
{
    public static EventMove Instance;
    void Awake()
    {
        Instance = this;
    }


    public List<EventMoveData> eventMoves;
    public void Run(int index)
    {
        var eventData = eventMoves.Find(x => x.ID == index);
        if (eventData != null)
        {
            eventData.Action.Invoke();
        }
    }

    internal float GetPos(int id)
    {
        return eventMoves.Find(x => x.ID == id).Pos;
    }

    [Serializable]
    public class EventMoveData
    {
        public int ID;
        public float Pos;
        public UnityEvent Action;
    }
}
