using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;
using UnityEngine.UI;

public class CypherQueryHandler : MonoBehaviour
{
    [SerializeField] private GameObject nodeVertexSwitch;
    [SerializeField] private GameObject labelDropDown;
    [SerializeField] private GameObject attributeDropdown;
    [SerializeField] private GameObject operatorDropdown;
    [SerializeField] private GameObject valueText;
    [SerializeField] private GameObject andOr;
    [SerializeField] private GameObject verticalObj;
    private List<GameObject> _elementsToCopy;
    private ContentSizeFitter _csf;
    private bool _fFflag;

    void Start()
    {
        _elementsToCopy = new List<GameObject>()
        {
            nodeVertexSwitch,
            labelDropDown,
            attributeDropdown,
            operatorDropdown,
            valueText,
            andOr
        };
        CreateQueryRow();
    }


    public void CreateQueryRow()
    {
        GameObject newQueryRow = new GameObject("QueryRow");
        newQueryRow.transform.SetParent(verticalObj.transform, true);
        bool flg = false;

        HorizontalLayoutGroup hlg = newQueryRow.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth = false; 
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        foreach (GameObject element in _elementsToCopy)
        {
            GameObject newElement = Instantiate(element, newQueryRow.transform);
            newElement.SetActive(true);
            
            if (_fFflag && !flg)
            {
                newElement.SetChildrenActive(false);
            }
            
            flg = true;
            _fFflag = true;
        }
    }
}