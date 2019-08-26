using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum EVENT_TYPE
{
    ACCOUNT_READY,
    SOCKET_READY,
    WHITE_FINISH,
    BLACK_FINISH,
    PLAY,
    GAMEOVER,
    RESTART,
}

public interface IListener
{
    void OnEvent(EVENT_TYPE eventType, Component sender, object param = null);
}


public class GameManager : Singleton<GameManager>
{
    public string currentGameID;
    public float maxCooldown = 5f;

    public int wScore;
    public int bScore;

    public int[] bWeights;
    public int[] wWeights;

    private Dictionary<EVENT_TYPE, List<IListener>> listeners = new Dictionary<EVENT_TYPE, List<IListener>>();

    public void ResetGame()
    {
        wScore = 10;
        bScore = 10;
        for (int i = 0; i < bWeights.Length; i++)
        {
            bWeights[i] = 0;
            wWeights[i] = 0;
        }
        GameObject[] sheeps = GameObject.FindGameObjectsWithTag("Sheep");
        foreach (GameObject sheep in sheeps) Destroy(sheep);
    }

    public Vector3 LaneVelocity(int laneIndex)
    {
        Vector3 velocity = Vector3.zero;

        if (bWeights[laneIndex] > wWeights[laneIndex])
        {
            velocity = Vector3.down / 3f;
        }
        if (bWeights[laneIndex] < wWeights[laneIndex])
        {
            velocity = Vector3.up / 3f;
        }
        return velocity;
    }

    public int LaneDirection(int laneIndex)
    {
        return bWeights[laneIndex] > wWeights[laneIndex] ? -1 : bWeights[laneIndex] < wWeights[laneIndex] ? 1 : 0;
    }


    public void AddListener(EVENT_TYPE eventType, IListener listener)
    {
        List<IListener> listenerList = null;

        if (listeners.TryGetValue(eventType, out listenerList))
        {
            listenerList.Add(listener);
            return;
        }

        listenerList = new List<IListener>();
        listenerList.Add(listener);
        listeners.Add(eventType, listenerList);
    }

    public void PostNotification(EVENT_TYPE eventType, Component sender = null, object param = null)
    {
        List<IListener> listenerList = null;

        if (!listeners.TryGetValue(eventType, out listenerList))
            return;

        for (int i = 0; i < listenerList.Count; i++)
        {
            if (!listenerList[i].Equals(null))
                listenerList[i].OnEvent(eventType, sender, param);
        }
    }
    public void RemoveEvent(EVENT_TYPE eventType)
    {
        listeners.Remove(eventType);
    }

    public void RemoveRedundancies()
    {
        Dictionary<EVENT_TYPE, List<IListener>> tmpListeners = new Dictionary<EVENT_TYPE, List<IListener>>();

        foreach (KeyValuePair<EVENT_TYPE, List<IListener>> Item in listeners)
        {
            for (int i = Item.Value.Count - 1; i >= 0; i--)
            {
                if (Item.Value[i].Equals(null))
                    Item.Value.RemoveAt(i);
            }

            if (Item.Value.Count > 0)
                tmpListeners.Add(Item.Key, Item.Value);
        }

        listeners = tmpListeners;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RemoveRedundancies();
    }
}
