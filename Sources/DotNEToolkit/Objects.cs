/*----------------------------------------------------------------
// Copyright (C) Suzhou HYC Technology Co.,LTD
// 版权所有。
//
// =================================
// CLR版本 ：4.0.30319.42000
// 命名空间 ：DotNEToolkit
// 文件名称 ：Objects.cs
// =================================
// 创 建 者 ：hyc-zyf
// 创建日期 ：2022/7/7 17:12:05
// 功能描述 ：
// 使用说明 ：
//
//
// 创建标识：hyc-zyf-2022/7/7 17:12:05
//
// 修改标识：
// 修改描述：
//
// 修改标识：
// 修改描述：
//----------------------------------------------------------------*/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit
{
    /// <summary>
    /// Object类型的扩展
    /// </summary>
    public static class Objects
    {
        /// <summary>
        /// 复制一个一模一样的对象
        /// 如果被复制的对象里有引用类型，那么也会重新创建一个新的对象引用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T Copy<T>(this T source)
        {
            string json = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
