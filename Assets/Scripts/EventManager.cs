using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EventManager : MonoBehaviour
{
    private static EventManager s_Instance;
    public event Action OnDisableInput;
    public event Action OnEnableInput;
    public event Action OnSyncObstacles;
    private void Awake()
    {
        s_Instance = this;
    }
    public void DisableInput()
    {
        OnDisableInput?.Invoke();
    }
    public void EnableInput()
    {
        OnEnableInput?.Invoke();
    }
    public void SyncObstacles()
    {
        OnSyncObstacles?.Invoke();
    }
    //Singleton getter---------------------
    public static EventManager Get()
    {
        return s_Instance;
    }
}
