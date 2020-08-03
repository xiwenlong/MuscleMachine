/****************************************************
    文件：BandPassFilter.cs
	作者：Ling
    邮箱: 1759147969@qq.com
    日期：2020.07.17 11.35.10
	功能：载波过滤器
*****************************************************/

using UnityEngine;
using UnityEngine.UI;

public class BandPassFilter : MonoBehaviour 
{
    private InputField _textLowInput;
    private InputField _textHighInput;
    private Slider _sliderL;
    private Slider _sliderR;

    private void Start()
    {
        _textLowInput = transform.Find(ConstTable.Instance.R_TextLow).GetComponent<InputField>();
        _textHighInput = transform.Find(ConstTable.Instance.R_TextHigh).GetComponent<InputField>();
        _textLowInput.onEndEdit.AddListener(EndEditRight);
        _textHighInput.onEndEdit.AddListener(EndEditLeft);

        _sliderL = transform.Find(ConstTable.Instance.R_SliderL).GetComponent<Slider>();
        _sliderL.onValueChanged.AddListener(ValueChangedLeft);
        _sliderR = transform.Find(ConstTable.Instance.R_SliderR).GetComponent<Slider>();
        _sliderR.onValueChanged.AddListener(ValueChangedRight);

        float left = float.Parse(PlayerPrefs.GetString(ConstTable.Instance.R_P_BandPassFilter).Split(',')[0]);
        float right = float.Parse(PlayerPrefs.GetString(ConstTable.Instance.R_P_BandPassFilter).Split(',')[1]);
        _sliderL.value =_sliderL.maxValue - (int)right + 1;
        _sliderR.value = (int)left;
    }

    private void SavePlayerPrefs()
    {
        int left = (int)_sliderR.value;
        int right = ((int)_sliderL.maxValue - (int)_sliderL.value + 1);
        if (left == right && left != 1)   //两边不是最小值，则永远是left-1
        {
            left -= 1;
        }
        else if(left == right)  //是最小值时，right+1
        {
            right += 1;
        }
        //存储时，不能让两个值一样
        PlayerPrefs.SetString(ConstTable.Instance.R_P_BandPassFilter, left + "," + right);
        //Debug.Log(PlayerPrefs.GetString(ConstTable.Instance.R_P_BandPassFilter));
    }

    private void ValueChangedLeft(float num)
    {
        _textHighInput.text = (_sliderL.maxValue - (int)num + 1).ToString();
        if (int.Parse(_textHighInput.text) <= int.Parse(_textLowInput.text))
        {
            _sliderL.value = _sliderL.maxValue - int.Parse(_textLowInput.text) + 1;
            //_textLowInput.text = _sliderL.value.ToString();
        }
        SavePlayerPrefs();
    }

    private void ValueChangedRight(float num)
    {
        _textLowInput.text = ((int)num).ToString();
        if (int.Parse(_textLowInput.text) >= int.Parse(_textHighInput.text))
        {
            _sliderR.value = int.Parse(_textHighInput.text);
            //_textHighInput.text = _sliderR.value.ToString();
        }
        SavePlayerPrefs();

    }

    private void EndEditLeft(string str)
    {
        _sliderL.value = _sliderL.maxValue - int.Parse(str) + 1;
        if (int.Parse(_textHighInput.text) <= int.Parse(_textLowInput.text))
        {
            _sliderL.value = _sliderL.maxValue - int.Parse(_textLowInput.text) + 1;
            //_textLowInput.text = _sliderL.value.ToString();
        }
        //SavePlayerPrefs();

    }

    private void EndEditRight(string str)
    {
        _sliderR.value = int.Parse(str);
        if (int.Parse(_textLowInput.text) >= int.Parse(_textHighInput.text))
        {
            _sliderR.value = int.Parse(_textHighInput.text);
            //_textHighInput.text = _sliderR.value.ToString();
        }
        //SavePlayerPrefs();
    }
}
