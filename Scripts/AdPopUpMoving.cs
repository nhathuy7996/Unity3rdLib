using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdPopUpMoving : MonoBehaviour
{

    [SerializeField] float _timeMoving = 3;

    Image _imageBG;
    Text _textNoAd;

    [SerializeField]
    float _speedFade = 0, _speedMove = 0;

    Vector2 _top;
    // Start is called before the first frame update
    void Awake()
    {
        _imageBG = this.GetComponentInChildren<Image>();
        _textNoAd = this.GetComponentInChildren<Text>();
        _top = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width/2, Screen.height));
        _speedFade = 1f / _timeMoving;
        _speedMove = Vector2.Distance(_top, this.transform.position) / _timeMoving;
    }

    private void OnEnable()
    {
        this.transform.localPosition = Vector3.zero;
        Color c = _imageBG.color;
        c.a = 1;
        _imageBG.color = c;

        c = _textNoAd.color;
        c.a = 1;
        _textNoAd.color = c;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Translate(new Vector3(0, _speedMove * Time.deltaTime, 0));
        Color c = _imageBG.color;
        c.a -= _speedFade * Time.deltaTime;
        _imageBG.color = c;
 

        c = _textNoAd.color;
        c.a -= _speedFade * Time.deltaTime;
       
        _textNoAd.color = c;

        if (c.a <= _speedFade * Time.deltaTime) this.gameObject.SetActive(false);
    }
}
