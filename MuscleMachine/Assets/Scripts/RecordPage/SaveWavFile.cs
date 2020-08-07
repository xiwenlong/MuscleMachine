/****************************************************
    文件：SaveWav.cs
	作者：Ling
    邮箱: 1759147969@qq.com
    日期：#CreateTime#
	功能：Nothing
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;


/**************************************************************************  

              Here is where the file will be created. A  

              wave file is a RIFF file, which has chunks  

              of data that describe what the file contains.  

              A wave RIFF file is put together like this:  

              The 12 byte RIFF chunk is constructed like this:  

              Bytes 0 - 3 :  'R' 'I' 'F' 'F'  

              Bytes 4 - 7 :  Length of file, minus the first 8 bytes of the RIFF description.  

                                (4 bytes for "WAVE" + 24 bytes for format chunk length +  

                                8 bytes for data chunk description + actual sample data size.)  

               Bytes 8 - 11: 'W' 'A' 'V' 'E'  

               The 24 byte FORMAT chunk is constructed like this:  

               Bytes 0 - 3 : 'f' 'm' 't' ' '  

               Bytes 4 - 7 : The format chunk length. This is always 16.  

               Bytes 8 - 9 : File padding. Always 1.  

               Bytes 10- 11: Number of channels. Either 1 for mono,  or 2 for stereo.  

               Bytes 12- 15: Sample rate.  

               Bytes 16- 19: Number of bytes per second.  

               Bytes 20- 21: Bytes per sample. 1 for 8 bit mono, 2 for 8 bit stereo or  16 bit mono, 4 for 16 bit stereo.  

               Bytes 22- 23: Number of bits per sample.  

               The DATA chunk is constructed like this:  

               Bytes 0 - 3 : 'd' 'a' 't' 'a'  

               Bytes 4 - 7 : Length of data, in bytes.  

               Bytes 8 -: Actual sample data.  

             ***************************************************************************/
public class SaveWavFile
{
    const int HEADER_SIZE = 44;
    private static int _dataLength;
    public static string FileName;


    public static void Save(/*string fileName, AudioClip clip*/)
    {
        //添加wav后缀
        //if (!fileName.ToLower().EndsWith(".wav"))
        //{
        //    fileName += ".wav";
        //}

        _dataLength = 0;
        //创建movie文件夹
        if (!Directory.Exists("movie"))
            Directory.CreateDirectory("movie");

        string curDir = Environment.CurrentDirectory;
        //string fileName = DateTime.Now.ToString("yyyyMMddhhmmss") + ".wav";
        FileName = curDir + "\\movie\\" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".wav";
        //1.创建头
        //FileStream fs = CreateEmpty(curDir + "\\movie\\" + fileName);
        FileStream fs = CreateEmpty(FileName);

        //2.写语音数据
        ConvertAndWrite(fs);

        //3.重写真正的文件头
        WriteHeader(fs/*, clip*/);
    }

    //创建头文件
    static FileStream CreateEmpty(string filepath)
    {
        var fileStream = new FileStream(filepath, FileMode.Create);
        byte emptyByte = new byte();

        //预留44个字节用于写数据头信息
        for (int i = 0; i < HEADER_SIZE; i++)
        {
            fileStream.WriteByte(emptyByte);
        }
        return fileStream;
    }


    static void ConvertAndWrite(FileStream fileStream/*, AudioClip clip*/)
    {
        //var samples = new float[clip.samples];
        //clip.GetData(samples, 0);

        int count = int.Parse(PlayerPrefs.GetString(ConstTable.Instance.R_P_SerialChannelCount));
        //
        for (int i = 0;i <= count - 1; i++)
        {
            var data = ReceiveData.RecordDataList[i];
            //Debug.Log("接受数据长度:"+ " " + ReceiveData.RecordDataList[i].Count);
            _dataLength += data.Count;
            Int16[] intData = new Int16[data.Count];
            Byte[] bytesData = new Byte[data.Count * 2];
            
            //int rescaleFactor = 32767;
            //string str = "";
            for (int j = 0; j <= data.Count - 1; j++)
            {
                //str += data[j] + " ";
                intData[j] = (short)(data[j] /** rescaleFactor*/);

                Byte[] byteArray = new byte[2];

                byteArray = BitConverter.GetBytes(intData[j]);

                byteArray.CopyTo(bytesData, j * 2);
            }
            //Debug.Log("i:" +i +" "+ str);
            fileStream.Write(bytesData, 0, bytesData.Length);
            ////写入Next信息，读取时可以用这个进行分割
            //Byte[] next = System.Text.Encoding.UTF8.GetBytes("NEXT");
            //fileStream.Write(next, 0, 4);
        }

        //数据全部写完以后，断开链接
        ConnectPort.Instance.DisConnect();
    }


    static void WriteHeader(FileStream fileStream/*, AudioClip clip*/)
    {
        //var hz = clip.frequency;

        //var channels = clip.channels;

        //var samples = clip.samples;

        var hz = 22050;
        var channels = 1;
        //var sa

        fileStream.Seek(0, SeekOrigin.Begin);

        //文档标识 RIFF
        Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        fileStream.Write(riff, 0, 4);


        //文件长度  不包括表示和长度的8字节
        //Debug.Log("文件长度：" + fileStream.Length);
        Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
        fileStream.Write(chunkSize, 0, 4);
        //文件格式类型  WAVE
        Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        fileStream.Write(wave, 0, 4);

        //格式块标志
        Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        fileStream.Write(fmt, 0, 4);
        //格式块长度
        Byte[] subChunk1 = BitConverter.GetBytes(16);
        fileStream.Write(subChunk1, 0, 4);
        //编码格式  通常为1
        Byte[] audioFormat = BitConverter.GetBytes(1);
        fileStream.Write(audioFormat, 0, 2);
        //声道个数
        Byte[] numChannels = BitConverter.GetBytes(channels);
        fileStream.Write(numChannels, 0, 2);
        //采样频率
        Byte[] sampleRate = BitConverter.GetBytes(hz);
        fileStream.Write(sampleRate, 0, 4);
        //数据传输速率  采样频率*声道个数*位数/8
        Byte[] byRate = BitConverter.GetBytes(hz * channels * 2);
        fileStream.Write(byRate, 0, 4);
        //采样帧大小    声道数*位数/8
        UInt16 blockAlign = (ushort)(channels * 2);
        fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);
        //采样位数  通常有 4，8，12，16，24，32
        UInt16 bps = 16;
        byte[] bitsPerSample = BitConverter.GetBytes(bps);
        fileStream.Write(bitsPerSample, 0, 2);
        //数据
        int count = int.Parse(PlayerPrefs.GetString(ConstTable.Instance.R_P_SerialChannelCount));
        string dataId = "dat" + count;
        Byte[] dataString = System.Text.Encoding.UTF8.GetBytes(dataId);
        fileStream.Write(dataString, 0, 4);
        //数据总长度？
        Byte[] subChunk2 = BitConverter.GetBytes(_dataLength * channels * 2);
        fileStream.Write(subChunk2, 0, 4);
        //写入
        fileStream.Close();
    }
}