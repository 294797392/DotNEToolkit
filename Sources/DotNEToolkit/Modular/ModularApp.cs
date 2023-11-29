using DotNEToolkit.Modular;
using DotNEToolkit.Modular.Attributes;
using DotNEToolkit.Utility;
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
    /// 清单文件里只存储不可变的数据
    /// 可变数据属于配置数据，调用ModularApp的GetConfig个SetConfig接口进行读写
    /// </summary>
    public abstract class AppManifest
    {
        /// <summary>
        /// 是否异步初始化模块
        /// 默认不异步，默认用同步初始化
        /// </summary>
        [JsonProperty("asyncInit")]
        public bool AsynchronousInitialization { get; set; }

        /// <summary>
        /// 配置文件里的所有的模块列表
        /// </summary>
        [JsonProperty("modules")]
        public List<ModuleDefinition> ModuleList { get; private set; }

        /// <summary>
        /// App的配置文件路径
        /// </summary>
        [JsonProperty("settingPath")]
        public string SettingPath { get; set; }

        /// <summary>
        /// 构造方法
        /// </summary>
        public AppManifest()
        {
            this.ModuleList = new List<ModuleDefinition>();
            this.AsynchronousInitialization = false;
        }
    }

    /// <summary>
    /// 封装一个基于模块化（ModuleFactory）实现的App开发框架
    /// 1. 实现了AppModule的依赖注入功能
    /// </summary>
    /// <typeparam name="TApp"></typeparam>
    /// <typeparam name="TManifest"></typeparam>
    public abstract class ModularApp<TApp, TManifest> : SingletonObject<TApp>
        where TApp : class
        where TManifest : AppManifest
    {
        protected static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(TApp));

        private const string KEY_CONFIG_PATH = "appConfig";
        private const string DefaultAppManifestFileName = "app.json";
        private const string DefaultAppSettingFileName = "app.setting.json";

        #region 实例变量

        private string configPath;
        private string settingPath;

        private Dictionary<string, string> settings;

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
                configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultAppManifestFileName);
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
        /// <param name="manifest">要初始化的App清单</param>
        /// <returns></returns>
        public int Initialize(string configFile, TManifest manifest)
        {
            this.configPath = configFile;
            this.Manifest = manifest;
            this.settingPath = string.IsNullOrEmpty(manifest.SettingPath) ? DefaultAppSettingFileName : manifest.SettingPath;

            #region 加载Setting

            if (File.Exists(DefaultAppSettingFileName))
            {
                try
                {
                    this.settings = JSONHelper.ParseFile<Dictionary<string, string>>(this.settingPath);

                    logger.InfoFormat("加载appSetting成功, path = {0}, count = {1}", this.settingPath, this.settings.Count);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("读取appSetting异常, 文件路径 = {0}, ex = {1}", this.settingPath, ex);
                    this.settings = new Dictionary<string, string>();
                }
            }
            else
            {
                logger.InfoFormat("appSetting不存在, 初始化新的appSetting");
                this.settings = new Dictionary<string, string>();
            }

            #endregion

            #region 加载ModuleFactory

            logger.Info("开始加载ModuleFactory...");
            ModuleFactoryOptions options = new ModuleFactoryOptions()
            {
                AsyncInitializing = this.Manifest.AsynchronousInitialization,
                ModuleList = this.Manifest.ModuleList
            };
            this.Factory = ModuleFactory.CreateFactory(options);
            this.Factory.Initialized += Factory_Initialized;
            this.Factory.ModuleStatusChanged += Factory_ModuleStatusChanged;

            // 创建模块的实例
            this.Factory.CreateModuleInstance();

            // 初始化AppModule
            // 1. 为Manifest字段赋值，使AppModule可以访问到AppManifest
            // 2. 如果AppModule依赖于某个其他的模块，注入其他模块
            this.InitializeAppModule();

            // 调用模块的初始化方法初始化模块
            int code = this.Factory.InitializeModuleInstance();
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
        /// 写入并保存配置文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void WriteSetting<T>(string key, T value)
        {
            string json = JsonConvert.SerializeObject(value);
            this.settings[key] = json;

            JSONHelper.Object2File<Dictionary<string, string>>(this.settingPath, this.settings);
        }

        /// <summary>
        /// 读取配置文件里的配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T ReadSetting<T>(string key, T defaultValue)
        {
            string json;
            if (!this.settings.TryGetValue(key, out json))
            {
                return defaultValue;
            }

            return JSONHelper.Parse<T>(json);
        }

        #endregion

        #region 实例方法

        private void InitializeAppModule()
        {
            List<AppModule<TManifest>> appModules = this.Factory.LookupModules<AppModule<TManifest>>();
            foreach (AppModule<TManifest> appModule in appModules)
            {
                appModule.AppManifest = this.Manifest;

                #region 如果该模块依赖其他模块，对模块进行依赖注入

                List<PropertyAttribute<InjectableAttribute>> propertyAttributes = ReflectionUtils.GetPropertyAttribute<InjectableAttribute>(appModule.GetType());
                foreach (PropertyAttribute<InjectableAttribute> propertyAttribute in propertyAttributes)
                {
                    // 要依赖注入的模块类型
                    Type targetType = propertyAttribute.Property.PropertyType;

                    // 找到对应的类型的实例
                    ModuleBase module = this.Factory.LookupModule(targetType);
                    if (module == null)
                    {
                        // 没找到
                        continue;
                    }

                    // 注入成功
                    propertyAttribute.Property.SetValue(appModule, module);
                }

                #endregion
            }
        }

        #endregion

        #region 事件处理器

        private void Factory_Initialized(ModuleFactory factory)
        {
            // 异步初始化会运行这个函数
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
