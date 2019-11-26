/*  ParameterSlider.cs

    Handler for the parameter defined by the slider name.
    Used primarily by GameManager.
*/

using UnityEngine;
using UnityEngine.UI;

public class ParameterSlider : MonoBehaviour
{
    // Private variables
    private Slider m_Slider;
    private InputField valueText;
    private float defaultValue = 0.0f;

    // Awake is called after all objects are initialized
    void Awake()
    {
        m_Slider = GetComponent<Slider>();
        valueText = GetComponentInChildren<InputField>();
    }

    // Set the default value used if a PlayerPref key doesn't exist for the parameter
    public void SetDefaultValue(float _value)
    {
        defaultValue = _value;
    }

    // Set integer slider value
    public void SetIntValue(int _value)
    {
        m_Slider.value = _value;
        UpdateTextByValue();
    }

    // Set float slider value
    public void SetFloatValue(float _value)
    {
        m_Slider.value = _value;
        UpdateTextByValue();
    }

    // Reset parameter to default value
    public void ResetValue()
    {
        if (m_Slider.wholeNumbers)
        {
            SetIntValue((int)defaultValue);
        }
        else
        {
            SetFloatValue(defaultValue);
        }
        UpdateTextByValue();
        SaveValue();
    }

    // Save value in PlayerPrefs
    public void SaveValue()
    {
        if (m_Slider.wholeNumbers)
        {
            PlayerPrefs.SetInt(m_Slider.name, (int)m_Slider.value);
        }
        else
        {
            PlayerPrefs.SetFloat(m_Slider.name, m_Slider.value);
        }
        PlayerPrefs.Save();
    }

    // Load value from PlayerPrefs or use default value
    public void LoadValue()
    {
        // Error check
        if (!m_Slider)
            return;

        if (m_Slider.wholeNumbers)
        {
            SetIntValue(PlayerPrefs.GetInt(m_Slider.name, (int)defaultValue));
        }
        else
        {
            SetFloatValue(PlayerPrefs.GetFloat(m_Slider.name, defaultValue));
        }
        UpdateTextByValue();
    }

    // Update value text based on slider value
    public void UpdateTextByValue()
    {
        // Error check
        if (!m_Slider)
            return;

        if (m_Slider.wholeNumbers)
            valueText.text = m_Slider.value.ToString();
        else
            valueText.text = m_Slider.value.ToString("F2");
    }

    // Update slider value based on value text
    public void UpdateValueByText()
    {
        float textToValue = float.Parse(valueText.text);

        if (textToValue < m_Slider.minValue)
        {
            textToValue = m_Slider.minValue;
            UpdateTextByValue();
        }
        else if (textToValue > m_Slider.maxValue)
        {
            textToValue = m_Slider.maxValue;
            UpdateTextByValue();
        }

        m_Slider.value = textToValue;
    }
}
