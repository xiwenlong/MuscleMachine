/****************************************************
    文件：ConstTable.cs
	作者：Ling
    邮箱: 1759147969@qq.com
    日期：2020.07.22 19.07.40
	功能：常量表
*****************************************************/

using UnityEngine;

public class ConstTable : MonoBehaviour 
{
    private static ConstTable m_instance;
    public static ConstTable Instance
    {
        get
        {
            if (m_instance is null)
            {
                m_instance = new ConstTable();
            }
            return m_instance;
        }
    }

    public readonly string R_canvas = "Canvas";
    public readonly string R_configPage = "ConfigPage";
    public readonly string R_recordPage = "RecordPage";
    public readonly string R_NotchFilter = "NotchFilter";
    public readonly string R_ChannelNumber = "ChannelNumber";

    public readonly string R_Material = "Material";

    public readonly string R_dropDownLabel = "Label";
    public readonly string R_dropDownList = "Dropdown List";
    public readonly string R_itemBG = "Item Background";

    public readonly string R_configBtn = "Btn_Config";
    public readonly string R_exportBtn = "Btn_Export";
    public readonly string R_recordBtn = "Btn_Record";
    public readonly string R_importBtn = "Btn_Import";
    public readonly string R_PlayBtn = "Btn_Play";
    public readonly string R_RefreshBtn = "Btn_Refresh";
    public readonly string R_Exit = "Btn_Exit";
    public readonly string R_WaveTime = "WaveTime";

    public readonly string R_TextLow = "Text_Low";
    public readonly string R_TextHigh = "Text_High";
    public readonly string R_SliderL = "SliderL";
    public readonly string R_SliderR = "SliderR";

    public readonly string R_P_SerialChannelCount = "SerialChannelCount";
    public readonly string R_P_SerialChannelColor = "SerialChannelColor";
    public readonly string R_P_BandPassFilter = "BandPassFilter";
    public readonly string R_P_NotchFilter = "NotchFilter";
}