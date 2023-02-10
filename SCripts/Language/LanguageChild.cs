using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DVAH;

public class LanguageChild : MonoBehaviour
{
    [SerializeField] string _key;
    Text _text;


    private void Awake()
    {
        if (_text == null)
            _text = this.GetComponent<Text>();

        StartCoroutine(WaitLanguageManager());
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
        if(LanguageManager.Instant != null)
            LanguageManager.Instant.RemoveListener(changeLan);
    }

    IEnumerator WaitLanguageManager()
    {
        yield return new WaitUntil(()=> LanguageManager.Instant != null);
        LanguageManager.Instant.AddListener(changeLan);
    }
}
