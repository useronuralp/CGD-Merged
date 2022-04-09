using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;
public class EventManager : MonoBehaviourPunCallbacks
{
    private static EventManager s_Instance;
    public event Action<Game.SenderType> OnDisableInput;
    public event Action OnEnableInput;
    public event Action OnSyncObstacles;
    public event Action OnToggleCursor;
    public event Action OnRagdolling;
    public event Action OnNotRagdolling;
    private void Awake()
    {
        s_Instance = this;
    }
    public void DisableInput(Game.SenderType type)
    {
        OnDisableInput?.Invoke(type);
    }
    public void EnableInput()
    {
        OnEnableInput?.Invoke();
    }
    public void SyncObstacles()
    {
        OnSyncObstacles?.Invoke();
    }
    public void ToggleCursor()
    {
        OnToggleCursor?.Invoke();
    }
    public void StartRagdolling()
    {
        OnRagdolling?.Invoke();
    }
    public void StopRagdolling()
    {
        OnNotRagdolling?.Invoke();
    }
    //Singleton getter---------------------
    public static EventManager Get()
    {
        return s_Instance;
    }
}
