/*  SliderValueUpdate.cs
    (DEPRECATED)

    Updates text string to match slider value. 
*/

using UnityEngine;
using UnityEngine.UI;

public class SliderValueUpdate : MonoBehaviour
{
    private Text m_Text;
    [SerializeField] private Slider m_Slider = null; // Set up in inspector

    // Start is called before the first frame update
    void Start()
    {
        m_Text = GetComponent<Text>();
    }

    // Update is called once per frame
    public void UpdateValue()
    {
        if (m_Slider.wholeNumbers)
            m_Text.text = m_Slider.value.ToString();
        else
            m_Text.text = m_Slider.value.ToString("F2");
    }
}
