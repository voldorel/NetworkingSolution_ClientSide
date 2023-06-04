using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using MyNetwork;

public class TurnBasedFighter : GameSession
{
    




    public Animator Player1Animator;
    public Animator Player2Animator;
    public Transform LeftDamageTextParent;
    public Transform RightDamageTextParent;
    public DamageNumberText DamageNumberTextPrefab;


    public Text TimerText;
    public Image TimerImage;
    private float _gameTime;


    public Button Button1;
    public Button Button2;
    public Button Button3;







    private void Start()
    {
        base.InitGameSession();
        base.OnPlayerEntered += TurnBasedFighter_OnPlayerEntered;
        base.OnPlayerLeft += TurnBasedFighter_OnPlayerLeft;



        base.OnMatchStart += response =>
        {
            Debug.Log("session start called " + response);
            _gameTime = 0f;
        };


        //_gameSession.OnNetworkFunctionCall

    }

    private void TurnBasedFighter_OnPlayerLeft(string userId)
    {
        Debug.Log(userId);
    }

    private void TurnBasedFighter_OnPlayerEntered(string userId)
    {
        Debug.Log(userId);
    }



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



    protected override void NetworkUpdate()
    {
        base.NetworkUpdate();
        //Debug.Log("well nice");

        if (_gameTime <= 0f)
        {
            _gameTime = 120f;
        }
        _gameTime -= (float)base.TickrateFixedTime;
        TimerText.text = ( (int) _gameTime).ToString();
        TimerImage.fillAmount = (float) _gameTime / 120f;
    }

    public IEnumerator ShowAttackAnimation(bool kill)
    {
        Player1Animator.SetTrigger("attack");
        yield return new WaitForSeconds(0.4f);
        if (kill)
        {
            Player2Animator.SetTrigger("die");
        }
        else
        {
            Player2Animator.SetTrigger("hit");
            ShowDamageNumber(-13, false);
        }
    }






    public IEnumerator ShowHitAnimation(bool kill)
    {
        Player2Animator.SetTrigger("attack");
        yield return new WaitForSeconds(0.4f);
        if (kill)
        {
            Player1Animator.SetTrigger("die");
        }
        else
        {
            Player1Animator.SetTrigger("hit");
            ShowDamageNumber(-13, true);
        }
    }

    public void EnableAbilityButtons(bool value)
    {
        Button1.interactable = value;
        Button2.interactable = value;
        Button3.interactable = value;
    }

    public void ShowDamageNumber(int value, bool spawnAtLeft)
    {
        var damageNumber = Instantiate(DamageNumberTextPrefab, spawnAtLeft ? LeftDamageTextParent : RightDamageTextParent) as DamageNumberText;
        if (value < 0)
        {
            damageNumber.SetDamageNumber("" + value);
        } else
        {
            damageNumber.SetDamageNumber("+" + value);
        }
    }


    public void OnClickAttack1()
    {
        

        NetCall("TestMethod1", 1, 2.3f);
    }

    public void OnClickAttack2()
    {
        EnableAbilityButtons(false);
    }

    public void OnClickAttack3()
    {
        EnableAbilityButtons(false);
    }

    public IEnumerator ResetButtons()
    {
        EnableAbilityButtons(false);
        yield return new WaitForSeconds(2);
        EnableAbilityButtons(true);
    }



    private void TestMethod1(int value, float x)
    {
        EnableAbilityButtons(false);
        StartCoroutine(ShowAttackAnimation(false));
        Debug.Log(value + " # " + x);
    }
}
