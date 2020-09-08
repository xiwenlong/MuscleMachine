/****************************************************
    文件：RecordPageBtns.cs
	作者：Ling
    邮箱: 1759147969@qq.com
    日期：2020.07.22 18.57.57
	功能：记录页面的所有按钮
*****************************************************/

using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.Video;
using System;

public class RecordPageBtns : MonoBehaviour
{
    public Sprite[] PlayOrPuseImage;

    public Button _configBtn;
    public Button _exportBtn;
    public Button _recordBtn;
    public Button _importBtn;
    public Button _playBtn;
    public Button _refreshBtn;
    public Button _exitBtn;
    private RawImage _rawImage;
    private bool _isPlay;
    private bool _isDraw;

    private void Start()
    {
        _isPlay = true;
        _configBtn = transform.Find(ConstTable.Instance.R_configBtn).GetComponent<Button>();
        _exportBtn = transform.Find(ConstTable.Instance.R_exportBtn).GetComponent<Button>();
        _recordBtn = transform.Find(ConstTable.Instance.R_recordBtn).GetComponent<Button>();
        _importBtn = transform.Find(ConstTable.Instance.R_importBtn).GetComponent<Button>();
        _playBtn = transform.Find(ConstTable.Instance.R_PlayBtn).GetComponent<Button>();
        _refreshBtn = transform.Find(ConstTable.Instance.R_RefreshBtn).GetComponent<Button>();
        _exitBtn = transform.Find(ConstTable.Instance.R_Exit).GetComponent<Button>();

        _configBtn.onClick.AddListener(() => JumptIntoConfigPage());
        _importBtn.onClick.AddListener(() => ImportMovie());
        _playBtn.onClick.AddListener(() => BeginDraw());
        _refreshBtn.onClick.AddListener(() => RefreshVideo());
        _exitBtn.onClick.AddListener(() => ExitVideo());
    }

    /// <summary>
    /// 打开配置页面
    /// </summary>
    private void JumptIntoConfigPage()
    {
        GameObject.Find(ConstTable.Instance.R_canvas).transform.GetChild(0).gameObject.SetActive(true);
        GameObject.Find(ConstTable.Instance.R_canvas).transform.GetChild(1).gameObject.SetActive(false);
    }

    /// <summary>
    /// 导出数据到Excel中（功能写在了ExportDataToExcel中，这里没有调用）
    /// </summary>
    private void ExportDataToExcel()
    {

    }

    /// <summary>
    /// 屏幕录制功能（功能写在了MovieRecord中，这里没有调用）
    /// </summary>
    private void MovieRecord()
    {

    }

    /// <summary>
    /// 从win10中导入movie播放
    /// </summary>
    private void ImportMovie()
    {
        transform.Find(ConstTable.Instance.R_TextInfo).GetComponent<Text>().text = "";
        OpenFileName pth = new OpenFileName();
        pth.structSize = Marshal.SizeOf(pth);
        //pth.filter = "All files (*.*)|*.*";
        pth.filter = "WAV files (*.wav)\0*.wav";
        //pth.filter = "Excel文件(*.xlsx)\0*.xlsx";
        pth.file = new string(new char[256]);
        pth.maxFile = pth.file.Length;
        pth.fileTitle = new string(new char[64]);
        pth.maxFileTitle = pth.fileTitle.Length;
        pth.initialDir = Application.dataPath.Replace("/", "\\") + "\\Resources"; //默认路径
        pth.title = "打开项目";
        pth.defExt = "dat";
        pth.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;

        string filePath = "";
        if (LocalDialog.GetOpenFileName(pth))
        {
            filePath = pth.file; //选择的文件路径;  
            //Debug.Log(filePath);
        }
        if (filePath != "")
        {
            _isPlay = true;
            _isDraw = true;
            WAVReader wav = new WAVReader();
            wav.ReadWavFile(filePath);

            InvalidOtherBtn();
            //Debug.Log("载入");
            Invoke("DrawWavData", 0.5f);
        }
    }

