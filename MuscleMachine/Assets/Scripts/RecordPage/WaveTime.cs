/****************************************************
    文件：SaveWav.cs
	作者：Ling
    邮箱: 1759147969@qq.com
    日期：#CreateTime#
	功能：Nothing
*****************************************************/

using UnityEngine;
using UnityEngine.UI;

public class WaveTime : MonoBehaviour
{
    private readonly string R_SliderText = "Text_Slider";
    private Slider IntervalSlider;

    private void Start()
    {
        IntervalSlider = GetComponentInChildren<Slider>();    
    }

    private void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            IntervalSlider.value += Input.GetAxis("Mouse ScrollWheel") * 200;
            transform.Find(R_SliderText).GetComponent<Text>().text = (int)IntervalSlider.value + "ms";
        }
    }
}
