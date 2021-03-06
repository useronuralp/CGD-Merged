using System;
using Photon.Pun;
/// <summary>
/// This class is contains all the events that can happen. It is a static class and can be accessed from anywhere.
/// It is used to decrease coupling and increase modularity of the game.
/// </summary>
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
    public event Action OnActivateForcefield;
    public event Action OnGetStunned;
    public event Action OnStunWearsOff;
    public event Action OnDeactivateWaterballoon;
    public event Action OnDeactivateDoubleJump;
    public event Action OnMoveUpLongRoad;
    public event Action OnMovedDownLongRoad;
    public event Action OnMoveUpTreetopKingdom;
    public event Action OnMoveDownTreetopKingdom;
    public event Action OnMakeHatTransparent;
    public event Action OnMakeHatOpaque;
    public event Action<int> OnChangeTrack;
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
    public void ActivateForcefield()
    {
        OnActivateForcefield?.Invoke();
    }
    public void GetStunned()
    {
        OnGetStunned?.Invoke();
    }
    public void StunWearsOff()
    {
        OnStunWearsOff?.Invoke();
    }
    public void DeactivateWaterballoon()
    {
        OnDeactivateWaterballoon?.Invoke();
    }
    public void DeactivateDoubleJump()
    {
        OnDeactivateDoubleJump?.Invoke();
    }
    public void MoveUp_LongRoad()
    {
        OnMoveUpLongRoad?.Invoke();
    }
    public void MoveDown_LongRoad()
    {
        OnMovedDownLongRoad?.Invoke();
    }
    public void MoveUp_TreetopKingdom()
    {
        OnMoveUpTreetopKingdom?.Invoke();
    }
    public void MoveDown_TreetopKingdom()
    {
        OnMoveDownTreetopKingdom?.Invoke();
    }
    public void MakeHatTransparent()
    {
        OnMakeHatTransparent?.Invoke();
    }
    public void MakeHatOpaque()
    {
        OnMakeHatOpaque?.Invoke();
    }
    public void ChangeTrack(int trackID)
    {
        OnChangeTrack?.Invoke(trackID);
    }
    //Singleton getter---------------------
    public static EventManager Get()
    {
        return s_Instance;
    }
}
