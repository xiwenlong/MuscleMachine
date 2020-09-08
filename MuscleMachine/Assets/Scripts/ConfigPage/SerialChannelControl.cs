using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerialChannelControl : MonoBehaviour
{
    private void Start()
    {
        int num = int.Parse(PlayerPrefs.GetString(ConstTable.Instance.R_P_SerialChannelCount));
        ShowControl(num);
    }

    public void RefreshUI(int num)
    {
        ShowControl(num);
    }

    private void ShowControl(int num)
    {
        //int num = int.Parse(PlayerPrefs.GetString(ConstTable.Instance.R_P_SerialChannelCount));
        for (int i = 0; i <= transform.childCount - 1; i++)
        {
            if ((i + 1) <= num)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
