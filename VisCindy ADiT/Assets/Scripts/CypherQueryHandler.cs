using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.MixedReality.Toolkit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.HID;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;


public class CypherQueryHandler : MonoBehaviour
{
    [SerializeField] private GameObject nodeVertexSwitch;
    [SerializeField] private GameObject labelDropDown;
    [SerializeField] private GameObject attributeDropdown;
    [SerializeField] private GameObject operatorDropdown;
    [SerializeField] private GameObject valueText;
    [SerializeField] private GameObject and;
    [SerializeField] private GameObject or;
    [SerializeField] private GameObject verticalObj;
    [SerializeField] private TMP_Text queryTextField;
    [SerializeField] private TMP_Dropdown getGraphDropdown;
    [SerializeField] private TMP_Dropdown nodeLabelDropdown;
    [SerializeField] private TMP_Dropdown nodePropertiesDropdown;
    public string apiUrl = "http://127.0.0.1:5000/api/";
    private List<GameObject> _elementsToCopy;
    private bool _fFflag;
    private int _rowCounter;
    private string _queryPriority;
    private string _finalQuery;
    private string _responseData1;
    private static readonly CookieContainer CookieContainer = new CookieContainer();
    private HttpClientHandler _handler = new HttpClientHandler
    {
        CookieContainer = CookieContainer,
        UseCookies = true
    };

    private JObject _jsonDictionary;

    void Start()
    {
        _elementsToCopy = new List<GameObject>()
        {
            nodeVertexSwitch,
            labelDropDown,
            attributeDropdown,
            operatorDropdown,
            valueText,
            and,
            or
        };
        
        _rowCounter = 0;
        _queryPriority = "p0";
        StartCoroutine(GetGraphProperties());
    }

    public void UpdateQueryText()
    {
        queryTextField.SetText(_queryPriority);
    }
    
    public void CreateDictionary()
    {
        _jsonDictionary = JsonConvert.DeserializeObject<JObject>(_responseData1);
    }
    
    public void UpdateDropdown()
    {
        nodeLabelDropdown.ClearOptions();
        nodePropertiesDropdown.ClearOptions();
        nodeLabelDropdown.AddOptions(_jsonDictionary["node_labels"]?.ToObject<List<string>>());
        nodePropertiesDropdown.AddOptions(_jsonDictionary["node_properties"]?.ToObject<List<string>>());
    }

    public void FinaliseQuery()
    {
        _finalQuery = "";
        int pointer = 0;
        foreach (char c in _queryPriority)
        {
            if (c == '(')
            {
                _finalQuery += '(';
            }
            else if (c == ')')
            {
                _finalQuery += ')';
            }
            else if (c == ' ')
            {
                _finalQuery += ' ';
            }
            else if (c == 'p')
            {
                string pNum = "";
                while (pointer + 1 < _queryPriority.Length && char.IsDigit(_queryPriority[pointer + 1]))
                {
                    pNum += _queryPriority[pointer + 1];
                    pointer++;
                }

                _finalQuery += "data from p" + pNum;
            }
            else if (c == '&')
            {
                if (_queryPriority[pointer - 1] == c)
                {
                    _finalQuery += "AND";
                }
            }
            else if (c == '|')
            {
                if (_queryPriority[pointer - 1] == c)
                {
                    _finalQuery += "OR";
                }
            }

            if (!char.IsDigit(c))
            {
                pointer++;
            }
        }
        
        queryTextField.text = _finalQuery;
    }


