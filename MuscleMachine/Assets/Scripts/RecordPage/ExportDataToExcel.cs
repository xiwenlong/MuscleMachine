/****************************************************
    文件：ExportDataToExcel.cs
	作者：Ling
    邮箱: 1759147969@qq.com
    日期：2020.07.17 12.03.08
	功能：导出数据到Excel中
*****************************************************/

using System;
using System.IO;
using OfficeOpenXml;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class OpenFileName
{
    public int structSize = 0;
    public IntPtr dlgOwner = IntPtr.Zero;
    public IntPtr instance = IntPtr.Zero;
    public String filter = null;
    public String customFilter = null;
    public int maxCustFilter = 0;
    public int filterIndex = 0;
    public String file = null;
    public int maxFile = 0;
    public String fileTitle = null;
    public int maxFileTitle = 0;
    public String initialDir = null;
    public String title = null;
    public int flags = 0;
    public short fileOffset = 0;
    public short fileExtension = 0;
    public String defExt = null;
    public IntPtr custData = IntPtr.Zero;
    public IntPtr hook = IntPtr.Zero;
    public String templateName = null;
    public IntPtr reservedPtr = IntPtr.Zero;
    public int reservedInt = 0;
    public int flagsEx = 0;
}
public class LocalDialog
{
    //链接指定系统函数       打开文件对话框  
    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
    public static bool GetOFN([In, Out] OpenFileName ofn)
    {
        return GetOpenFileName(ofn);
    }
    //链接指定系统函数        另存为对话框  
    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetSaveFileName([In, Out] OpenFileName ofn);
    public static bool GetSFN([In, Out] OpenFileName ofn)
    {
        return GetSaveFileName(ofn);
    }
}

public class ExportDataToExcel : MonoBehaviour
{
    private void Start()
    {
        transform.GetComponent<Button>().onClick.AddListener(() =>
        {
            CreatExcel();
        });
    }

    private void CreatExcel()
    {
        OpenFileName openFileName = new OpenFileName();
        openFileName.structSize = Marshal.SizeOf(openFileName);
        openFileName.filter = "Excel文件(*.xlsx)\0*.xlsx";
        openFileName.file = new string(new char[256]);
        openFileName.maxFile = openFileName.file.Length;
        openFileName.fileTitle = new string(new char[64]);
        openFileName.maxFileTitle = openFileName.fileTitle.Length;
        openFileName.initialDir = Application.streamingAssetsPath.Replace('/', '\\');
        //默认路径    
        openFileName.title = "导出数据";
        openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;
        if (LocalDialog.GetSaveFileName(openFileName))
        {
            string createPath = openFileName.file + ".xlsx";
            FileInfo newFile = new FileInfo(createPath);
            if (newFile.Exists)
            {
                newFile.Delete();
                newFile = new FileInfo(createPath);
            }
            using (ExcelPackage package = new ExcelPackage(newFile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("table1");
                //创建worksheet              
                //worksheet.Column(1).Width = 30;
                //直接指定行列数进行赋值     
                //worksheet.Cells[1, 1].Value = "序号";
                for (int i = 2; i <= 7; i++)
                {
                    worksheet.Cells[1, i].Value = "CH" + (i - 1);
                }
                worksheet.Cells[1, 8].Value = "时间";
                worksheet.Cells[2, 8].Value = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

                for (int i = 0; i <= 5; i++)
                {
                    for (int j = 0; j <= ReceiveData.DealDataList[i].Count - 1; j++)
                    {
                        worksheet.Cells[j + 2, 1].Value = j + 1;
                        worksheet.Cells[j + 2, i+2].Value = ReceiveData.DealDataList[i][j];
                    }
                    ReceiveData.DealDataList[i].Clear();
                }
                //}

                ////直接指定单元格进行赋值              
                //worksheet.Cells["A2"].Value = "数据2";
                //创建worksheet           
                //ExcelWorksheet worksheet1 = package.Workbook.Worksheets.Add("table2");
                ////直接指定行列数进行赋值        
                //worksheet1.Cells[1, 1].Value = "名称";
                ////直接指定单元格进行赋值          
                //worksheet1.Cells["A2"].Value = "名称";
                package.Save();//保存excel      
            }
        }
    }
}