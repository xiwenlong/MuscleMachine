/****************************************************
    文件：SerialChannel.cs
	作者：Ling
    邮箱: 1759147969@qq.com
    日期：2020.07.17 11.35.10
	功能：颜色选择器
*****************************************************/

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

/// <summary>
/// 下面两个数据是6个下拉框公用的
/// </summary>
public class SerialChannelConfig
{
    public static Color[] SerialChannelColor =
        new Color[] { new Color(1,0.3f,0.3f,1),new Color(0.79f,0.75f,0.24f,1),new Color(0.23f,0.75f,0.23f,1),
                      Color.gray, new Color(0.26f,0.26f,0.77f,1),new Color(0.4f,0.78f,0.78f),Color.white};
    public static string[] SerialChannelStr = new string[] { "Red", "Yellow", "Green", "Gray", "Blue", "Cyan", "White" };
    public static int[] SelectedColor = new int[] { 1,1,1,1,1,1,0};
}

public class SerialChannel : Dropdown
{
    public Color SerialColor;
    public int SelectedIndex;


    public override void OnPointerClick(PointerEventData eventData)
    {
        ShowDropItems();
    }

    protected override void Start()
    {
        base.Start();
        SelectedIndex = transform.parent.GetSiblingIndex();

        //设置下拉框背景颜色
        SerialColor = SerialChannelConfig.SerialChannelColor[SelectedIndex];
        transform.GetComponent<Image>().color = SerialColor;
    }

    public void ShowDropItems()
    {
        //添加下拉框的内容
        this.options.Clear();
        List<string> addItems = new List<string>();
        for (int i = 0; i <= SerialChannelConfig.SelectedColor.Length - 1; i++)
        {
            if (SerialChannelConfig.SelectedColor[i] == 0)
            {
                addItems.Add(i.ToString());
            }
        }
        this.AddOptions(addItems);

        //item添加完成以后，调用父类的显示方法
        base.Show();

        //修改下拉框的总高度，增加22px，
        Transform toggleRoot = transform.Find(ConstTable.Instance.R_dropDownList);
        toggleRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(0, toggleRoot.GetComponent<RectTransform>().sizeDelta.y + 22);

        //获得content的所有子物体
        Toggle[] toggleList = toggleRoot.GetComponent<ScrollRect>().content.GetComponentsInChildren<Toggle>(false);
        //对所有子物体进行自定义设计
        for (int i = 0; i < toggleList.Length; i++)
        {
            Toggle toggle = toggleList[i];
            toggle.onValueChanged.RemoveAllListeners();
            toggle.isOn = false;
            //修改每一个下拉框的背景颜色
            toggle.transform.Find(ConstTable.Instance.R_itemBG).GetComponent<Image>().color
                = SerialChannelConfig.SerialChannelColor[int.Parse(toggle.name[toggle.name.Length - 1].ToString())];
            toggle.onValueChanged.AddListener(x => OnSelectItem(toggle));
        }
    }

    /// <summary>
    /// 选择某个item后的响应内容
    /// </summary>
    /// <param name="toggle"></param>
    private void OnSelectItem(Toggle toggle)
    {
        if (!toggle.isOn)
        {
            toggle.isOn = true;
            return;
        }

        int selectedIndex = -1;
        Transform trans = toggle.transform;
        Transform parent = trans.parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i) == trans)
            {
                selectedIndex = i - 1;
                break;
            }
        }

        if (selectedIndex < 0)
            return;
        value = selectedIndex;
        //根据选择的item的背景颜色，修改dropdown物体的image颜色
        SerialColor = toggle.transform.Find(ConstTable.Instance.R_itemBG).GetComponent<Image>().color;
        transform.GetComponent<Image>().color = SerialColor;
        //颜色交替，之前的颜色设置为可用，当前重新选择的颜色设置为已用。
        SerialChannelConfig.SelectedColor[SelectedIndex] = 0;
        SelectedIndex = int.Parse(toggle.name[toggle.name.Length - 1].ToString());
        SerialChannelConfig.SelectedColor[SelectedIndex] = 1;

        SetString();
        //调用父类的隐藏方法
        Hide();
    }

    /// <summary>
    /// 修改某个通道颜色后，存入内存中
    /// </summary>
    private void SetString()
    {
        SerialChannel[] serialChannels = transform.parent.parent.GetComponentsInChildren<SerialChannel>();
        string str = "";

        for(int i = 0;i <= serialChannels.Length - 1; i++)
        {
            str += SerialChannelConfig.SerialChannelStr[serialChannels[i].SelectedIndex] + ",";
        }
        str = str.Remove(str.Length - 1);
        PlayerPrefs.SetString(ConstTable.Instance.R_P_SerialChannelColor, str);
    }
}