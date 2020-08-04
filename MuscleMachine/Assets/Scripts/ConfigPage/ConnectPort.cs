/****************************************************
    文件：ConnectPort.cs
	作者：Ling
    邮箱: 1759147969@qq.com
    日期：2020.07.23 20.32.36
	功能：链接端口
*****************************************************/

using UnityEngine;
using System.IO.Ports;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading;
using System;

/// <summary>
/// 接收到的数据
/// </summary>
public class ReceiveData
{
    public static List<int> ReceiveDataList = new List<int>();
    public static List<int> DealDataList = new List<int>();
}

public class ConnectPort : MonoBehaviour 
{
    public static ConnectPort _instance;
    public static ConnectPort Instance
    {
        get
        {
            return _instance;
        }
    }

    public SerialPort SerialPort;
    public bool IsReceiveData;
    private Dropdown _dropDown;
    private Button _connectBtn;
    private Thread _receiveThread;

    private void Awake()
    {
        _instance = this;
    }
    private void Start()
    {
        _connectBtn = GetComponentInChildren<Button>();
        _connectBtn.onClick.AddListener(() => Connect());
    }

    private void OnEnable()
    {
        _dropDown = GetComponentInChildren<Dropdown>();
        _dropDown.options.Clear();
        string[] portsName = GetPortsName();
        List<string> portsNameLst = new List<string>();
        for(int i = 0; i<= portsName.Length - 1; i++)
        {
            portsNameLst.Add(portsName[i]);
        }
        _dropDown.AddOptions(portsNameLst);
    }

    /// <summary>
    /// 返回所有链接的串口名称
    /// </summary>
    /// <returns></returns>
    private string[] GetPortsName()
    {
        return SerialPort.GetPortNames();
    }

    private void Connect()
    {
        //链接端口
        string portName = _dropDown.transform.Find(ConstTable.Instance.R_dropDownLabel).GetComponent<Text>().text;
        SerialPort = new SerialPort(portName);
        SerialPort.BaudRate = 230400;  //波特率
        SerialPort.StopBits = StopBits.One;  //停止位
        SerialPort.DataBits = 8;             //数据位
        SerialPort.Parity = Parity.None;     //奇偶校验
        SerialPort.ReadTimeout = 800;        //读出延时
        SerialPort.Open();

        IsReceiveData = true;
        _receiveThread = new Thread(ReceivePortData);
        _receiveThread.IsBackground = true;
        _receiveThread.Start();

        //调整按钮功能
        _connectBtn.GetComponentInChildren<Text>().text = "Disconnect";
        _connectBtn.onClick.RemoveAllListeners();
        _connectBtn.onClick.AddListener(() => DisConnect());
    }

    public void DisConnect()
    {
        //关闭链接
        IsReceiveData = false;
        SerialPort.Close();
        SerialPort.Dispose();
        _receiveThread.Abort();
        //_receiveThread = null;
        ReceiveData.ReceiveDataList.Clear();

        //调整按钮功能
        _connectBtn.GetComponentInChildren<Text>().text = "Connect";
        _connectBtn.onClick.RemoveAllListeners();
        _connectBtn.onClick.AddListener(() => Connect());
    }

    /// <summary>
    /// 接受端口传递的数据
    /// </summary>
    private void ReceivePortData()
    {
        while (IsReceiveData)
        {
            if (SerialPort.IsOpen)
            {
                byte[] data = new byte[SerialPort.BytesToRead];   //定义缓冲区，因为串口事件触发时有可能收到不止一个字节
                //_serialPort.Read(data, 0, data.Length);
                SerialPort.Read(data, 0, data.Length);
                List<int> receiveDataList = new List<int>();
                //添加到
                for (int i = 0; i <= data.Length - 1; i++)
                {
                    string hexData = data[i].ToString("X2");
                    if (hexData.Equals("FA") || hexData.Equals("AF") || hexData.Equals("0E"))
                        continue;
                    receiveDataList.Add(data[i]);
                }
                //数组超过一定长度，删除掉左侧数据
                if (receiveDataList.Count >= 64)
                {
                    receiveDataList.RemoveRange(0, receiveDataList.Count - 64);
                }
                ReceiveData.ReceiveDataList = receiveDataList;
                //ReceiveData.DealDataList.AddRange(receiveDataList);
                Thread.Sleep(18);
            }
        }
        Thread.Sleep(30);
    }
}