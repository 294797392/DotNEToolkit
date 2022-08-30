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

        /// <summary>
        /// 某个模块出现循环引用
        /// </summary>
        public event Action<ModuleFactory, IModuleInstance> CircularReference;

        #endregion

        #region 实例变量

        private List<ModuleMetadata> metadataList;

        /// <summary>
        /// 存储已经加载了的模块
        /// </summary>
        private List<ModuleBase> moduleList;

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
                if ((code = moduleInst.Initialize(moduleInst.Definition.InputParameters)) != DotNETCode.SUCCESS)
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

            // 如果模块已经初始化过了，那么返回
            // 因为存在依赖关系，有可能一个模块会调用多次这个方法
            if (moduleInst.Status == ModuleStatus.Initialized)
            {
                return DotNETCode.SUCCESS;
            }

            // 开始初始化依赖项
            if (moduleInst.References.Count > 0)
            {
                if (moduleInst.References.Contains(baseModule.ID))
                {
                    // 此时说明有循环引用
                    // 循环引用直接返回，不初始化
                    logger.ErrorFormat("模块{0}存在循环引用, 请检查Dependencies配置", baseModule.Name);
                    return DotNETCode.MODULE_CIRCULAR_REFERENCE;
                }

                foreach (string dependencyModuleId in moduleInst.References)
                {
                    ModuleBase dependencyModule = this.moduleList.FirstOrDefault(v => v.ID == dependencyModuleId);
                    if (dependencyModule.Status != ModuleStatus.Initialized)
                    {
                        // 如果模块状态不是初始化成功，那么说明该依赖模块还没初始化，开始初始化
                        if ((code = this.InitializeModule(dependencyModule, interval, baseModule)) != DotNETCode.SUCCESS)
                        {
                            // 只要有一个模块初始化失败, 那么就返回
                            return code;
                        }
                    }
                }
            }

            if (interval == -1)
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
                    if (code == DotNETCode.MODULE_CIRCULAR_REFERENCE)
                    {
                        if (this.CircularReference != null)
                        {
                            this.CircularReference(this, moduleInst);
                        }
                    }
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
        private int CreateModuleInstance(IEnumerable<ModuleDefinition> initialModules)
        {
            if (initialModules.Count() == 0)
            {
                return 0;
            }

            foreach (ModuleDefinition module in initialModules)
            {
                ModuleBase moduleInst = this.CreateModule<ModuleBase>(module);

                this.moduleList.Add(moduleInst);
            }

            return this.moduleList.Count;
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
        /// 创建一个模块工厂并同步初始化模块实例
        /// </summary>
        /// <param name="initialModules">要初始化的模块实例</param>
        /// <returns></returns>
        public static ModuleFactory CreateFactory(IEnumerable<ModuleDefinition> initialModules)
        {
            ModuleFactory factory = new ModuleFactory();

            foreach (ModuleDefinition moduleDef in initialModules)
            {
                factory.SetupModule(moduleDef);
            }

            return factory;
        }

        /// <summary>
        /// 创建一个空的工厂
        /// </summary>
        /// <returns></returns>
        public static ModuleFactory CreateFactory()
        {
            return new ModuleFactory();
        }

        /// <summary>
        /// 异步加载模块
        /// 如果模块连接失败，该函数会自动重连模块
        /// </summary>
        /// <param name="initialModules"></param>
        /// <param name="interval">自动重连模块的间隔时间</param>
        /// <returns></returns>
        public void SetupModulesAsync(IEnumerable<ModuleDefinition> initialModules, int interval)
        {
            int modules = this.CreateModuleInstance(initialModules);
            if (modules == 0)
            {
                return;
            }

            this.InitializeModulesAsync(this.moduleList, interval);
        }

        /// <summary>
        /// 同步加载一组组件，加载失败会直接返回，不会尝试重新加载
        /// </summary>
        /// <param name="initialModules">要加载的模块列表</param>
        /// <returns>是否加载成功</returns>
        public int SetupModules(IEnumerable<ModuleDefinition> initialModules)
        {
            int modules = this.CreateModuleInstance(initialModules);
            if (modules == 0)
            {
                return DotNETCode.SUCCESS;
            }

            foreach (ModuleBase moduleInst in moduleList)
            {
                int code = this.InitializeModule(moduleInst, -1, moduleInst);
                if (code != DotNETCode.SUCCESS)
                {
                    return code;
                }
            }

            return DotNETCode.SUCCESS;
        }

        /// <summary>
        /// 同步加载一个模块，如果加载失败则直接返回
        /// 不会尝试重新加载
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public int SetupModule(ModuleDefinition module)
        {
            ModuleBase moduleInst = this.CreateModule<ModuleBase>(module);

            int code = moduleInst.Initialize(module.InputParameters);
            if (code != DotNETCode.SUCCESS)
            {
                logger.DebugFormat("模块加载失败, 错误码:{0}", code);
                return code;
            }

            this.moduleList.Add(moduleInst);

            return DotNETCode.SUCCESS;
        }

        public List<TModuleInstance> LookupModules<TModuleInstance>() where TModuleInstance : IModuleInstance
        {
            return this.moduleList.OfType<TModuleInstance>().ToList();
        }

        /// <summary>
        /// 根据模块定义创建一个模块实例
        /// 但是不初始化它
        /// 如果要创建并初始化，那么请调用SetupModule
        /// </summary>
        /// <param name="module">模块定义</param>
        /// <returns></returns>
        public TModule CreateModule<TModule>(ModuleDefinition module) where TModule : ModuleBase
        {
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
            TModule moduleInst = ConfigFactory<TModule>.CreateInstance(className);
            moduleInst.Definition = module;
            moduleInst.Factory = this;

            logger.DebugFormat("加载模块成功, {0}", module.Name);

            return moduleInst;
        }

        public bool ContainsModule(string moduleID)
        {
            return this.moduleList.Exists(m => m.ID == moduleID);
        }

        /// <summary>
        /// 删除并释放某个模块
        /// </summary>
        /// <param name="moduleID"></param>
        public void ReleaseModule(string moduleID)
        {
            ModuleBase moduleInst = this.moduleList.FirstOrDefault(v => v.ID == moduleID);
            if (moduleInst == null)
            {
                return;
            }

            try
            {
                moduleInst.Release();
                this.moduleList.Remove(moduleInst);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("卸载模块异常, {0}", moduleID), ex);
            }
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