    public void CreateQueryRow() // 0 = nic, 1 &&, 2 ||
    {
        GameObject newQueryRow = new GameObject("QueryRow" + _rowCounter);
        
        newQueryRow.transform.SetParent(verticalObj.transform, true);
        bool flg = false;

        HorizontalLayoutGroup hlg = newQueryRow.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth = false; 
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.transform.localScale = new Vector3(1, 1, 1);
        hlg.transform.position = new Vector3(hlg.transform.position.x, hlg.transform.position.y, (float)0.2);

        foreach (GameObject element in _elementsToCopy)
        {
            GameObject newElement = Instantiate(element, newQueryRow.transform);
            newElement.SetActive(true);
            if (newElement.name is "AND(Clone)" or "OR(Clone)")
            {
                
                if(newElement.name == "AND(Clone)")
                {
                    Button buttonComponent = newElement.GetComponent<Button>();

                    // Add Event Trigger component
                    EventTrigger trigger = newElement.AddComponent<EventTrigger>();
                    EventTrigger.Entry entry = new EventTrigger.Entry();
                    entry.eventID = EventTriggerType.PointerClick;

                    // Capture the button ID (assuming you have a way to assign it)
                    int buttonID = _rowCounter; // Or get it from the button itself

                    // Add listener with a lambda expression to capture the button ID
                    entry.callback.AddListener((data) =>
                    {
                        // Call a new function to handle the button click with ID
                        OnButtonClickedAND(buttonID);
                        CreateQueryRow();
                    });

                    trigger.triggers.Add(entry);
                }
                if (newElement.name == "OR(Clone)")
                {
                    Button buttonComponent = newElement.GetComponent<Button>();

                    // Add Event Trigger component
                    EventTrigger trigger = newElement.AddComponent<EventTrigger>();
                    EventTrigger.Entry entry = new EventTrigger.Entry();
                    entry.eventID = EventTriggerType.PointerClick;

                    // Capture the button ID (assuming you have a way to assign it)
                    int buttonID = _rowCounter; // Or get it from the button itself

                    // Add listener with a lambda expression to capture the button ID
                    entry.callback.AddListener((data) =>
                    {
                        // Call a new function to handle the button click with ID
                        OnButtonClickedOR(buttonID);
                        CreateQueryRow();
                    });
                    trigger.triggers.Add(entry);
                }
                
            }

            if (_fFflag && !flg)
            {
                newElement.SetChildrenActive(false);
            }
            flg = true;
            _fFflag = true;
        }
        _rowCounter++;
    }

    public void OnButtonClickedAND(int buttonID) //and
    {
        int index = _queryPriority.IndexOf("p" + buttonID, StringComparison.Ordinal);

        if (index != -1)
        {
            _queryPriority = _queryPriority.Insert(index, "(");
            if ((_rowCounter-1)  == buttonID)
            {
                _queryPriority = _queryPriority.Insert(index + 3, " && p" + _rowCounter);
            }
            else
            {
                _queryPriority = _queryPriority.Insert(index + 4, "&& p" + _rowCounter + ") ");
            }
        }

        _queryPriority += ' ';
        // Debug.Log(_queryPriority + "rwc: " + _rowCounter + "btID: " + buttonID);
        for (int i = 0; i < ParenthesesDifference(_queryPriority); i++)
        {
            _queryPriority += ')';
        }
    }
    
    public void OnButtonClickedOR(int buttonID) //and
    {
        int index = _queryPriority.IndexOf("p" + buttonID, StringComparison.Ordinal);

        if (index != -1)
        {
            _queryPriority = _queryPriority.Insert(index, "(");
            //zasa mi jebe a robim to iste
            if ((_rowCounter - 1) == buttonID)
            {
                _queryPriority = _queryPriority.Insert(index + 3, " || p" + _rowCounter);
            }
            else
            {
                _queryPriority = _queryPriority.Insert(index + 4, "|| p" + _rowCounter + ") ");
            }
        }

        _queryPriority += ' ';
        // Debug.Log(_queryPriority + "rwc: " + _rowCounter + "btID: " + buttonID);
        for (int i = 0; i < ParenthesesDifference(_queryPriority); i++)
        {
            _queryPriority += ')';
        }
    }
    
    int ParenthesesDifference(string inputString)
    {
        int count1 = 0;
        int count2 = 0;
        foreach (char c in inputString)
        {
            if (c == '(')
            {
                count1++;
            }

            if (c == ')')
            {
                count2++;
            }
        }
        return count1-count2;
    }

    public void DeleteQuery()
    {
        for (int i = verticalObj.transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = verticalObj.transform.GetChild(i).gameObject;
            if (child.name != "Query Text Field")
            {
                Destroy(child);
            }
        }
        StartCoroutine(GetGraphProperties());
        queryTextField.text = "Cypher query";
    }
    
    private IEnumerator GetGraphProperties()
    {
        using (HttpClient client = new HttpClient(new HttpClientHandler
               {
                   CookieContainer = CookieContainer,
                   UseCookies = true
               }))
        {
            var response1 = client.GetAsync(apiUrl + "properties/" + 
                                            getGraphDropdown.options[getGraphDropdown.value].text);
            yield return response1;

            if (response1.Result.IsSuccessStatusCode)
            {
                _responseData1 = response1.Result.Content.ReadAsStringAsync().Result;
                yield return _responseData1;
                Debug.Log(_responseData1);
                CreateDictionary();
                UpdateDropdown();
                CreateQueryRow();
            }
            else
            {
                Console.WriteLine($"Error: {response1.Result.StatusCode} - {response1.Result.ReasonPhrase}");
            }
        }
    }
}