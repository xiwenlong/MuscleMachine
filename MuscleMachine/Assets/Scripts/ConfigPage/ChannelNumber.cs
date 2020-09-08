using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChannelNumber : MonoBehaviour
{
    private Dropdown _dropDown;

    private void Start()
    {
        _dropDown = transform.Find("Dropdown").GetComponent<Dropdown>();
        _dropDown.onValueChanged.AddListener((int value) => Save(value));
    }

    private void Save(int value)
    {
        PlayerPrefs.SetString(ConstTable.Instance.R_P_SerialChannelCount, (value + 1).ToString());
        GameObject.Find("SerialChannel").GetComponent<SerialChannelControl>().RefreshUI((value + 1));
    }
}