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
    private readonly int R_audioDataLength = 200;
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
    private bool _isShowChangeBtn;       //是否显示上下的缩放按钮

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
        _linePointZ = 100;
        //根据选择的通道总数和当前所在下标，确定位置
        _channelIndex = int.Parse(transform.parent.name[transform.parent.name.Length - 1].ToString());
        int count = int.Parse(PlayerPrefs.GetString(ConstTable.Instance.R_P_SerialChannelCount));
        count = 6;
        int middle = Mathf.CeilToInt(count / 2f);
        _offsetY = 0.8f + (1.2f * (_channelIndex <= middle ? middle - _channelIndex : middle - _channelIndex));
        //_offsetY = 0;
        GameObject.Find(ConstTable.Instance.R_TextInfo).GetComponent<Text>().text = "";


        _scaleFactor = 1;
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
        transform.parent.Find(R_Img).transform.position = new Vector3(0, parentLine.GetPosition(0).y,0);
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
        StartCoroutine("Fixed");
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
        if (!_isShowChangeBtn)
        {
            _isShowChangeBtn = !_isShowChangeBtn;
            _plusBtn.gameObject.SetActive(true);
            _minusBtn.gameObject.SetActive(true);
        }
        else
        {
            _isShowChangeBtn = !_isShowChangeBtn;
            _plusBtn.gameObject.SetActive(false);
            _minusBtn.gameObject.SetActive(false);
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
            if (_scaleFactor < 6.12f)  //6.12大约是1.2^10
                _scaleFactor *= 1.2f;
        }
        else
        {
            if (_scaleFactor > 0.15)   // 1/1.2...
                _scaleFactor /= 1.2f;
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
            double[] OtherDataZH = ShowDaiTong();
            int[] DataListZH = DealPortData(OtherDataZH).ToArray();
 
            //Debug.Log(IntervalSliderText.text + " " + OtherDataZH.Length + " " + DataListZH.Length+" "+_channelIndex);
            int interval = 10;     //每次10个点向左移动
            float rightPos = 6.4f;
            for (int i = 0; i <= DataListZH.Length - 1; i+=10)
            {
                if (DataListZH.Length - 1 - i >= 10)
                    interval = 10;
                else
                    interval = DataListZH.Length - 1 - i;

                float intervalX = (1000f - IntervalSlider.value + 1) / 1000f;
                IntervalSliderText.text = (int)IntervalSlider.value + "ms";

            //Debug.Log(_parentStartPosY+" "+ _offsetY+" "+ _linePointPosY[0] * _scaleFactor);
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
                    float posX = rightPos - (R_audioDataLength - 1 - k) * intervalX;
                    _linePointPosY[k] = Mathf.Clamp((DataListZH[i + flagIndex]) / 10f, -100, 100);
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
            B = new double[3] { 0.96897915136010271, -1.5678412012906751, 0.96897915136010271 };
            A = new double[3] { 1, -1.5678412012906751, 0.93795830272020542 };
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
            //Debug.Log(DataListZH[DataListZH.Count - 1] + " " + D3[i] + " "  + receiveDataList[i] + " " + M[0]);

        }
        ReceiveData.DealDataList[_channelIndex].AddRange(DataListZH);
        //Debug.Log(DataListZH.Count + " " + ReceiveData.DealDataList.Count);
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
        int[] data = new int[ReceiveData.ReceiveDataList[_channelIndex].Count];
        string str = "";
        string str2 = "";
;        for (int i = 0;i <= data.Length - 1; i++)
        {
            data[i] = ReceiveData.ReceiveDataList[_channelIndex][i];
            //str += data[i] + " ";
        }
        //Debug.Log("de:"+str);
        ReceiveData.ReceiveDataList[_channelIndex].Clear();
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
}