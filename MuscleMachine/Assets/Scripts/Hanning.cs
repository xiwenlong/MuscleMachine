/****************************************************
    文件：Hanning.cs
	作者：Ling
    邮箱: 1759147969@qq.com
    日期：2020.07.29 11.40.41
	功能：汉宁窗
*****************************************************/

using System;
using UnityEngine;

public class Hanning 
{
    public int N = 0;
    public Hanning(double Wp, double Ws)//wp,ws以pi为单位
    {
        int i;
        double n = (3.1 * 2 * Math.PI) / (Ws - Wp);
        for (i = 0; i < n; i++) ;
        N = i;
        //Debug.Log(Ws + " " + Wp + " " + N);
    }

    public double[] GetWin()
    {
        int n;
        double[] wd = new double[N];
        for (n = 0; n < N; n++)
        {
            double b = Math.Cos((2 * Math.PI * (double)n) / ((double)N - 1));
            double res = (0.5 - 0.5 * b);
            wd[n] = res;
        }
        return wd;
    }
}
