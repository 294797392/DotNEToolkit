using DotNEToolkit.Modular.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNEToolkit.Modular
{
    /// <summary>
    /// 模块工厂
    /// </summary>
    public class ModuleFactory
    {
        private const string ModuleMetadataFilePattern = "modules.*.json";

        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("ModuleFactory");

        #endregion

        #region 公开事件

        /// <summary>
        /// 当所有模块都加载完成的时候触发
        /// 只有在异步加载模块的时候才会触发
        /// </summary>
        public event Action<ModuleFactory> Initialized;

        /// <summary>
        /// 当模块状态改变的时候触发
        /// </summary>
        public event Action<ModuleFactory, IModuleInstance, ModuleStatus> ModuleStatusChanged;

        #endregion

        #region 实例变量

        private List<ModuleMetadata> metadataList;

        /// <summary>
        /// 存储已经加载了的模块
        /// </summary>
        private List<ModuleBase> moduleList;

        private ModuleFactoryOptions options;

        #endregion

        #region 属性

        /// <summary>
        /// 所有模块的元数据信息
        /// </summary>
        public ReadOnlyCollection<ModuleMetadata> MetadataList { get; private set; }

        #endregion

        #region 构造方法

        /// <summary>
        /// 构造方法
        /// </summary>
        protected ModuleFactory()
        {
            this.moduleList = new List<ModuleBase>();
            this.metadataList = new List<ModuleMetadata>();
            this.metadataList.AddRange(LookupModuleMetadatas());
            this.MetadataList = new ReadOnlyCollection<ModuleMetadata>(this.metadataList);
        }

        #endregion

        #region 实例方法

        private int InitializeModuleFinal(ModuleBase moduleInst)
        {
            int code = DotNETCode.SUCCESS;

            moduleInst.Status = ModuleStatus.Initializing;
            this.NotifyModuleStatusChanged(moduleInst, ModuleStatus.Initializing);

            try
            {
                if ((code = moduleInst.Initialize()) != DotNETCode.SUCCESS)
                {
                    moduleInst.Status = ModuleStatus.InitializeFailed;
                    this.NotifyModuleStatusChanged(moduleInst, ModuleStatus.InitializeFailed);
                    logger.WarnFormat("初始化模块失败, module = {0}, code = {1}", moduleInst.Name, code);
                    return code;
                }

                moduleInst.Status = ModuleStatus.Initialized;
                this.NotifyModuleStatusChanged(moduleInst, ModuleStatus.Initialized);

                logger.InfoFormat("模块初始化成功, module = {0}", moduleInst.Name);

                return DotNETCode.SUCCESS;
            }
            catch (Exception ex)
            {
                moduleInst.Status = ModuleStatus.InitializeException;
                this.NotifyModuleStatusChanged(moduleInst, ModuleStatus.InitializeException);
                logger.Error("初始化模块异常", ex);
                return DotNETCode.UNKNOWN_EXCEPTION;
            }
        }

        /// <summary>
        /// 初始化一个模块，会解析依赖项
        /// </summary>
        /// <param name="moduleInst">要初始化的模块</param>
        /// <param name="interval">
        /// > 0  : 重新初始化的间隔时间，如果初始化失败，那么会一直初始化
        /// = -1 : 失败直接返回，不尝试重新初始化
        /// </param>
        /// <param name="baseModule">当前初始化的模块</param>
        private int InitializeModule(ModuleBase moduleInst, int interval, ModuleBase baseModule)
        {
            int code = DotNETCode.SUCCESS;

            if (interval <= 0)
            {
                return this.InitializeModuleFinal(moduleInst);
            }
            else
            {
                while ((code = this.InitializeModuleFinal(moduleInst)) != DotNETCode.SUCCESS)
                {
                    Thread.Sleep(interval);
                }

                return DotNETCode.SUCCESS;
            }
        }

        private void InitializeModulesAsync(IEnumerable<ModuleBase> moduleList, int interval)
        {
            Task.Factory.StartNew(() =>
            {
                foreach (ModuleBase moduleInst in moduleList)
                {
                    int code = this.InitializeModule(moduleInst, interval, moduleInst);
                }

                if (this.Initialized != null)
                {
                    this.Initialized(this);
                }
            });
        }

        /// <summary>
        /// 创建模块的实例
        /// 只创建实例，不调用Initialize方法
        /// </summary>
        /// <param name="initialModules"></param>
        /// <returns>返回创建了的模块个数</returns>
        private List<ModuleBase> CreateModuleInstance(IEnumerable<ModuleDefinition> initialModules)
        {
            List<ModuleBase> moduleList = new List<ModuleBase>();

            foreach (ModuleDefinition module in initialModules)
            {
                if (module.HasFlag(ModuleFlags.Disabled))
                {
                    logger.InfoFormat("模块{0}被禁用, 不加载", module.Name);
                    continue;
                }

                // 优先加载ClassName
                string className = module.ClassName;

                // 如果ClassName不存在，那么根据MetadataID寻找ClassName
                if (string.IsNullOrEmpty(className))
                {
                    ModuleMetadata metadata = this.metadataList.FirstOrDefault(info => info.ID == module.MetadataID);
                    if (metadata == null)
                    {
                        logger.ErrorFormat("客户端不存在模块:{0}", module);
                        throw new ModuleNotFoundException(module);
                    }

                    className = metadata.ClassName;
                }

                // 开始加载实例
                ModuleBase moduleInst = ConfigFactory<ModuleBase>.CreateInstance(className);
                moduleInst.Definition = module;
                moduleInst.Factory = this;
                moduleInst.InputParameters = module.InputParameters;

                logger.DebugFormat("加载模块成功, {0}", module.Name);

                moduleList.Add(moduleInst);
            }

            return moduleList;
        }

        #endregion

        #region 公开接口

        /// <summary>
        /// 读取所有的类型定义
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ModuleMetadata> LookupModuleMetadatas()
        {
            return JSONHelper.ParseDirectory<ModuleMetadata>(AppDomain.CurrentDomain.BaseDirectory, ModuleMetadataFilePattern);
        }

        /// <summary>
        /// 创建一个空的工厂
        /// </summary>
        /// <returns></returns>
        public static ModuleFactory CreateFactory(ModuleFactoryOptions options)
        {
            return new ModuleFactory()
            {
                options = options,
            };
        }

        /// <summary>
        /// 初始化ModuleFactory
        /// 1. 根据Options加载模块
        /// 
        /// 同步加载：如果初始化摸个模块失败，那么就返回FAILED
        /// 异步加载：永远返回SUCCESS
        /// </summary>
        public int Initialize()
        {
            if (this.options.ModuleList.Count == 0)
            {
                return DotNETCode.SUCCESS;
            }

            this.CreateModuleInstance();
            return this.InitializeModuleInstance();
        }

        public List<TModuleInstance> LookupModules<TModuleInstance>() where TModuleInstance : IModuleInstance
        {
            return this.moduleList.OfType<TModuleInstance>().ToList();
        }

        /// <summary>
        /// 根据ID查找系统中可用的组件
        /// 如果ID为空，则返回第一个可用的组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public TModuleInstance LookupModule<TModuleInstance>(string id) where TModuleInstance : IModuleInstance
        {
            if (string.IsNullOrEmpty(id))
            {
                return this.moduleList.OfType<TModuleInstance>().FirstOrDefault();
            }
            else
            {
                return this.moduleList.OfType<TModuleInstance>().FirstOrDefault(c => c.ID == id);
            }
        }

        /// <summary>
        /// 查找指定类型的组件，如果有多个，则返回第一个
        /// </summary>
        /// <typeparam name="TModuleInstance"></typeparam>
        /// <returns></returns>
        public TModuleInstance LookupModule<TModuleInstance>() where TModuleInstance : IModuleInstance
        {
            return this.moduleList.OfType<TModuleInstance>().FirstOrDefault();
        }

        /// <summary>
        /// 根据模块类型获取对应的模块实例
        /// </summary>
        /// <param name="targetType">要获取的模块类型</param>
        /// <returns></returns>
        public ModuleBase LookupModule(Type targetType)
        {
            foreach (ModuleBase module in this.moduleList)
            {
                Type moduleType = module.GetType();
                if (moduleType.IsAssignableFrom(targetType))
                {
                    // 此时说明targetType是moduleType的基类
                    return module;
                }
            }

            return null;
        }

        #endregion

        #region Internal方法

        /// <summary>
        /// 创建所有模块的实例
        /// 只创建实例，不初始化
        /// </summary>
        internal void CreateModuleInstance()
        {
            List<ModuleBase> moduleList = this.CreateModuleInstance(this.options.ModuleList);
            this.moduleList.AddRange(moduleList);
        }

        /// <summary>
        /// 根据同步或者异步配置，来初始化模块
        /// ModuleFactoryOptions.AsyncInitializing
        /// </summary>
        /// <returns></returns>
        internal int InitializeModuleInstance()
        {
            if (this.options.AsyncInitializing)
            {
                this.InitializeModulesAsync(this.moduleList, this.options.ReInitializeInterval);
                return DotNETCode.SUCCESS;
            }
            else
            {
                foreach (ModuleBase moduleInstance in moduleList)
                {
                    int code = moduleInstance.Initialize();
                    if (code != DotNETCode.SUCCESS)
                    {
                        logger.DebugFormat("模块加载失败, 错误码:{0}", code);
                        moduleInstance.Status = ModuleStatus.InitializeFailed;
                        return DotNETCode.FAILED;
                    }
                    moduleInstance.Status = ModuleStatus.Initialized;
                }

                return DotNETCode.SUCCESS;
            }
        }

        #endregion

        #region 实例方法

        private void NotifyModuleStatusChanged(IModuleInstance moduleInst, ModuleStatus status)
        {
            if (this.ModuleStatusChanged != null)
            {
                this.ModuleStatusChanged(this, moduleInst, status);
            }
        }

        #endregion
    }
}
