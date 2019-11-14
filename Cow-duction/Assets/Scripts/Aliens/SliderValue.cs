using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderValue : MonoBehaviour
{
    public Slider _Slider;
    public Text _Text;
    // Start is called before the first frame update
    void Start()
    {
        if(PlayerPrefs.HasKey(_Slider.name))
        {
            _Slider.value = PlayerPrefs.GetFloat(_Slider.name);

            if (_Slider.wholeNumbers)
                _Text.text = _Slider.value.ToString();
            else
                _Text.text = _Slider.value.ToString("F2");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
