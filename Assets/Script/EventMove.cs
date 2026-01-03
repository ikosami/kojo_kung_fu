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
    public void Run(int id)
    {
        EventMoveData eventData = GetData(id);
        if (eventData != null)
        {
            eventData.Action.Invoke();
        }
    }

    internal float GetPos(int id)
    {
        EventMoveData eventData = GetData(id);
        return eventData != null ? eventData.Pos : 0f;
    }
    public EventMoveData GetData(int index)
    {
        return eventMoves.Find(x => x.ID == index);
    }

    [Serializable]
    public class EventMoveData
    {
        public int ID;
        public float Pos;
        public UnityEvent Action;
    }
}
