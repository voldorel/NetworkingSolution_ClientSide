using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnBasedFighter : MonoBehaviour
{
    public Animator Player1Animator;
    public Animator Player2Animator;


    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            StartCoroutine(ShowAttackAnimation(false));
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            StartCoroutine(ShowHitAnimation(false));
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(ShowAttackAnimation(true));
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            StartCoroutine(ShowHitAnimation(true));
        }
    }

    public IEnumerator ShowAttackAnimation(bool kill)
    {
        Player1Animator.SetTrigger("attack");
        yield return new WaitForSeconds(0.3f);
        if (kill)
        {
            Player2Animator.SetTrigger("die");
        }
        else 
            Player2Animator.SetTrigger("hit");
    }

    public IEnumerator ShowHitAnimation(bool kill)
    {
        Player2Animator.SetTrigger("attack");
        yield return new WaitForSeconds(0.3f);
        if (kill)
        {
            Player1Animator.SetTrigger("die");
        }
        else
            Player1Animator.SetTrigger("hit");
    }
}
