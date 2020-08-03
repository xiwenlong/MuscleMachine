/****************************************************
    文件：MovieRecord.cs
	作者：Ling
    邮箱: 1759147969@qq.com
    日期：2020.07.16 16.13.33
	功能：屏幕录制功能
*****************************************************/

using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System;
using System.IO;
using UnityEngine.UI;

public class MovieRecord : MonoBehaviour
{
    private readonly string R_TextInfo = "Text_Info";
    private int state = -1;
    private int rate = 25;
    private float curSpan = 0;
    private float nowTime = 0;
    private int _currentImgIndex = 0;
    private bool _isRecordScreen;     //是否录屏中

    private static MovieRecord _instance;
    public static MovieRecord Instance
    {
        get { return _instance; }
    }
    public static Action<string> OnMovieCreateOver;


    private void Awake()
    {
        _instance = this;
        state = 1;
        if (!Directory.Exists("tImg"))
            Directory.CreateDirectory("tImg");
        if (!Directory.Exists("movie"))
            Directory.CreateDirectory("movie");
        curSpan = 1.0f / 25;

        transform.GetComponent<Button>().onClick.AddListener(() => StartOrStop());
    }

    //创建视频，将所有图片制作成视频
    public void CreateMovie(string path, string comName, string outFileName)
    {
        Process p = new Process();
        string curDir = System.Environment.CurrentDirectory;
        //string curDir = Application.persistentDataPath + "/";
        p.StartInfo.FileName = curDir + @"/ffmpeg.exe";
        //print(System.Environment.CurrentDirectory + @"/ffmpeg.exe");
        //string arg = string.Format("-i {0}/{1}/{2}%d.jpg -s 1024*768 -r 8 -vcodec mpeg4 {3}", curDir, path, comName, outFileName);
        string arg = string.Format("-i {0}/{1}/{2}%d.jpg -s 1024*768 -b:v 4096k -framerate 8 -vcodec mpeg4 {3}", curDir, path, comName, outFileName);
        //string arg = string.Format("-i {0}/{1}/{2}%d.jpg -s 1024*768 -b:v 4096k -framerate 8 -vcodec mpeg4 {3}", curDir, path, comName, outFileName);
        //输出保存路径
        transform.parent.Find(R_TextInfo).GetComponent<Text>().text = "录制结束，保存至：\n" + curDir + "\\" + outFileName;
        //transform.parent.Find(R_TextInfo).GetComponent<Text>().text = curDir+"/"+path+"/"+comName+"\n"+outFileName;

        p.StartInfo.Arguments = arg;//参数
        p.StartInfo.RedirectStandardError = true;//重定向标准错误流
        p.StartInfo.CreateNoWindow = false;
        p.StartInfo.UseShellExecute = false;//不使用操作系统外壳程序启动
        p.ErrorDataReceived += new DataReceivedEventHandler(Output);//输出流的事件
        p.Start();//启动
        p.BeginErrorReadLine();//开始异步读取
        p.WaitForExit();//阻塞等待进程结束
        p.Close();   //关闭
        p.Dispose(); //释放
        StartCoroutine("DeleteFile");

        //执行视频创建回调函数
        OnMovieCreateOver?.Invoke(Directory.GetCurrentDirectory() + "\\" + outFileName);
        //UnityEngine.Debug.Log(Directory.GetCurrentDirectory());
    }
    //输出消息
    private static void Output(object sendProcess, DataReceivedEventArgs output)
    {
        if (!String.IsNullOrEmpty(output.Data))
        {
            UnityEngine.Debug.Log(output.Data);
        }
    }


    private void StartOrStop()
    {
        //如果在录频，则结束录频
        if (_isRecordScreen)
        {
            StartCoroutine("StopRecord");
            _isRecordScreen = false;
            InvalidOrValidBtns(true);
        }
        //接口在接受数据，且不在录频中
        else if(ConnectPort.Instance.IsReceiveData && !_isRecordScreen)
        {
            StartCoroutine("RecordScreen");
            _isRecordScreen = true;
            transform.parent.Find(R_TextInfo).GetComponent<Text>().text = "录制中";
            //一些按钮无效化
            InvalidOrValidBtns(false);
        }
    }

    private void InvalidOrValidBtns(bool flag)
    {
        RecordPageBtns recordPageBtns = transform.parent.GetComponent<RecordPageBtns>();
        recordPageBtns._configBtn.interactable = flag;
        recordPageBtns._exportBtn.interactable = flag;
        recordPageBtns._importBtn.interactable = flag;
        //recordPageBtns._configBtn.interactable = flag;
    }

    //结束录制视频
    public IEnumerator StopRecord()
    {
        state = -1;
        StopCoroutine("RecordScreen");
        _currentImgIndex = 0;
        CreateMovie("tImg", "img", string.Format("movie/{0}.mp4", DateTime.Now.ToString("yyyyMMddhhmmss")));

        yield return null;
    }

    /// <summary>
    /// 录屏
    /// </summary>
    /// <returns></returns>
    private IEnumerator RecordScreen()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            //yield return null;
            if (state == 1)
            {
                nowTime += Time.deltaTime;
                if (nowTime > curSpan)
                {
                    nowTime = 0;
                    Texture2D texture = ScreenShots();
                    //t.Apply();
                    SaveImgToJpg(texture, "tImg/img", _currentImgIndex);
                    _currentImgIndex++;
                    Resources.UnloadUnusedAssets();
                }
            }
        }
    }

    /// <summary>
    /// 截取当前指定范围内的屏幕内容
    /// </summary>
    /// <returns></returns>
    private Texture2D ScreenShots()
    {
        Texture2D texture = new Texture2D(Screen.width, Screen.height);
        UnityEngine.Debug.Log(Screen.width + " " + Screen.height);
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        return texture;
    }

    /// <summary>
    /// 保存截屏的图片 为jpg文件
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="comName"></param>
    /// <param name="index"></param>
    private void SaveImgToJpg(Texture2D texture, string comName, int index)
    {
        var bytes = texture.EncodeToJPG();
        FileStream fs = new FileStream(comName + index + ".jpg", FileMode.Create);
        fs.Write(bytes, 0, bytes.Length);
        fs.Flush();
        fs.Close();
    }

    /// <summary>
    /// 删除文件夹
    /// </summary>
    /// <returns></returns>
    private IEnumerator DeleteFile()
    {
        //yield return new WaitForSeconds(5f);
        //删除文件夹，并重新生成
        Directory.Delete("tImg", true);
        //yield return new WaitForSeconds(5f);
        Directory.CreateDirectory("tImg");
        yield return new WaitForSeconds(5f);
        transform.parent.Find(R_TextInfo).GetComponent<Text>().text = "";
    }
}