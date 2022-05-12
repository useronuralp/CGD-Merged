using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheLongRoad : MonoBehaviour
{
    private Animator m_Animator;
    void Start()
    {
        m_Animator = GetComponent<Animator>();
        EventManager.Get().OnMoveUpLongRoad += MoveUp;
        EventManager.Get().OnMovedDownLongRoad += MoveDown;
    }
    public void MoveUp()
    {
        m_Animator.SetTrigger("MoveUp");
    }
    public void MoveDown()
    {
        m_Animator.SetTrigger("MoveDown");
    }
    public void Disable()
    {
        EventManager.Get().MoveUp_TreetopKingdom();
        gameObject.SetActive(false);
    }
}
