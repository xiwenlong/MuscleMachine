/****************************************************
    文件：ChannelControll.cs
	作者：Ling
    邮箱: 1759147969@qq.com
    日期：#CreateTime#
	功能：Nothing
*****************************************************/

using UnityEngine;

public class ChannelControl : MonoBehaviour 
{
    private void OnEnable()
    {
        if (!ConnectPort.Instance.IsReceiveData) return;
        int count = int.Parse(PlayerPrefs.GetString(ConstTable.Instance.R_P_SerialChannelCount));

        for (int i = 0;i <= count - 1; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i <= transform.childCount - 1; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }
}
