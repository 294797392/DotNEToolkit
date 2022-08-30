using DotNEToolkit.Modular;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNEToolkit
{
    public abstract class AppManifest
    {
        /// <summary>
        /// 配置文件里的所有的列表
        /// </summary>
        [JsonProperty("modules")]
        public List<ModuleDefinition> ModuleList { get; private set; }

        public AppManifest()
        {
            this.ModuleList = new List<ModuleDefinition>();
        }
    }

    /// <summary>
    /// 封装一个基于模块化（ModuleFactory）实现的App开发框架
    /// </summary>
    /// <typeparam name="TApp"></typeparam>
    /// <typeparam name="TManifest"></typeparam>
    public abstract class ModularApp<TApp, TManifest> : SingletonObject<TApp>
        where TApp : class
        where TManifest : AppManifest
    {
        protected static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(TApp));

        private const string KEY_CONFIG_PATH = "appConfig";
        private const string DefaultConfigFileName = "app.json";

        #region 实例变量

        private string configPath;

        #endregion

        #region 属性

        /// <summary>
        /// 模块工厂
        /// </summary>
        public ModuleFactory Factory { get; private set; }

        /// <summary>
        /// App的配置文件
        /// </summary>
        public TManifest Manifest { get; private set; }

        #endregion

        /// <summary>
        /// 初始化App
        /// </summary>
        /// <returns></returns>
        public int Initialize()
        {
            #region 加载配置文件

            // 先查找exe.config文件里是否配置了配置文件的路径
            string configPath = ConfigurationManager.AppSettings.Get(KEY_CONFIG_PATH);
            if (string.IsNullOrEmpty(configPath))
            {
                // 如果exe.config文件里没配置，那么使用根目录下的app.json文件
                configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultConfigFileName);
                logger.InfoFormat("客户端没配置配置文件的路径, 使用默认路径:{0}", configPath);
            }
            else
            {
                logger.InfoFormat("使用用户配置的配置文件, 路径:{0}", configPath);
            }

            this.configPath = configPath;

            try
            {
                this.Manifest = JSONHelper.ParseFile<TManifest>(configPath);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("解析App配置文件异常, {0}", configPath), ex);
                return DotNETCode.PARSE_CONFIG_FAILED;
            }

            #endregion

            #region 加载ModuleFactory

            logger.Info("开始加载ModuleFactory...");
            this.Factory = ModuleFactory.CreateFactory();
            this.Factory.Initialized += Factory_Initialized;
            this.Factory.ModuleStatusChanged += Factory_ModuleStatusChanged;
            this.Factory.CircularReference += Factory_CircularReference;
            this.Factory.SetupModulesAsync(this.Manifest.ModuleList.Where(v => !v.HasFlag(ModuleFlags.Disabled)), 2000);

            #endregion

            return this.OnInitialize();
        }

        /// <summary>
        /// 释放App占用的资源
        /// </summary>
        public void Release()
        {
            this.Factory.Initialized -= this.Factory_Initialized;
            this.Factory.ModuleStatusChanged -= this.Factory_ModuleStatusChanged;
            this.Factory.CircularReference -= this.Factory_CircularReference;

            this.OnRelease();
        }

        /// <summary>
        /// 保存Manifest文件
        /// </summary>
        public int SaveManifest()
        {
            try
            {
                JSONHelper.Object2File<TManifest>(this.configPath, this.Manifest);
                return DotNETCode.SUCCESS;
            }
            catch (Exception ex)
            {
                logger.Error("保存配置文件异常", ex);
                return DotNETCode.FAILED;
            }
        }

        #region 受保护方法

        #endregion

        #region 事件处理器

        private void Factory_Initialized(ModuleFactory factory)
        {
            this.OnModuleInitialized();
        }

        private void Factory_ModuleStatusChanged(ModuleFactory factory, IModuleInstance moduleInst, ModuleStatus status)
        {
            this.OnModuleStatusEvent(moduleInst, status);
        }

        private void Factory_CircularReference(ModuleFactory factory, IModuleInstance moduleInst)
        {
            this.OnCircularReference(moduleInst);
        }

        #endregion

        #region 抽象方法

        /// <summary>
        /// 子类初始化
        /// </summary>
        /// <returns></returns>
        protected abstract int OnInitialize();

        /// <summary>
        /// 子类释放资源
        /// </summary>
        protected abstract void OnRelease();

        /// <summary>
        /// 处理模块状态改变事件
        /// </summary>
        /// <param name="moduleInst"></param>
        /// <param name="status">模块状态</param>
        protected abstract void OnModuleStatusEvent(IModuleInstance moduleInst, ModuleStatus status);

        /// <summary>
        /// 处理所有模块都初始化成功的事件
        /// </summary>
        protected abstract void OnModuleInitialized();

        /// <summary>
        /// 当出现模块循环引用的时候触发
        /// </summary>
        /// <param name="moduleInst">存在循环引用的模块</param>
        protected abstract void OnCircularReference(IModuleInstance moduleInst);

        #endregion
    }
}
