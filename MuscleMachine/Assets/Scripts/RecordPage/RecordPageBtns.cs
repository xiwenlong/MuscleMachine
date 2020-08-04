﻿/****************************************************
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

public class RecordPageBtns : MonoBehaviour
{
    public Sprite[] PlayOrPuseImage;
    public GameObject VideoImage;

    public Button _configBtn;
    public Button _exportBtn;
    public Button _recordBtn;
    public Button _importBtn;
    public Button _playBtn;
    public Button _refreshBtn;
    public Button _exitBtn;
    private VideoPlayer _videoPlayer;
    private RawImage _rawImage;
    private bool _isPlay;

    private void Start()
    {
        _isPlay = false;
        _configBtn = transform.Find(ConstTable.Instance.R_configBtn).GetComponent<Button>();
        _exportBtn = transform.Find(ConstTable.Instance.R_exportBtn).GetComponent<Button>();
        _recordBtn = transform.Find(ConstTable.Instance.R_recordBtn).GetComponent<Button>();
        _importBtn = transform.Find(ConstTable.Instance.R_importBtn).GetComponent<Button>();
        _playBtn = transform.Find(ConstTable.Instance.R_PlayBtn).GetComponent<Button>();
        _refreshBtn = transform.Find(ConstTable.Instance.R_RefreshBtn).GetComponent<Button>();
        _exitBtn = transform.Find(ConstTable.Instance.R_Exit).GetComponent<Button>();

        _configBtn.onClick.AddListener(() => JumptIntoConfigPage());
        _importBtn.onClick.AddListener(() => ImportMovie());
        _playBtn.onClick.AddListener(() => PlayVideo());
        _refreshBtn.onClick.AddListener(() => RefreshVideo());
        _exitBtn.onClick.AddListener(() => ExitVideo());

        _videoPlayer = VideoImage.GetComponent<VideoPlayer>();
        _rawImage = VideoImage.GetComponent<RawImage>();
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
        pth.filter = "MP4 files (*.mp4)\0*.mp4";
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
            _videoPlayer.url = filePath;
            InvalidOtherBtn();
            VideoImage.SetActive(true);
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
        for (int i = 0; i <= transform.parent.childCount - 1; i++)
        {
            transform.parent.GetChild(i).gameObject.SetActive(false);
        }
        gameObject.SetActive(true);
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
        for (int i = 0; i <= transform.parent.childCount - 1; i++)
        {
            transform.parent.GetChild(i).gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (_videoPlayer.texture == null) return;

        _rawImage.texture = _videoPlayer.texture;
    }

    private void PlayVideo()
    {
        if (VideoImage.activeSelf == true && !_isPlay)
        {
            _videoPlayer.Play();
            _playBtn.GetComponent<Image>().sprite = PlayOrPuseImage[1];
            _isPlay = !_isPlay;
        }
        else if(VideoImage.activeSelf == true && _isPlay)
        {
            _videoPlayer.Pause();
            _playBtn.GetComponent<Image>().sprite = PlayOrPuseImage[0];
            _isPlay = !_isPlay;
        }
    }

    private void ExitVideo()
    {
        if (VideoImage.activeSelf == true)
        {
            ValidOtherBtn();
            VideoImage.SetActive(false);
        }
    }

    private void RefreshVideo()
    {
        if (VideoImage.activeSelf == true)
            _videoPlayer.time = 0;
    }
}