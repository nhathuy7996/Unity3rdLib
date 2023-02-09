using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DVAH;

public class LanguageChild : MonoBehaviour
{
    [SerializeField] string _key;
    Text _text;


    private void Start()
    {
        if (_text == null)
            _text = this.GetComponent<Text>();

        LanguageManager.Instant.AddListener(changeLan);
    }

    private void OnEnable()
    {
        this.changeLan(null);
    }

    void changeLan(object param)
    {
        if (_text == null)
            return;
        
        _text.text = LanguageManager.Instant.Translator(_key);
    }

    private void OnDestroy()
    {
        LanguageManager.Instant.RemoveListener(changeLan);
    }
}
