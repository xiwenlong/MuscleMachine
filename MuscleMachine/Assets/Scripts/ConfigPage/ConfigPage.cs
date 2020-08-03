/****************************************************
    文件：ConfigPage.cs
	作者：Ling
    邮箱: 1759147969@qq.com
    日期：2020.07.17 11.27.04
	功能：配置页面
*****************************************************/

using UnityEngine;
using UnityEngine.UI;

public class ConfigPage : MonoBehaviour 
{
    private readonly string R_SettingBtn = "Btn_Setting";
    private readonly string R_RecordPage = "RecordPage";
    private Button _settingBtn;

    private void Start()
    {
        _settingBtn = transform.Find(R_SettingBtn).GetComponent<Button>();
        _settingBtn.onClick.AddListener(() => ClickSettingBtn());
    }

    private void ClickSettingBtn()
    {
        GameObject.Find(ConstTable.Instance.R_canvas).transform.GetChild(0).gameObject.SetActive(false);
        GameObject.Find(ConstTable.Instance.R_canvas).transform.GetChild(1).gameObject.SetActive(true);
        SetString();
    }

    private void SetString()
    {
        //保存陷波滤波器的hz值
        Toggle[] toggles = transform.Find(ConstTable.Instance.R_NotchFilter).GetComponentsInChildren<Toggle>();
        string str = "";
        for(int i = 0;i <= toggles.Length - 1; i++)
        {
            if (toggles[i].isOn) str = toggles[i].name.Substring(0, 2);
        }
        PlayerPrefs.SetString(ConstTable.Instance.R_P_NotchFilter, str);

        //保存所选择的通道数
        str = transform.Find(ConstTable.Instance.R_ChannelNumber + "/Dropdown").GetComponentInChildren<Text>().text;
        PlayerPrefs.SetString(ConstTable.Instance.R_P_SerialChannelCount, str);
    }
}
