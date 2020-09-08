/****************************************************
    文件：ConnectPort.cs
	作者：Ling
    邮箱: 1759147969@qq.com
    日期：2020.07.23 20.32.36
	功能：链接端口
*****************************************************/

using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 接收到的数据
/// </summary>
public class ReceiveData
{
    public static int ChannelCount;
    public static List<List<int>> ReceiveDataList = new List<List<int>>();   //接受用
    public static List<List<int>> DealDataList = new List<List<int>>();      //导出用
    public static List<byte> WaitForDealList = new List<byte>();             //接受的缓存用
    public static List<List<int>> RecordDataList = new List<List<int>>();    //录制用
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
        for (int i = 0; i <= 5; i++)
        {
            ReceiveData.ReceiveDataList.Add(new List<int>());
            ReceiveData.DealDataList.Add(new List<int>());
            ReceiveData.RecordDataList.Add(new List<int>());
        }
    }

    private void OnEnable()
    {
        _dropDown = GetComponentInChildren<Dropdown>();
        _dropDown.options.Clear();
        string[] portsName = GetPortsName();
        List<string> portsNameLst = new List<string>();
        for (int i = 0; i <= portsName.Length - 1; i++)
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
        SerialPort.BaudRate = 230400;        //波特率
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

        for (int i = 0; i <= ReceiveData.ReceiveDataList.Count - 1; i++)
        {
            ReceiveData.DealDataList[i].Clear();
        }
    }

    public void DisConnect()
    {
        //关闭链接
        IsReceiveData = false;
        SerialPort.Close();
        SerialPort.Dispose();
        _receiveThread.Abort();
        _receiveThread = null;

        for (int i = 0; i <= ReceiveData.ReceiveDataList.Count - 1; i++)
        {
            ReceiveData.ReceiveDataList[i].Clear();
            ReceiveData.RecordDataList[i].Clear();
        }
        ReceiveData.WaitForDealList.Clear();

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
        //while (IsReceiveData)
        int count = 0;
        while (IsReceiveData)
        {
            count++;
            if (SerialPort.IsOpen)
            {
                byte[] data = new byte[SerialPort.BytesToRead];   //定义缓冲区，因为串口事件触发时有可能收到不止一个字节
                //byte[] data = new byte[64];   //定义缓冲区，因为串口事件触发时有可能收到不止一个字节
                SerialPort.Read(data, 0, data.Length);
                //Debug.Log("长度：" + data.Length);
                //List<int> receiveDataList = new List<int>();

                ReceiveData.WaitForDealList.AddRange(data);

                int minLeft = 1000, maxRight = 0;
                //string str = "";
                //String str2 = "";
                //for (int i = 0; i <= ReceiveData.WaitForDealList.Count - 1; i++)
                //{
                //    str += ReceiveData.WaitForDealList[i].ToString("X2") + " ";
                //    str2 += ReceiveData.WaitForDealList[i] + " ";
                //}
                //Debug.Log(str);
                //Debug.Log(str2);
                for (int i = 0; i <= ReceiveData.WaitForDealList.Count - 1; i++)
                {
                    //找到FA，AF
                    int left = 0, right = 0;
                    int j = i;
                    for (; j <= ReceiveData.WaitForDealList.Count - 1; j++)
                    {
                        if (ReceiveData.WaitForDealList[j].ToString("X2").Equals("FA"))
                        {
                            left = j;
                            //Debug.Log("左边：" + left);
                        }
                        if (ReceiveData.WaitForDealList[j].ToString("X2").Equals("AF") && left != 0 && j > left)
                        {
                            right = j;
                            //Debug.Log("右边：" + right);
                        }
                        if (left != 0 && right != 0) break;
                    }
                    //找到了
                    if (left != 0 && right != 0)
                    {
                        minLeft = minLeft <= left ? minLeft : left;
                        maxRight = maxRight >= right ? maxRight : right;
                        //Debug.Log(left + " " + right);
                        //i = j;  //下一次掠过已经找到的部分
                        int k = 0;
                        //加2，舍去FA,0E
                        //Debug.Log(left + 2 + 11);
                        for (i = left + 2; i <= right - 3; i+=2)
                        {
                            //int temp = ReceiveData.WaitForDealList[i] * 100 + ReceiveData.WaitForDealList[i + 1];
                            //Debug.Log(temp + " " + Convert.ToInt32("0x"+temp.ToString(), 16));
                            try
                            {
                                if (ReceiveData.ReceiveDataList[k++].Count >= 960) continue;

                                ReceiveData.ReceiveDataList[k - 1].Add(Convert.ToInt32("0x" + ReceiveData.WaitForDealList[i].ToString("X2") + ReceiveData.WaitForDealList[i+1].ToString("X2"), 16));
                                //if((k-1)==0)
                                //Debug.Log((k - 1) + " " + "0x" + ReceiveData.WaitForDealList[i].ToString("X2") + ReceiveData.WaitForDealList[i + 1].ToString("X2") + " " + Convert.ToInt32("0x" + ReceiveData.WaitForDealList[i].ToString("X2") + ReceiveData.WaitForDealList[i + 1].ToString("X2"), 16) + " " + ReceiveData.ReceiveDataList[k - 1][ReceiveData.ReceiveDataList[k - 1].Count - 1] /*+ " " + ReceiveData.ReceiveDataList[k - 1].Count*/);
                            }
                            catch (Exception e)
                            {
                                Debug.Log(e + " i:" + i + " RE" + ReceiveData.WaitForDealList.Count
                                    + "k:" + (k - 1));
                            }
                            k %= 12;
                            //Debug.Log(i + "-" + (k - 1) + ":" + ((k - 1) / 2) + " " + ReceiveData.WaitForDealList[i] + " " + ReceiveData.WaitForDealList[i].ToString("X2"));
                        }
                        i = j;  //下一次掠过已经找到的部分,大循环自己++，所以这边不用加
                    }
                }
                //Debug.Log(ReceiveData.WaitForDealList.Count + " " + (maxRight + 1));
                if (maxRight > 0)
                    ReceiveData.WaitForDealList.RemoveRange(0, maxRight + 1);

                Thread.Sleep(18);

            }
        }
        Thread.Sleep(30);
    }

    //16进制转10进制
    private void HexToDec()
    {

    }
}