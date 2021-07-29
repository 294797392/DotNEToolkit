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
    /// 自动识别modules.*.json的文件为模块元数据文件
    /// 
    /// ModuleFactory从模块元数据文件里根据TypeID去查找对应模块的EntryClass
    /// </summary>
    public class ModuleFactory
    {
        private const string ModuleMetadataFilePattern = "modules.*.json";

        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("ModuleFactory");

        #endregion

        #region 公开事件

        /// <summary>
        /// 当某个模块的状态发生改变的时候触发
        /// </summary>
        public event Action<ModuleFactory, IModuleInstance, ModuleStatus> ModuleStatusChanged;

        /// <summary>
        /// 当所有模块都加载完成的时候触发
        /// 只有在异步加载模块的时候才会触发
        /// </summary>
        public event Action<ModuleFactory> Initialized;

        /// <summary>
        /// 当模块出发了一个事件的时候触发
        /// </summary>
        public event Action<ModuleFactory, IModuleInstance, string, object> ModuleEvent;

        #endregion

        #region 实例变量

        private List<ModuleMetadata> metadataList;

        #endregion

        #region 属性

        /// <summary>
        /// 所有模块的元数据信息
        /// </summary>
        public ReadOnlyCollection<ModuleMetadata> MetadataList { get; private set; }

        /// <summary>
        /// 存储已经加载了的模块
        /// </summary>
        public List<IModuleInstance> ModuleList { get; private set; }

        #endregion

        #region 构造方法

        protected ModuleFactory()
        {
            this.ModuleList = new List<IModuleInstance>();
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

        private int InitializeModuleFinal(IModuleInstance moduleInst)
        {
            int code = DotNETCode.SUCCESS;

            this.NotifyModuleStatusChanged(moduleInst, ModuleStatus.Initializing);

            try
            {
                moduleInst.Status = ModuleStatus.Initializing;

                if ((code = moduleInst.Initialize(moduleInst.Definition.InputParameters)) != DotNETCode.SUCCESS)
                {
                    moduleInst.Status = ModuleStatus.InitializeFailed;
                    this.NotifyModuleStatusChanged(moduleInst, ModuleStatus.InitializeFailed);
                    logger.WarnFormat("初始化模块失败, module = {0}, code = {1}, {2}", moduleInst.Name, code, DotNETCode.GetMessage(code));
                    return code;
                }

                logger.InfoFormat("模块初始化成功, module = {0}", moduleInst.Name);

                moduleInst.Status = ModuleStatus.Initialized;
                this.NotifyModuleStatusChanged(moduleInst, ModuleStatus.Initialized);

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

        private void InitializeModuleFinal(IModuleInstance moduleInst, int interval)
        {
            int code = DotNETCode.SUCCESS;

            while (true)
            {
                if ((code = this.InitializeModuleFinal(moduleInst)) != DotNETCode.SUCCESS)
                {
                    Thread.Sleep(interval);
                    continue;
                }
                else
                {
                    break;
                }
            }
        }

        private void InitializeModulesAsync(IEnumerable<IModuleInstance> moduleList, int interval)
        {
            Task.Factory.StartNew(() =>
            {
                foreach (IModuleInstance moduleInst in moduleList)
                {
                    this.InitializeModuleFinal(moduleInst, interval);
                }

                if (this.Initialized != null)
                {
                    this.Initialized(this);
                }
            });
        }

        private int CreateModuleInstance(ModuleDefinition moduleDef, out IModuleInstance instance)
        {
            instance = null;

            ModuleMetadata metadata = this.metadataList.FirstOrDefault(info => info.ID == moduleDef.TypeID);
            if (metadata == null)
            {
                logger.ErrorFormat("客户端不存在模块:{0}", moduleDef);
                return DotNETCode.MODULE_NOT_FOUND;
            }
            else
            {
                try
                {
                    instance = ConfigFactory<IModuleInstance>.CreateInstance(metadata.EntryClass);
                    instance.Definition = moduleDef;
                    instance.Factory = this;
                    instance.PublishEvent += this.Module_PublishEvent;
                    return DotNETCode.SUCCESS;
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("加载模块异常, {0}, {1}", moduleDef, ex);
                    return DotNETCode.UNKNOWN_EXCEPTION;
                }
            }
        }

        #endregion

        #region 公开接口

        /// <summary>
        /// 根据配置文件加载ModuleFactory
        /// 同步接口
        /// </summary>
        /// <param name="descFile">ModuleFactory描述文件的路径</param>
        /// <returns></returns>
        public static ModuleFactory CreateFactory(string descFile)
        {
            ModuleFactoryDescription description;
            if (!JSONHelper.DeserializeJSONFile(descFile, out description))
            {
                return null;
            }

            return CreateFactory(description.ModuleList.Where(v => !v.HasFlag(ModuleFlags.Disabled)));
        }

        /// <summary>
        /// 创建一个模块工厂并同步初始化模块实例
        /// </summary>
        /// <param name="initialModules">要初始化的模块实例</param>
        /// <returns></returns>
        public static ModuleFactory CreateFactory(IEnumerable<ModuleDefinition> initialModules, IEnumerable<string> metadataFiles = null)
        {
            ModuleFactory factory = new ModuleFactory();

            factory.RegisterMetadata(metadataFiles);

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
        /// 注册一组组件元数据
        /// </summary>
        /// <param name="moduleFiles">要加载的类型文件列表</param>
        public void RegisterMetadata(IEnumerable<string> metaFiles)
        {
            if (metaFiles == null)
            {
                return;
            }

            this.metadataList.AddRange(JSONHelper.ParseFiles<ModuleMetadata>(metaFiles));
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

            foreach (ModuleDefinition moduleDef in initialModules)
            {
                IModuleInstance moduleInst;

                int code = this.CreateModuleInstance(moduleDef, out moduleInst);
                if (code != DotNETCode.SUCCESS)
                {
                    // TODO：初始化模块失败..
                    continue;
                }

                this.ModuleList.Add(moduleInst);
            }

            this.InitializeModulesAsync(this.ModuleList, interval);
        }

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
            IModuleInstance moduleInst;

            int code = DotNETCode.SUCCESS;

            if ((code = this.CreateModuleInstance(module, out moduleInst)) != DotNETCode.SUCCESS)
            {
                return code;    
            }

            if ((code = moduleInst.Initialize(module.InputParameters)) != DotNETCode.SUCCESS)
            {
                return code;
            }

            this.ModuleList.Add(moduleInst);

            return DotNETCode.SUCCESS;
        }

        public List<TModuleInstance> LookupModules<TModuleInstance>() where TModuleInstance : IModuleInstance
        {
            return this.ModuleList.OfType<TModuleInstance>().ToList();
        }

        public bool ContainsModule(string moduleID)
        {
            return this.ModuleList.Exists(m => m.ID == moduleID);
        }

        /// <summary>
        /// 删除并释放某个模块
        /// </summary>
        /// <param name="moduleID"></param>
        public void RemoveModule(string moduleID)
        {
            IModuleInstance moduleInst = this.ModuleList.FirstOrDefault(v => v.ID == moduleID);
            if (moduleInst == null)
            {
                return;
            }

            try
            {
                moduleInst.PublishEvent -= this.Module_PublishEvent;
                moduleInst.Release();
                this.ModuleList.Remove(moduleInst);
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
                return this.ModuleList.OfType<TModuleInstance>().FirstOrDefault();
            }
            else
            {
                return this.ModuleList.OfType<TModuleInstance>().FirstOrDefault(c => c.ID == id);
            }
        }

        /// <summary>
        /// 查找指定类型的组件，如果有多个，则返回第一个
        /// </summary>
        /// <typeparam name="TModuleInstance"></typeparam>
        /// <returns></returns>
        public TModuleInstance LookupModule<TModuleInstance>() where TModuleInstance : IModuleInstance
        {
            return this.ModuleList.OfType<TModuleInstance>().FirstOrDefault();
        }

        /// <summary>
        /// 根据类型ID创建一个模块实例
        /// </summary>
        /// <typeparam name="TModuleInstance"></typeparam>
        /// <param name="metadataID">要创建的模块的类型ID</param>
        /// <returns></returns>
        public TModuleInstance CreateInstance<TModuleInstance>(string metadataID) where TModuleInstance : IModuleInstance
        {
            ModuleMetadata type = this.metadataList.FirstOrDefault(minfo => minfo.ID == metadataID);
            if (type == null)
            {
                return default(TModuleInstance);
            }

            try
            {
                return ConfigFactory<TModuleInstance>.CreateInstance(type.EntryClass);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("创建模块实例异常, {0}, {1}", type, ex);
                return default(TModuleInstance);
            }
        }

        /// <summary>
        /// 判断某个Module是否派生自baseType
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public bool SubclassOf(ModuleDefinition definition, Type baseType)
        {
            ModuleMetadata metadata = this.metadataList.FirstOrDefault(v => v.ID == definition.TypeID);
            if (metadata == null)
            {
                return false;
            }

            Type t = Type.GetType(metadata.EntryClass);
            if (t.IsSubclassOf(baseType))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        private void NotifyModuleStatusChanged(IModuleInstance module, ModuleStatus status)
        {
            if (this.ModuleStatusChanged != null)
            {
                this.ModuleStatusChanged(this, module, status);
            }
        }

        private void Module_PublishEvent(IModuleInstance module, string eventCode, object eventParams)
        {
            if (this.ModuleEvent != null)
            {
                this.ModuleEvent(this, module, eventCode, eventParams);
            }
        }
    }
}
