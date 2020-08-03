/****************************************************
    文件：UnitImpulseReact.cs
	作者：Ling
    邮箱: 1759147969@qq.com
    日期：2020.07.29 11.42.31
	功能：Nothing
*****************************************************/

using UnityEngine;
using System;

public class UnitImpulseReact 
{
    private double Wc;
    private int alpha;
    private int N;

    public UnitImpulseReact(double Wp, double Ws, int N)
    {
        this.Wc = 0.5 * (Wp + Ws);
        if (N % 2 != 0)
        {
            this.alpha = (N - 1) / 2;
        }
        else
        {
            this.alpha = N / 2;
        }
        this.N = N;
    }

    public double[] GetDaiTong(double Wl, double Wh)
    {
        double[] hd = new double[N];
        for (int n = 0; n < N; n++)
        {
            double numerator = Math.Sin((n - alpha) * Wh) - Math.Sin((n - alpha) * Wl);
            double denominator = Math.PI * (n - alpha);
            if (n == alpha)
            {
                hd[n] = (Wh - Wl) / Math.PI;
            }
            else
            {
                hd[n] = numerator / denominator;
            }
        }
        return hd;
    }
}
