using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using UnityEngine.Serialization;

public class ShowKeyboard : MonoBehaviour
{
    [FormerlySerializedAs("TMPInputField")] public TMP_InputField tmpInputField;
    
    void Start()
    {
        tmpInputField.onSelect.AddListener(x => OpenKeyboard());
    }

    public void OpenKeyboard()
    {
        NonNativeKeyboard.Instance.InputField = tmpInputField;
        NonNativeKeyboard.Instance.PresentKeyboard(tmpInputField.text);
    }
}
