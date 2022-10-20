/*----------------------------------------------------------------
// Copyright (C) Suzhou HYC Technology Co.,LTD
// 版权所有。
//
// =================================
// CLR版本 ：4.0.30319.42000
// 命名空间 ：DotNEToolkitConsole
// 文件名称 ：TestExcel.cs
// =================================
// 创 建 者 ：hyc-zyf
// 创建日期 ：2022/10/20 14:50:49
// 功能描述 ：
// 使用说明 ：
//
//
// 创建标识：hyc-zyf-2022/10/20 14:50:49
//
// 修改标识：
// 修改描述：
//
// 修改标识：
// 修改描述：
//----------------------------------------------------------------*/

using DotNEToolkit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkitConsole
{
    public static class TestExcel
    {
        private static int value = 0;

        public static void CreateNew()
        {
            string excelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "1.xls");

            TableData tableData = TableData.Create();
            tableData.Set(0, 0, "0");
            tableData.Set(0, 1, "0");
            tableData.Set(0, 2, "0");
            tableData.Set(0, 3, "0");
            tableData.Set(0, 4, "0");
            tableData.Set(0, 5, "0");
            Excel.TableData2Excel(excelPath, tableData, WriteOptions.CreateNew);
        }

        public static void CreateOrAppend()
        {
            string excelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "1.xls");

            TableData tableData = TableData.Create();
            tableData.Set(0, 0, value++.ToString());
            tableData.Set(0, 1, value++.ToString());
            tableData.Set(0, 2, value++.ToString());
            tableData.Set(0, 3, value++.ToString());
            tableData.Set(0, 4, value++.ToString());
            Excel.TableData2Excel(excelPath, tableData, WriteOptions.Append);
        }
    }
}
