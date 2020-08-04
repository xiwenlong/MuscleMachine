/****************************************************
    文件：StartSoftWare.cs
	作者：Ling
    邮箱: 1759147969@qq.com
    日期：2020.07.23 09.01.10
	功能：软件开始的初始化事件
*****************************************************/

using UnityEngine;
using System.Collections;

public class StartSoftWare : MonoBehaviour 
{
    private void Start()
    {
        if (!PlayerPrefs.HasKey(ConstTable.Instance.R_P_BandPassFilter))
        {
            PlayerPrefs.SetString(ConstTable.Instance.R_P_BandPassFilter, "1,4999");
        }
        //if (!PlayerPrefs.HasKey(ConstTable.Instance.R_P_NotchFilter))
        //{
            PlayerPrefs.SetString(ConstTable.Instance.R_P_NotchFilter, "50");
        //}
        //if (!PlayerPrefs.HasKey(ConstTable.Instance.R_P_SerialChannelColor))
        //{
            PlayerPrefs.SetString(ConstTable.Instance.R_P_SerialChannelColor, "Red,Yellow,Green,Gray,Blue,Cyan");
        //}
        //if (!PlayerPrefs.HasKey(ConstTable.Instance.R_P_SerialChannelCount))
        //{
            PlayerPrefs.SetString(ConstTable.Instance.R_P_SerialChannelCount, "1");
        //}

        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(3f);
        gameObject.SetActive(false);
    }
}