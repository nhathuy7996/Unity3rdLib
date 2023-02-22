using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DVAH;
using System;
using SimpleJSON;
using UnityEngine.UI;
using System.Linq;

namespace DVAH
{
    public class LanguageManager : Singleton<LanguageManager>
    {

        [SerializeField] LanguageName _currentLanguage = LanguageName.EN;
        [SerializeField] GameObject _languageBtnManager;
        [SerializeField] Sprite _selectedLan, _normalLan;

        public bool isLanguageSeted => PlayerPrefs.GetInt("LAN", -1) != -1;

        List<Button> _languageBtns = new List<Button>();
        Dictionary<string, Dictionary<string, string>> _languageDict = new Dictionary<string, Dictionary<string, string>>();

        List<Action<object>> _observerLanguage = new List<Action<object>>();

        // Start is called before the first frame update
        protected override void Awake()
        {
            base.Awake();
            var datasetLan = Resources.Load<TextAsset>("Languages/EN");
            if (datasetLan != null)
            {
                var datasetLanParse = JSON.Parse(datasetLan.text).AsObject;


                foreach (KeyValuePair<string, JSONNode> obj in datasetLanParse.Dict)
                {
                    var languageKey = new Dictionary<string, string>();
                    foreach (KeyValuePair<string, JSONNode> key in obj.Value.AsObject)
                    {
                        languageKey.Add(key.Key, key.Value);
                    }
                    _languageDict.Add(obj.Key, languageKey);
                }
            }

            _languageBtns = _languageBtnManager.GetComponentsInChildren<Button>().ToList();

            foreach (var btn in _languageBtns)
            {
                btn.onClick.AddListener(() =>
                {
                    int id = btn.transform.GetSiblingIndex();
                    ChangeLanguage((LanguageName)id);
                });
            }
        }

        private void Start()
        {
            if (isLanguageSeted)
            {
                ClosePopUp();

            }
            else
            {
                 
            }

            ChangeLanguage((LanguageName)PlayerPrefs.GetInt("LAN"));
        }

        public void ClosePopUp()
        {

            this.gameObject.SetActive(false);
            _ = AdManager.Instant.ShowBanner();
        }

        public string Translator(string key)
        {
            if (!_languageDict.ContainsKey(key))
                return key;

            if (!_languageDict[key].ContainsKey(_currentLanguage.ToString()))
                return key;

            return _languageDict[key][_currentLanguage.ToString()];

        }

        public void ChangeLanguage(LanguageName language = LanguageName.EN)
        {
            _currentLanguage = language;

            foreach (Action<object> a in _observerLanguage)
            {
                a?.Invoke(null);
            }

            PlayerPrefs.SetInt("LAN", (int)language);

            foreach (var btn in _languageBtns)
            {
                int id = btn.transform.GetSiblingIndex();
                if (id == (int)_currentLanguage)
                {
                    btn.image.sprite = _selectedLan;
                    btn.GetComponentInChildren<Text>().color = Color.white;
                }
                else
                {
                    btn.image.sprite = _normalLan;
                    btn.GetComponentInChildren<Text>().color = Color.black;
                }
            }

        }

        public void AddListener(Action<object> a)
        {
            _observerLanguage.Add(a);
        }

        public void RemoveListener(Action<object> a)
        {
            _observerLanguage.Remove(a);
        }
    }
}

public enum LanguageName
{
    EN = 0,
    PO = 1 ,
    HI = 2,
    SP = 3
}
