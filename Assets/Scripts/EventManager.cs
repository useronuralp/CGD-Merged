using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EventManager : MonoBehaviour
{
    private static EventManager s_Instance;
    public event Action OnDisableInput;
    public event Action OnEnableInput;
    public event Action<float> OnReduceStamina;
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
    public void ReduceStamina(float amount)
    {
        OnReduceStamina?.Invoke(amount);
    }
    //Singleton getter---------------------
    public static EventManager Get()
    {
        return s_Instance;
    }
}
