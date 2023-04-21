using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextInputFarsi : MonoBehaviour
{
    [SerializeField]
    private Text _text;
    [SerializeField]
    private Text _farsiUiText;

    public void OnTextChange()
    {
        _farsiUiText.text = _text.text;
    }

    public string GetText()
    {
        return _text.text;
    }
}
