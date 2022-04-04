using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableInput : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        EventManager.Get().DisableInput();      
    }
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        EventManager.Get().EnableInput();
    }
}
