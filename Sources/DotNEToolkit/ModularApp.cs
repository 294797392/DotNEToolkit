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

        /// <summary>
        /// 模块工厂
        /// </summary>
        public ModuleFactory Factory { get; private set; }

        /// <summary>
        /// App的配置文件
        /// </summary>
        public TManifest Manifest { get; private set; }

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
            this.Factory.ModuleEvent += Factory_ModuleEvent;
            this.Factory.SetupModulesAsync(this.Manifest.ModuleList.Where(v => !v.HasFlag(ModuleFlags.Disabled)), 2000);

            #endregion

            return this.OnInitialize();
        }

        public void Release()
        {
            this.OnRelease();
        }

        #region 受保护方法

        #endregion

        #region 事件处理器

        private void Factory_ModuleEvent(ModuleFactory factory, IModuleInstance moduleInst, int eventType, object eventData)
        {
            this.HandleModuleEvent(moduleInst, eventType, eventData);
        }

        private void Factory_Initialized(ModuleFactory factory)
        {
        }

        #endregion

        #region 抽象方法

        protected abstract int OnInitialize();

        protected abstract void OnRelease();

        /// <summary>
        /// 处理模块状态改变事件
        /// </summary>
        /// <param name="moduleInst"></param>
        /// <param name="eventType"></param>
        /// <param name="eventData"></param>
        public abstract void HandleModuleEvent(IModuleInstance moduleInst, int eventType, object eventData);

        #endregion
    }
}
