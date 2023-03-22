using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DVAH;
using TMPro;

public class LanguageChild : MonoBehaviour
{
    [SerializeField] string _key;
    Text _text;
    
    TextMeshProUGUI _textMesh;

    private void Awake()
    {
        AssignText();
        StartCoroutine(WaitLanguageManager());
    }

    public LanguageChild SetKey(string key)
    {
        _key = key;
        this.changeLan(null);
        return this;
    }

    void AssignText()
    {
        if (_text != null)
        {
            return;
        }

        if (this.TryGetComponent<Text>(out _text))
        {
            return;
        }

        if (_textMesh != null)
            return;

        if (this.TryGetComponent<TextMeshProUGUI>(out _textMesh))
        {
            return;
        }
    }

    private void OnEnable()
    {
        this.changeLan(null);
    }

    void changeLan(object param)
    {
        AssignText();

        if (_text)
            _text.text = LanguageManager.Instant.Translator(_key);

        if (_textMesh)
            _textMesh.text = LanguageManager.Instant.Translator(_key);
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
        this.changeLan(null);
    }
}
