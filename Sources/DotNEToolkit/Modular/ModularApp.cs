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
    /// <summary>
    /// App清单文件
    /// </summary>
    public abstract class AppManifest
    {
        /// <summary>
        /// 配置文件里的所有的模块列表
        /// </summary>
        [JsonProperty("modules")]
        public List<ModuleDefinition> ModuleList { get; private set; }

        /// <summary>
        /// 所有AppModule模块列表
        /// 暂时没实现
        /// </summary>
        [JsonProperty("appModules")]
        internal List<ModuleDefinition> AppModuleList { get; private set; }

        public AppManifest()
        {
            this.ModuleList = new List<ModuleDefinition>();
            this.AppModuleList = new List<ModuleDefinition>();
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
        /// 指定是否使用异步加载模块
        /// </summary>
        protected virtual bool AsyncInitializing { get { return true; } }

        /// <summary>
        /// 暂时没实现
        /// </summary>
        internal List<AppModule<TApp, TManifest>> AppModules { get; private set; }

        /// <summary>
        /// 模块工厂
        /// </summary>
        public ModuleFactory Factory { get; private set; }

        /// <summary>
        /// App的配置文件
        /// </summary>
        public TManifest Manifest { get; private set; }

        #endregion

        #region 公开接口

        /// <summary>
        /// 初始化App
        /// 自动读取App.config文件里的appConfig里配置的配置文件路径
        /// </summary>
        /// <returns></returns>
        public int Initialize()
        {
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

            return this.Initialize(configPath);
        }

        /// <summary>
        /// 指定一个配置文件的路径初始化App
        /// </summary>
        /// <param name="configFile">App配置文件路径</param>
        /// <returns></returns>
        public int Initialize(string configFile)
        {
            if (!File.Exists(configFile))
            {
                logger.ErrorFormat("配置文件不存在, {0}", configFile);
                return DotNETCode.FILE_NOT_FOUND;
            }

            #region 加载配置文件

            TManifest manifest = default(TManifest);

            try
            {
                manifest = JSONHelper.ParseFile<TManifest>(configFile);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("解析App配置文件异常, {0}", configFile), ex);
                return DotNETCode.PARSE_CONFIG_FAILED;
            }

            #endregion

            return this.Initialize(configFile, manifest);
        }

        /// <summary>
        /// 使用AppManifest的实例初始化App
        /// </summary>
        /// <param name="configFile">指定配置文件的完整路径，当调用SaveManifest的时候，将会使用该路径进行保存</param>
        /// <param name="manifest"></param>
        /// <returns></returns>
        public int Initialize(string configFile, TManifest manifest)
        {
            this.configPath = configFile;
            this.AppModules = new List<AppModule<TApp, TManifest>>();
            this.Manifest = manifest;

            #region 加载ModuleFactory

            logger.Info("开始加载ModuleFactory...");
            ModuleFactoryOptions options = new ModuleFactoryOptions()
            {
                AsyncInitializing = this.AsyncInitializing,
                ModuleList = this.Manifest.ModuleList
            };
            this.Factory = ModuleFactory.CreateFactory(options);
            this.Factory.Initialized += Factory_Initialized;
            this.Factory.ModuleStatusChanged += Factory_ModuleStatusChanged;
            int code = this.Factory.Initialize();
            if (code != DotNETCode.SUCCESS)
            {
                return code;
            }

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

        #endregion

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
        protected virtual void OnModuleStatusEvent(IModuleInstance moduleInst, ModuleStatus status)
        { }

        /// <summary>
        /// 处理所有模块都初始化成功的事件
        /// </summary>
        protected virtual void OnModuleInitialized()
        { }

        /// <summary>
        /// 当出现模块循环引用的时候触发
        /// </summary>
        /// <param name="moduleInst">存在循环引用的模块</param>
        protected virtual void OnCircularReference(IModuleInstance moduleInst)
        { }

        #endregion
    }
}
