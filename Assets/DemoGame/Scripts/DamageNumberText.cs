using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageNumberText : MonoBehaviour
{
    [SerializeField] private Text _damageNumberText;

    private void Start()
    {
        StartCoroutine(DestroySelf());
    }
    public void SetDamageNumber(string valueText)
    {
        _damageNumberText.text = valueText;
    }


    private IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(2);
        Destroy(gameObject);
    }
}
