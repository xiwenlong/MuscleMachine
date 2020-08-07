/****************************************************
    文件：AudioVisualization.cs
	作者：Ling
    邮箱: 1759147969@qq.com
    日期：2020.07.16 14.13.43
	功能：Nothing
*****************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class AudioVisualization : MonoBehaviour
{
    //private readonly int R_linePoints = 64;             //一次显示64个点
    private readonly int R_audioDataLength = 14000;
    private readonly string R_Img = "Img";
    private readonly string R_ImgArrow = "Img/Img_Arrow";
    private readonly string R_ImgPlus = "Img/Img_Plus";
    private readonly string R_ImgMinus = "Img/Img_Minus";

    public Slider IntervalSlider;
    public Text IntervalSliderText;

    private int _channelIndex;
    private float _scaleFactor;             //缩放系数
    private float _parentStartPosY;      //父物体射线起始y值
    private float _offsetY;
    private AudioSource _audioSource;    //声源 
    private LineRenderer _linerenderer;  //画线  
    private Thread _dealThread;
    private bool _isDealData;
    private bool _isDraw;       //是否显示上下的缩放按钮

    private float[] _linePointPosY;       //保存每一个点的原始y值
    //private float _startLinePointY;
    private float _linePointZ;

    private Button _arrowBtn;
    private Button _plusBtn;
    private Button _minusBtn;

    private void Start()
    {
        //_parentStartPosY = transform.parent.GetComponent<LineRenderer>().GetPosition(0).y;
        _arrowBtn = transform.parent.Find(R_ImgArrow).GetComponent<Button>();
        _plusBtn = transform.parent.Find(R_ImgPlus).GetComponent<Button>();
        _minusBtn = transform.parent.Find(R_ImgMinus).GetComponent<Button>();
        _arrowBtn.onClick.AddListener(() => ShowOrHideChangeBtn());
        _plusBtn.onClick.AddListener(() => ChangeScaleFactor(1));
        _minusBtn.onClick.AddListener(() => ChangeScaleFactor(-1));
        _plusBtn.gameObject.SetActive(false);
        _minusBtn.gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        _isDraw = false;
        _linePointZ = 100;
        //根据选择的通道总数和当前所在下标，确定位置
        _channelIndex = int.Parse(transform.parent.name[transform.parent.name.Length - 1].ToString());
        int count = int.Parse(PlayerPrefs.GetString(ConstTable.Instance.R_P_SerialChannelCount));
        count = 6;
        int middle = Mathf.CeilToInt(count / 2f);
        _offsetY = 1.2f + (1.2f * (_channelIndex <= middle ? middle - _channelIndex : middle - _channelIndex));
        //_offsetY = 0;
        GameObject.Find(ConstTable.Instance.R_TextInfo).GetComponent<Text>().text = "";

        _scaleFactor = 1 /1.2f/1.2f/1.2f/1.2f;
        _linePointPosY = new float[R_audioDataLength];
        _audioSource = GetComponent<AudioSource>();//获取声源组件   
        _linerenderer = GetComponent<LineRenderer>();//获取画线组件 
        _linerenderer.positionCount = R_audioDataLength;//设定线段的片段数量 

        //加载线条的材质
        string[] colorName = PlayerPrefs.GetString(ConstTable.Instance.R_P_SerialChannelColor).Split(',');
        _linerenderer.material = Resources.Load<Material>(ConstTable.Instance.R_Material + "/" + colorName[_channelIndex - 1]);

        //画出基准线
        LineRenderer parentLine = transform.parent.GetComponent<LineRenderer>();
        parentLine.SetPosition(0, new Vector3(parentLine.GetPosition(0).x, _parentStartPosY + _offsetY, parentLine.GetPosition(0).z));
        parentLine.SetPosition(1, new Vector3(parentLine.GetPosition(1).x, _parentStartPosY + _offsetY, parentLine.GetPosition(1).z));
        parentLine.material = Resources.Load<Material>(ConstTable.Instance.R_Material + "/" + colorName[_channelIndex - 1]);
        transform.parent.Find(R_Img).transform.position = new Vector3(0, parentLine.GetPosition(0).y, 0);
        //将脚本所挂载的gameobject向左移动，使得生成的物体中心正对摄像机 
        transform.position = new Vector3(-R_audioDataLength * 0.5f * 0.05f, transform.position.y, transform.position.z);


        float intervalX = IntervalSlider.value / 100f;
        //初始化所有点的起始位置
        for (int i = 0; i <= R_audioDataLength - 1; i++)
        {
            _linePointPosY[i] = _parentStartPosY + _offsetY;
            Vector3 startPos = new Vector3(transform.position.x + i * intervalX, _linePointPosY[i], _linePointZ);
            _linerenderer.SetPosition(i, startPos);
        }
        if (ConnectPort.Instance.IsReceiveData)
        {
            StartCoroutine("Fixed");
        }

        GetDaiTongData();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnDestroy()
    {
        _dealThread?.Abort();
        _dealThread = null;
    }

    private void ShowOrHideChangeBtn()
    {
        //if (!_isShowChangeBtn)
        //{
            //_isShowChangeBtn = !_isShowChangeBtn;
            HideAllArrow();
            _plusBtn.gameObject.SetActive(true);
            _minusBtn.gameObject.SetActive(true);
        //}
        //else
        //{
        //    _isShowChangeBtn = !_isShowChangeBtn;
        //    _plusBtn.gameObject.SetActive(false);
        //    _minusBtn.gameObject.SetActive(false);
        //}
    }

    private void HideAllArrow()
    {
        for(int i = 0;i <= transform.parent.parent.childCount - 1; i++)
        {
            transform.parent.parent.GetChild(i).Find(R_ImgPlus).gameObject.SetActive(false);
            transform.parent.parent.GetChild(i).Find(R_ImgMinus).gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 波形图放大缩小
    /// </summary>
    /// <param name="factor"></param>
    private void ChangeScaleFactor(int factor)
    {
        if (factor == 1)
        {
            DrawLineInfoInPause();
            //if (_scaleFactor < 6.12f)  //6.12大约是1.2^10
                _scaleFactor *= 1.2f;
        }
        else
        {
            DrawLineInfoInPause();
            //if (_scaleFactor > 0.15)   // 1/1.2...
                _scaleFactor /= 1.2f;
        }
    }

    /// <summary>
    /// 暂停状态下重绘
    /// </summary>
    private void DrawLineInfoInPause()
    {
        //不在画
        if (!_isDraw && _linerenderer.positionCount != 0)
        {
            float rightPos = 6.4f;
            for (int k = 0; k <= R_audioDataLength - 1; k++)
            {
                float intervalX = (1000f - IntervalSlider.value + 1) / 1000f;
                IntervalSliderText.text = (int)IntervalSlider.value + "ms";

                float posX = rightPos - (R_audioDataLength - 1 - k) * intervalX;
                //_linePointPosY[k] = (float)(DataListZH[i]) / 2f;

                //flagIndex = (flagIndex + 1) % (interval + 1);
                Vector3 cubePos = new Vector3(posX,
                    _parentStartPosY + _linePointPosY[k] * _scaleFactor + _offsetY,
                    _linerenderer.GetPosition(k).z);
                //画线 
                _linerenderer.SetPosition(k, cubePos);

            }
        }
    }


    /// <summary>
    /// 绘制波形
    /// </summary>
    /// <returns></returns>
    private IEnumerator Fixed()
    {
        while (ConnectPort.Instance.IsReceiveData)
        {
            _isDraw = true;
            //double[] OtherDataZH = ShowDaiTong();
            double[] OtherDataZH = GetDaiTongData();
            int[] DataListZH = DealPortData(OtherDataZH).ToArray();
            //int[] DataListZH = DealPortData(OtherDataZH).ToArray();

            int interval = 10;     //每次10个点向左移动
            float rightPos = 6.4f;
            int drawPointLength = DataListZH.Length > 960 ? 960 : DataListZH.Length;
            for (int i = 0; i <= drawPointLength - 1; i += 10)
            {
                if (drawPointLength - 1 - i >= 10)
                    interval = 10;
                else
                    interval = drawPointLength - 1 - i;

                float intervalX = (1000f - IntervalSlider.value + 1) / 1000f;
                IntervalSliderText.text = (int)IntervalSlider.value + "ms";

                //所有点向左平移一个位置
                for (int k = 0; k <= R_audioDataLength - 2 - interval; k++)
                {
                    float posX = rightPos - (R_audioDataLength - 2 - k) * intervalX;
                    _linePointPosY[k] = _linePointPosY[k + interval];
                    Vector3 newPos = new Vector3(posX, _parentStartPosY + _linePointPosY[k] * _scaleFactor + _offsetY, _linePointZ);
                    _linerenderer.SetPosition(k, newPos);
                }
                int flagIndex = 0;
                //重新设置最右侧点的值
                for (int k = R_audioDataLength - 1 - interval; k <= R_audioDataLength - 1; k++)
                {
                    //Debug.Log("y:" + (DataListZH[i + flagIndex]) / 10f + " "+ temp[i+flagIndex]);

                    float posX = rightPos - (R_audioDataLength - 1 - k) * intervalX;
                    //_linePointPosY[k] = Mathf.Clamp((DataListZH[i + flagIndex] - 20f), -100, 100);
                    //Debug.Log((DataListZH[i + flagIndex]) + " " + (float)(DataListZH[i + flagIndex]));
                    _linePointPosY[k] = (float)(DataListZH[i + flagIndex]) / 2f ;
                    //Debug.Log((float)(DataListZH[i + flagIndex]) / 2f + " "+ (DataListZH[i + flagIndex]));
                    //Debug.Log(OtherDataZH[i + flagIndex] + " " + DataListZH[i + flagIndex]);

                    flagIndex = (flagIndex + 1) % (interval + 1);
                    Vector3 cubePos = new Vector3(posX,
                        _parentStartPosY + _linePointPosY[k] * _scaleFactor + _offsetY,
                        _linerenderer.GetPosition(k).z);
                    //画线 
                    _linerenderer.SetPosition(k, cubePos);

                }
                yield return null;
            }
            yield return null;
        }
        //清除所有线的内容
        _linerenderer.positionCount = 0;
    }

    /// <summary>
    /// 外部调用，绘制wav中读取的数据
    /// </summary>
    /// <returns></returns>
    public IEnumerator DrawWavData()
    {
        _isDraw = true;

        //Debug.Log(_linerenderer.positionCount);
        for (int j = pausePointIndex; j <= ReceiveData.ChannelCount - 1; j++)
        {
            pausePointIndex = j;
            int interval = 5;     //每次10个点向左移动
            float rightPos = 6.4f;
            //int drawPointLength = DataListZH.Length > 960 ? 960 : DataListZH.Length;
            int[] data = ReceiveData.RecordDataList[j].ToArray();

            for (int i = 0; i <= data.Length - 1; i += 10)
            {
                if (data.Length - 1 - i >= 10)
                    interval = 10;
                else
                    interval = data.Length - 1 - i;

                float intervalX = (1000f - IntervalSlider.value + 1) / 1000f;
                IntervalSliderText.text = (int)IntervalSlider.value + "ms";

                //所有点向左平移一个位置
                for (int k = 0; k <= R_audioDataLength - 2 - interval; k++)
                {
                    float posX = rightPos - (R_audioDataLength - 2 - k) * intervalX;
                    _linePointPosY[k] = _linePointPosY[k + interval];
                    Vector3 newPos = new Vector3(posX, _parentStartPosY + _linePointPosY[k] * _scaleFactor + _offsetY, _linePointZ);
                    _linerenderer.SetPosition(k, newPos);
                }
                int flagIndex = 0;
                //重新设置最右侧点的值
                for (int k = R_audioDataLength - 1 - interval; k <= R_audioDataLength - 1; k++)
                {
                    //Debug.Log("y:" + (data[i + flagIndex]) / 10f + " " + temp[i + flagIndex]);

                    float posX = rightPos - (R_audioDataLength - 1 - k) * intervalX;
                    //_linePointPosY[k] = Mathf.Clamp((data[i + flagIndex])/* / 10f*/, -100, 100);
                    _linePointPosY[k] = (float)(data[i + flagIndex]) / 2f;

                    flagIndex = (flagIndex + 1) % (interval + 1);
                    Vector3 cubePos = new Vector3(posX,
                        _parentStartPosY + _linePointPosY[k] * _scaleFactor + _offsetY,
                        _linerenderer.GetPosition(k).z);
                    //画线 
                    _linerenderer.SetPosition(k, cubePos);

                }
                yield return null;
            }
            yield return null;
        }
        ////清除所有线的内容
        //_linerenderer.positionCount = 0;
    }

    public void BeginDrawWavData()
    {
        //Debug.Log("开始");
        InitLinePoints();
        //Debug.Log(_linerenderer.positionCount);
        StartCoroutine("DrawWavData");
    }
    private int pausePointIndex = 0;  //暂停时的点下标
    public void PauseDrawData()
    {
        StopCoroutine("DrawWavData");
        _isDraw = false;

    }
    public void ContinueDrawData()
    {
        StartCoroutine("DrawWavData");
    }
    /// <summary>
    /// 停止画线，并清除所有点
    /// </summary>
    public void StopDrawData()
    {
        StopCoroutine("DrawWavData");
        pausePointIndex = 0;
        _linerenderer.positionCount = 0;
        _isDraw = false;

    }

    public void RefreshDrawData()
    {
        StopCoroutine("DrawWavData");
        pausePointIndex = 0;
        InitLinePoints();
    }

    public void InitLinePoints()
    {
        float intervalX = IntervalSlider.value / 100f;
        _linerenderer.positionCount = R_audioDataLength;//设定线段的片段数量 

        //初始化所有点的起始位置
        for (int i = 0; i <= R_audioDataLength - 1; i++)
        {
            _linePointPosY[i] = _parentStartPosY + _offsetY;
            Vector3 startPos = new Vector3(transform.position.x + i * intervalX, _linePointPosY[i], _linePointZ);
            _linerenderer.SetPosition(i, startPos);
        }
    }

    #region 带通滤波器和陷波滤波器
    /// <summary>
    /// 处理端口传输数据
    /// </summary>
    private List<int> DealPortData(double[] data)
    {
        List<double> receiveDataList = new List<double>(data);

        double[] D = new double[receiveDataList.Count];
        for (int i = 0; i <= receiveDataList.Count - 1; i++)
        {
            double m = receiveDataList[i];
            D[i] = m;
        }

        int[] D2 = new int[receiveDataList.Count];

        //500采样频率的50Hz陷波
        int hz = int.Parse(PlayerPrefs.GetString(ConstTable.Instance.R_P_NotchFilter));
        double[] B;
        double[] A;
        if (hz == 50)
        {
            B = new double[3] { 0.998403955733781378611979562265332788229, -1.995822607302023987685402062197681516409, 0.998403955733781378611979562265332788229 };
            A = new double[3] { 1, -1.995822607302023987685402062197681516409, 0.996807911467562757223959124530665576458 };
        }
        else
        {
            B = new double[3] { 1, -1.369878270138394, 0.879200583685033 };
            A = new double[3] { 0.939600291842516, -1.369878270138394, 0.939600291842516 };
        }
        double W1 = 0;
        double W2 = 0;
        double W3 = 0;

        for (int i = 0; i <= receiveDataList.Count - 1; i++)
        {
            W1 = A[0] * D[i] - A[1] * W2 - A[2] * W3;
            double s = Math.Floor(B[0] * W1 + B[1] * W2 + B[2] * W3);
            D2[i] = (int)s;
            W3 = W2;
            W2 = W1;
        }
        W1 = 0;
        W2 = 0;
        W3 = 0;

        int[] D3 = D2;
        for (int i = 2; i <= receiveDataList.Count - 4; i++)
        {
            int s = D2[i - 2] + D2[i - 1] + D2[i] + D2[i + 1] + D2[i + 2];
            D3[i] = s / 5;
        }

        List<int> DataListZH = new List<int>();
        //数据入组
        for (int i = 0; i < D3.Length; i++)
        {
            byte[] M = BitConverter.GetBytes(Mathf.Abs(D3[i]));
            if (D3[i] < 0)
                DataListZH.Add(-M[0]);
            else
                DataListZH.Add(M[0]);
            //Debug.Log(M[0] + " " + DataListZH[DataListZH.Count - 1] + " " + data[i]);
            //Debug.Log(DataListZH[DataListZH.Count - 1] / 10f + " " + D3[i] + " " + receiveDataList[i] + " " + M[0] /*+ " " + ((i < temp.Count) ? temp[i] : -10000)*/);
        }
        ReceiveData.DealDataList[_channelIndex - 1].AddRange(DataListZH);

        if (MovieRecord.Instance != null && MovieRecord.Instance._isRecordScreen)
        {
            ReceiveData.RecordDataList[_channelIndex - 1].AddRange(DataListZH);
        }
        return DataListZH;
    }



    private double[] ShowDaiTong()
    {
        //获得指定通道的数据
        //Debug.Log("data0:" + ReceiveData.ReceiveDataList[0].Count);
        //Debug.Log("data1:" + ReceiveData.ReceiveDataList[1].Count);
        //Debug.Log("data2:" + ReceiveData.ReceiveDataList[2].Count);
        //Debug.Log("data3:" + ReceiveData.ReceiveDataList[3].Count);
        //Debug.Log("data4:" + ReceiveData.ReceiveDataList[4].Count);
        //Debug.Log("data5:" + ReceiveData.ReceiveDataList[5].Count);
        int[] data = new int[ReceiveData.ReceiveDataList[_channelIndex - 1].Count];
        //string str = "";
        //string str2 = "";
        ; for (int i = 0; i <= data.Length - 1; i++)
        {
            data[i] = ReceiveData.ReceiveDataList[_channelIndex - 1][i];
            //str += data[i] + " ";
        }
        //Debug.Log("de:"+str);
        ReceiveData.ReceiveDataList[_channelIndex - 1].Clear();
        //double[] g = GetFilterDaiTong(0.03 * Math.PI, 0.05 * Math.PI, out int num);
        float left = float.Parse(PlayerPrefs.GetString(ConstTable.Instance.R_P_BandPassFilter).Split(',')[0]);
        float right = float.Parse(PlayerPrefs.GetString(ConstTable.Instance.R_P_BandPassFilter).Split(',')[1]);
        double[] g = GetFilterDaiTong(left / 5000 * Math.PI, right / 5000 * Math.PI, out int num);
        //double[] g = GetFilterDaiTong(0.01 * Math.PI, 10 * Math.PI, out int num);
        double[] res = Convolution(data, g);

        //Debug.Log(res.Length+" "+g.Length);
        //for(int i = 0; i <= data.Length - 1; i++)
        //{
        //    str += data[i] + " ";
        //    str2 += res[i] + " ";
        //    //Debug.Log("data:" + data[i] + " " + res[i]);
        //}
        //Debug.Log("da:" + str);
        //Debug.Log("re:" + str2);
        return res;
    }
    /// <summary>
    /// 以汉宁窗的方式获得带通滤波结果
    /// </summary>
    /// <param name="Wp"></param>
    /// <param name="Ws"></param>
    /// <param name="n"></param>
    /// <returns></returns>
    public double[] GetFilterDaiTong(double Wp, double Ws, out int n)
    {
        Hanning win = new Hanning(Wp, Ws);//构造汉宁窗
        UnitImpulseReact UIR = new UnitImpulseReact(Wp, Ws, win.N);//构造单位冲激响应
        double[] hd = new double[win.N];
        double[] w = new double[win.N];
        double[] h = new double[win.N];
        string[] res = new string[win.N];//定义数组
        w = win.GetWin();//获取窗函数序列
        hd = UIR.GetDaiTong(Wp, Ws);//获取单位冲激响应序列
        for (int i = 0; i < win.N; i++)
        {
            h[i] = w[i] * hd[i];//得到滤波器序列
        }
        n = win.N;
        return h;
    }

    /// <summary>
    /// 卷积
    /// </summary>
    /// <param name="f"></param>
    /// <param name="g"></param>
    /// <returns></returns>
    public double[] Convolution(int[] f, double[] g)
    {
        int n = f.Length;
        int k = g.Length;
        double[] res = new double[n + k - 1];

        for (int i = 0; i < n + k - 1; i++)
        {
            double t = 0.0;
            for (int j = 0; j < f.Length; j++)
            {
                double temp = 0.0;
                //decimal temp = new decimal("0");
                if (i - j < 0 || i - j >= g.Length)
                {
                    temp = 0.0;
                }
                else
                {
                    temp = f[j] * g[i - j];
                    //Debug.Log(g[i - j] + " " + f[j] + " " + f[j] * (float)Math.Round(g[i - j]));
                }
                t = t + temp;
                //Debug.Log(t + " " + temp);
            }
            res[i] = t;
            //Debug.Log(res[i]);
        }
        return res;
    }
    #endregion




    #region 重写带通滤波器

    private double[] GetDaiTongData()
    {
        int i, j, n2, n, band, wn;
        double fl, fh, fs/*, freq*/;
        double[] h=new double[100], c = new double[100], x = new double[300], y = new double[300];
        c[1] = 0.0;

        float left = float.Parse(PlayerPrefs.GetString(ConstTable.Instance.R_P_BandPassFilter).Split(',')[0]);
        float right = float.Parse(PlayerPrefs.GetString(ConstTable.Instance.R_P_BandPassFilter).Split(',')[1]);
        fs = 10000;
        fl = left / fs;   //通带下边界频率
        fh = right / fs;//通带上边界频率
        //fs = 1000;
        //fl = 125 /fs;
        //fh = 300 / fs;
        n = 32;
        band = 3;
        wn = 4;

        firWin(n, band, fl, fh, wn, ref h);
        //n2 = n / 2;
        //for (i = 0; i <= n; i++)
        //{
        //    //j = n - i;
        //    Debug.Log("h[" + i + "]=" + h[i] /*+ "=h(" + j + ")"*/);
        //}
        //return null;
        return CalcFIR(h, n);
        //gain(h, c, n, 1,ref x,ref y, 300, 2);
        //for(i = 0;i < 300; i++)
        //{
        //    freq = 0.5 * i / 299;
        //    Debug.Log(freq +" "+ x[i]);
        //}
    }
    public double[] CalcFIR(double[] filterParameters,int n)
    {
        //传输进来的数据
        int[] dataSerial = ReceiveData.ReceiveDataList[_channelIndex - 1].ToArray();
        //int[] dataSerial = new int[] { 0, 0, 0, 83, 0, 83, 0, 83, 0, 82, 0, 82, 0, 82, 0, 82, 0, 81, 0, 81, 0, 81, 0, 81, 0, 80, 0, 80, 0, 79, 0, 79, 0, 79, 0, 79, 0, 79, 0, 79, 0, 78, 0, 78, 0, 78, 0, 78, 0, 77, 0, 76, 0, 76, 0, 76, 0, 76, 0, 76, 0, 75, 0, 75, 0, 75, 0, 75, 0, 74, 0, 74, 0, 73, 0, 73, 0, 73, 0, 73, 0, 72, 0, 71, 0, 71, 0, 71, 0, 71, 0, 71 };
        //
        //int order = filterParameters.Length;
        //滤波后的数据
        double[] FIRResult = new double[dataSerial.Length];

        string str = "";
        string str2 = "";

        //Debug.Log(n);
        for (int i = 0; i < dataSerial.Length; i++)
        {
            double sum = 0;
            str = "";
            if (i < n)
            {
                for (int j = 0; j < i; j++)
                {
                    sum += filterParameters[j] * dataSerial[i - j];
                    //Debug.Log
                    //str += sum + " ";
                    //if (i == 19)
                    //    Debug.Log(filterParameters[j] + " " + dataSerial[i - j] + " " +(filterParameters[j] * dataSerial[i - j]));
                } 
            }
            else
            {
                for (int j = 0; j < n; j++)
                {
                    sum += filterParameters[j] * dataSerial[i - j];
                }
            }
            FIRResult[i] = sum;
            //Debug.Log("i:" + i + " 原值：" + dataSerial[i] + " 滤波：" + sum + " 变换："+ str);
        }
        //for(int i = 0;i < FIRResult.Length; i++)
        //{
        //    str += dataSerial[i] + ",";
        //    str2 += FIRResult[i] + " ";
        //}
        //Debug.Log("原始：" + str);
        //Debug.Log("滤波：" + str2);
        return FIRResult;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="b">存放滤波器分子多项式的系数b(i)</param>
    /// <param name="a">存放滤波器分子多项式的系数a(i)</param>
    /// <param name="m">滤波器分子阶数</param>
    /// <param name="n">滤波器分母阶数</param>
    /// <param name="x">长度len，存放实部</param>
    /// <param name="y">长度len，存放虚部</param>
    /// <param name="len">频率响应长度</param>
    /// <param name="sign">实部和虚部</param>
    public void gain(double[] b, double[] a, int m, int n,ref double[] x,ref double[] y, int len, int sign)
    {
        int i, k;
        double ar, ai, br, bi, zr, zi, im, re, den, numr, numi, freq, temp;
        for (k = 0; k < len; k++)
        {
            freq = k * 0.5 / (len - 1);
            zr = Math.Cos(-8.0 * Math.Atan(1.0) * freq);
            zi = Math.Sin(-8.0 * Math.Atan(1.0) * freq);

            br = 0.0;
            bi = 0.0;
            for (i = m; i > 0; i--)
            {
                re = br;
                im = bi;
                br = (re + b[i]) * zr - im * zi;
                bi = (re + b[i]) * zi + im * zr;
            }

            ar = 0.0;
            ai = 0.0;
            for (i = n; i > 0; i--)
            {
                re = ar;
                im = ai;
                ar = (re + a[i]) * zr - im * zi;
                ai = (re + a[i]) * zi + im * zr;
            }

            br = br + b[0];
            ar = ar + 1.0;
            numr = ar * br + ai * bi;
            numi = ar * bi - ai * br;
            den = ar * ar + ai * ai;
            x[k] = numr / den;
            y[k] = numi / den;

            switch (sign)
            {
                case 1:
                    {
                        temp = Math.Sqrt(x[k] * x[k] + y[k] * y[k]);
                        y[k] = Math.Atan2(y[k], x[k]);
                        x[k] = temp;
                        break;
                    }
                case 2:
                    {
                        temp = x[k] * x[k] + y[k] * y[k];
                        y[k] = Math.Atan2(y[k], x[k]);
                        x[k] = 10.0 * Math.Log10(temp);
                        break;
                    }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="n">滤波器阶数</param>
    /// <param name="band">滤波器类型，1,2,3,4 -> 低通，高通，带通，带阻</param>
    /// <param name="fln">变量</param>
    /// <param name="fhn">变量</param>
    /// <param name="wn">窗函数类型 矩形，图基窗，三角窗，汉宁窗，海明窗，布拉克曼窗，凯塞窗</param>
    /// <param name="h">存放FIR滤波器系数</param>
    private void firWin(int n,int band,double fln,double fhn,int wn,ref double[] h)
    {
        int i, n2, mid;
        double s, pi, wc1, wc2=0, beta, delay;
        beta = 0.0;

        if (wn == 7) //凯塞窗
        {
            //自己输入beta
            beta = 4;
        }

        pi = 4.0 * Math.Atan(1.0);
        //如果阶数是偶数，则窗口长度为奇数
        if ((n % 2) == 0)
        {
            n2 = n / 2 - 1;
            mid = 1;
        }
        else
        {
            n2 = n / 2;
            mid = 0;
        }

        delay = n / 2.0;
        wc1 = 2.0 * pi * fln;
        if (band >= 3) wc2 = 2.0 * pi * fhn;

        switch (band)
        {
            case 1:  //低通
                {
                    for(i = 0; i <= n2; i++)
                    {
                        s = i - delay;
                        h[i] = (Math.Sin(wc1 * s) / (pi * s)) * Window(wn, n + 1, i, beta);
                        h[n - i] = h[i];
                    }
                    if (mid == 1) h[n /2] = wc1 / pi;
                    break;
                }
            case 2:  //高通
                {
                    for(i = 0; i <= n2; i++)
                    {
                        s = i - delay;
                        h[i] = (Math.Sin(pi*s)-Math.Sin(wc1*s))/(pi*s);
                        h[i] = h[i] * Window(wn, n + 1, i, beta);
                        h[n - i] = h[i];
                    }
                    if (mid == 1) h[n / 2] = 1.0 - wc1 / pi;
                    break;
                }
            case 3:  //带通
                {
                    for(i = 0; i <= n2; i++)
                    {
                        s = i - delay;
                        h[i] = (Math.Sin(wc2 * s) - Math.Sin(wc1 * s)) / (pi * s);
                        h[i] = h[i] * Window(wn, n + 1, i, beta);
                        h[n - i] = h[i];
                    }
                    if (mid == 1) h[n / 2] = (wc2 - wc1) / pi;
                    break;
                }
            case 4:  //带阻
                {
                    break;
                }
        }
    }

    private double Window(int type,int n,int i,double beta)
    {
        int k;
        double pi, w;
        pi = 4.0 * Math.Atan(1.0);
        w = 1.0;

        switch (type)
        {
            case 1:
                {
                    w = 1.0;
                    break;
                }
            case 2:
                {
                    k = (n - 2) / 10;
                    if (i <= k)
                        w = 0.5 * (1.0 - Math.Cos(i * pi / (k + 1)));
                    if (i > n - k - 2)
                        w = 0.5 * (1.0 - Math.Cos((n - i - 1) * pi / (k + 1)));
                    break;
                }
            case 3:
                {
                    w = 1.0 - Math.Abs(1.0 - 2 * i / (n - 1.0));
                    break;
                }
            case 4:
                {
                    w = 0.5 * (1.0 - Math.Cos(2 * i * pi / (n - 1)));
                    break;
                }
            case 5:
                {
                    w = 0.54 - 0.46 * Math.Cos(2 * i * pi / (n - 1));
                    break;
                }
            case 6:
                {
                    w = 0.42 - 0.5 * Math.Cos(2 * i * pi / (n - 1)) + 0.08 * Math.Cos(4 * i * pi / (n - 1));//布莱克曼窗
                    break;
                }
            case 7:
                {
                    w = kaiser(i, n, beta);//凯塞窗
                    break;
                }
        }
        return w;
    }

    private double kaiser(int i,int n,double beta)
    {
        double a, w, a2, b1, b2, beta1;



        b1 = bessel0(beta);

        a = 2.0 * i / (double)(n - 1) - 1.0;

        a2 = a * a;

        beta1 = beta * Math.Sqrt(1.0 - a2);

        b2 = bessel0(beta1);

        w = b2 / b1;

        return (w);
    }

    private double bessel0(double x)
    {
        int i;

        double d, y, d2, sum;

        y = x / 2.0;

        d = 1.0;

        sum = 1.0;

        for (i = 1; i <= 25; i++)

        {
            d = d * y / i;

            d2 = d * d;

            sum = sum + d2;

            if (d2 < sum * (1.0e-8)) break;

        }

        return (sum);
    }
    #endregion

}