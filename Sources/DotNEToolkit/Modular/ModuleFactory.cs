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

        /// <summary>
        /// 读取所有的类型定义
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ModuleMetadata> LookupModuleMetadatas()
        {
            return JSONHelper.ParseDirectory<ModuleMetadata>(AppDomain.CurrentDomain.BaseDirectory, ModuleMetadataFilePattern);
        }

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
        /// 初始化一个模块，直到该模块初始化成功为止
        /// 如果初始化失败，那么会一直初始化
        /// </summary>
        /// <param name="moduleInst">要初始化的模块</param>
        /// <param name="interval">重新初始化的间隔时间</param>
        private void InitializeModule(ModuleBase moduleInst, int interval)
        {
            int code = DotNETCode.SUCCESS;

            while ((code = this.InitializeModuleFinal(moduleInst)) != DotNETCode.SUCCESS)
            {
                Thread.Sleep(interval);
            }
        }

        private void InitializeModulesAsync(IEnumerable<ModuleBase> moduleList, int interval)
        {
            Task.Factory.StartNew(() =>
            {
                foreach (ModuleBase moduleInst in moduleList)
                {
                    this.InitializeModule(moduleInst, interval);
                }

                if (this.Initialized != null)
                {
                    this.Initialized(this);
                }
            });
        }

        #endregion

        #region 公开接口

        /// <summary>
        /// 根据配置文件加载ModuleFactory
        /// 同步接口
        /// </summary>
        /// <param name="moduleFile">模块文件</param>
        /// <returns></returns>
        public static ModuleFactory CreateFactory(string moduleFile)
        {
            ModuleFactoryDescription description = JSONHelper.ParseFile<ModuleFactoryDescription>(moduleFile, new ModuleFactoryDescription());
            return CreateFactory(description.ModuleList.Where(v => !v.HasFlag(ModuleFlags.Disabled)));
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
        /// 异步初始化ModuleFactory
        /// </summary>
        public void SetupAsync(string descFile, int interval = 1000)
        {
            ModuleFactoryDescription description = JSONHelper.ParseFile<ModuleFactoryDescription>(descFile, new ModuleFactoryDescription());
            this.SetupModulesAsync(description.ModuleList.Where(v => !v.HasFlag(ModuleFlags.Disabled)), 1000);
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
            if (initialModules.Count() == 0)
            {
                return;
            }

            foreach (ModuleDefinition module in initialModules)
            {
                ModuleBase moduleInst = this.CreateModule<ModuleBase>(module);

                this.moduleList.Add(moduleInst);
            }

            this.InitializeModulesAsync(this.moduleList, interval);
        }

        /// <summary>
        /// 同步加载一组组件，加载失败会直接返回，不会尝试重新加载
        /// </summary>
        /// <param name="modules">要加载的模块列表</param>
        /// <returns>是否加载成功</returns>
        public int SetupModules(IEnumerable<ModuleDefinition> modules)
        {
            int code = DotNETCode.SUCCESS;

            foreach (ModuleDefinition module in modules)
            {
                if ((code = this.SetupModule(module)) != DotNETCode.SUCCESS)
                {
                    return code;
                }
            }

            return code;
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
