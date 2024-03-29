﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit.Modular
{
    /// <summary>
    /// 模块实例公共接口
    /// </summary>
    public interface IModuleInstance
    {
        /// <summary>
        /// 模块状态
        /// </summary>
        ModuleStatus Status { get; }

        /// <summary>
        /// 模块ID
        /// </summary>
        string ID { get; }

        /// <summary>
        /// 模块名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 描述
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 描述文件
        /// </summary>
        ModuleDefinition Definition { get; }

        /// <summary>
        /// 所属的工厂
        /// </summary>
        ModuleFactory Factory { get; }

        /// <summary>
        /// 初始化模块
        /// </summary>
        /// <returns></returns>
        int Initialize();

        /// <summary>
        /// 释放模块
        /// </summary>
        /// <returns></returns>
        void Release();
    }
}
