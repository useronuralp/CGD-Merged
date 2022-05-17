using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreetopKingdom : MonoBehaviour
{
    private Animator m_Animator;
    void Start()
    {
        m_Animator = GetComponent<Animator>();
        EventManager.Get().OnMoveUpTreetopKingdom += MoveUp;
        EventManager.Get().OnMoveDownTreetopKingdom += MoveDown;
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
        EventManager.Get().MoveUp_LongRoad();
        gameObject.SetActive(false);
    }
}