    private void DrawWavData()
    {
        //Debug.Log("调用");
        for(int i = 0;i <= 5; i++)
        {
            transform.parent.Find("Channel").GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i <= ReceiveData.ChannelCount - 1; i++)
        {
            transform.parent.Find("Channel").GetChild(i).gameObject.SetActive(true);
            transform.parent.Find("Channel").GetChild(i).GetComponentInChildren<AudioVisualization>().BeginDrawWavData();
        }
    }

    /// <summary>
    /// 看导入的视频时，其他无关按钮失效
    /// </summary>
    private void InvalidOtherBtn()
    {
        _recordBtn.gameObject.SetActive(false);
        _importBtn.gameObject.SetActive(false);
        _configBtn.gameObject.SetActive(false);
        _exportBtn.gameObject.SetActive(false);
    }

    /// <summary>
    /// 有效化
    /// </summary>
    private void ValidOtherBtn()
    {
        _recordBtn.gameObject.SetActive(true);
        _importBtn.gameObject.SetActive(true);
        _configBtn.gameObject.SetActive(true);
        _exportBtn.gameObject.SetActive(true);
    }


    private void BeginDraw()
    {
        if (_isDraw)
        {
            if (!_isPlay)
            {
                _playBtn.GetComponent<Image>().sprite = PlayOrPuseImage[0];
                _isPlay = true;
                for (int i = 0; i <= ReceiveData.ChannelCount - 1; i++)
                {
                    transform.parent.Find("Channel").GetChild(i).GetComponentInChildren<AudioVisualization>().ContinueDrawData();
                }
            }
            else if (_isPlay)
            {
                _playBtn.GetComponent<Image>().sprite = PlayOrPuseImage[1];
                _isPlay = false;
                for (int i = 0; i <= ReceiveData.ChannelCount - 1; i++)
                {
                    transform.parent.Find("Channel").GetChild(i).GetComponentInChildren<AudioVisualization>().PauseDrawData();
                }
            }
        }
        else
        {
            int num = int.Parse(PlayerPrefs.GetString(ConstTable.Instance.R_P_SerialChannelCount));
            if (!_isPlay)
            {
                _playBtn.GetComponent<Image>().sprite = PlayOrPuseImage[0];
                _isPlay = true;
                for (int i = 0; i <= num - 1; i++)
                {
                    transform.parent.Find("Channel").GetChild(i).GetComponentInChildren<AudioVisualization>().ContinueConnectData();
                }
            }
            else if (_isPlay)
            {
                _playBtn.GetComponent<Image>().sprite = PlayOrPuseImage[1];
                _isPlay = false;
                for (int i = 0; i <= num - 1; i++)
                {
                    transform.parent.Find("Channel").GetChild(i).GetComponentInChildren<AudioVisualization>().PauseConnectData();
                }
            }
        }
    }

    private void ExitVideo()
    {
        if (_isDraw)
        {
            _isDraw = false;
            _playBtn.GetComponent<Image>().sprite = PlayOrPuseImage[0];
            _isPlay = false;
            ValidOtherBtn();
            StopAllCoroutines();
            for (int i = 0; i <= ReceiveData.ChannelCount - 1; i++)
            {
                transform.parent.Find("Channel").GetChild(i).GetComponentInChildren<AudioVisualization>().StopDrawData();
            }
        }
    }

    private void RefreshVideo()
    {
        if (_isDraw)
        {
            _playBtn.GetComponent<Image>().sprite = PlayOrPuseImage[1];
            _isPlay = false;
            for (int i = 0; i <= ReceiveData.ChannelCount - 1; i++)
            {
                transform.parent.Find("Channel").GetChild(i).GetComponentInChildren<AudioVisualization>().RefreshDrawData();
            }
        }
    }
}