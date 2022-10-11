/*----------------------------------------------------------------
// Copyright (C) Suzhou HYC Technology Co.,LTD
// 版权所有。
//
// =================================
// CLR版本 ：4.0.30319.42000
// 命名空间 ：DotNEToolkit
// 文件名称 ：DbConnectionString.cs
// =================================
// 创 建 者 ：hyc-zyf
// 创建日期 ：2022/10/10 17:28:14
// 功能描述 ：
// 使用说明 ：
//
//
// 创建标识：hyc-zyf-2022/10/10 17:28:14
//
// 修改标识：
// 修改描述：
//
// 修改标识：
// 修改描述：
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit
{
    /// <summary>
    /// 定义各种数据库连接字符串模板
    /// </summary>
    public static class DbConnectionString
    {
        /// <summary>
        /// Mysql格式的数据库连接字符串模板
        /// </summary>
        public const string MysqlFormat = "Server={0};Database={1};Uid={2};Pwd={3};";
    }
}
