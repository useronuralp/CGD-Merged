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
    public event Action<bool> OnToggleCursor;
    public event Action OnRagdolling;
    public event Action OnNotRagdolling;
    public event Action OnStartedGettingUp;
    public event Action OnStoppedGettingUp;
    public event Action OnStartingSpectating;
    public event Action OnStoppingSpectating;
    public event Action OnDroppingChatFocus;
    public event Action OnStopAllCoroutines;
    public event Action OnGameEnd;
    public event Action OnUpdateScores;
    public event Action<string> OnChangeEyes;
    public event Action OnTriggerEndGame;
    public event Action OnActivateDoubleJump;
    public event Action OnActivateWaterBaloon;
    public event Action OnGetStunned;
    public event Action OnStunWearsOff;
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
    public void ToggleCursor(bool forceUnlock = false)
    {
        OnToggleCursor?.Invoke(forceUnlock);
    }
    public void StartRagdolling()
    {
        OnRagdolling?.Invoke();
    }
    public void StopRagdolling()
    {
        OnNotRagdolling?.Invoke();
    }
    public void StartedGettingUp()
    {
        OnStartedGettingUp?.Invoke();
    }
    public void StoppedGettingUp()
    {
        OnStoppedGettingUp?.Invoke();
    }
    public void StartSpectating()
    {
        OnStartingSpectating?.Invoke();
    }
    public void StopSpectating()
    {
        OnStoppingSpectating?.Invoke();
    }
    public void DropChatFocus()
    {
        OnDroppingChatFocus?.Invoke();
    }
    public void StopAllCoroutines_InControls()
    {
        OnStopAllCoroutines?.Invoke();  
    }
    public void EndGame()
    {
        OnGameEnd?.Invoke();
    }
    public void UpdateScores()
    {
        OnUpdateScores?.Invoke();
    }
    public void ChangeEyes(string type)
    {
        OnChangeEyes?.Invoke(type);
    }
    public void TriggerEndGame()
    {
        OnTriggerEndGame?.Invoke();
    }
    public void ActivateDoubleJump()
    {
        OnActivateDoubleJump?.Invoke();
    }
    public void ActivateWaterBaloon()
    {
        OnActivateWaterBaloon?.Invoke();
    }
    public void GetStunned()
    {
        OnGetStunned?.Invoke();
    }
    public void StunWearsOff()
    {
        OnStunWearsOff?.Invoke();
    }
    //Singleton getter---------------------
    public static EventManager Get()
    {
        return s_Instance;
    }
}
