/****************************************************
    文件：WindowSize.cs
	作者：Ling
    邮箱: 1759147969@qq.com
    日期：#CreateTime#
	功能：Nothing
*****************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;
public class WindowMaxAndMin : MonoBehaviour
{
    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();
    const int SW_SHOWMINIMIZED = 2; //｛最小化, 激活｝  
    const int SW_SHOWMAXIMIZED = 3;//最大化  
    const int SW_SHOWRESTORE = 1;//还原  

    private bool _switchOver;
    public static void OnClickMinimize()
    { //最小化   
        ShowWindow(GetForegroundWindow(), SW_SHOWMINIMIZED);
    }
    public static void OnClickMaximize()
    {
        //最大化  
        ShowWindow(GetForegroundWindow(), SW_SHOWMAXIMIZED);
    }
    public void OnClickRestore()
    {
        //还原  
        //ShowWindow(GetForegroundWindow(), SW_SHOWRESTORE);
    }

    private void Start()
    {
        ////string newOutFileName = outFileName.Substring(0, outFileName.Length - 4) + ".wav";
        ////UnityEngine.Debug.Log((curDir + "\\" + outFileName) + " " + newOutFileName);
        //string curDir = System.Environment.CurrentDirectory;

        ////string arg = string.Format("-i F:\\Study\\selfGame\\Game\\MuscleMachine\\movie\\1.mp4 - ar 8000 - ac 1 - acodec pcm_u8 F:\\Study\\selfGame\\Game\\MuscleMachine\\movie\\1.wav");
        //string arg = string.Format("-i {0}/{1}/{2}%d.jpg -s 1024*768 -b:v 4096k -framerate 8 -vcodec mpeg4 {3}", curDir, "tImg", "img", "2.mp4");

        ////string arg = string.Format("-i movie\\1.mp4 - ar 8000 - ac 1 - acodec pcm_u8 F:\\Study\\selfGame\\Game\\MuscleMachine\\movie\\1.wav");
        //Process p1 = new Process();
        //p1.StartInfo.FileName = curDir + @"/ffmpeg.exe";

        //p1.StartInfo.Arguments = arg;//参数
        //p1.StartInfo.RedirectStandardError = true;//重定向标准错误流
        //p1.StartInfo.CreateNoWindow = true;
        //p1.StartInfo.UseShellExecute = false;//不使用操作系统外壳程序启动
        //p1.ErrorDataReceived += new DataReceivedEventHandler(Output);//输出流的事件
        //p1.Start();//启动
        //p1.BeginErrorReadLine();//开始异步读取
        //p1.WaitForExit();//阻塞等待进程结束
        //p1.Close();   //关闭
        //p1.Dispose(); //释放
    }
    private static void Output(object sendProcess, DataReceivedEventArgs output)
    {
        if (!String.IsNullOrEmpty(output.Data))
        {
            //UnityEngine.Debug.Log(output.Data);
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && Screen.fullScreen)
        {
            //_switchOver = !_switchOver;
            Screen.SetResolution(1600, 900, false);
            Screen.fullScreen = false;
        }
    }
    //测试  
    //public void OnGUI()
    //{
    //    if (GUI.Button(new Rect(100, 100, 200, 100), "最大化"))
    //        OnClickMaximize();
    //    if (GUI.Button(new Rect(100, 300, 200, 100), "最小化"))
    //        OnClickMinimize();
    //    if (GUI.Button(new Rect(100, 500, 200, 100), "窗口还原"))
    //        OnClickRestore();
    //}
}