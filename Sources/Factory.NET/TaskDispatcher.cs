using DotNEToolkit;
using DotNEToolkit.Modular;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using AsyncTask = System.Threading.Tasks.Task<bool>;

namespace Factory.NET
{
    /// <summary>
    /// 测试项目执行引擎
    /// </summary>
    public class TaskDispatcher
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("TaskDispatcher");

        #endregion 

        #region 事件

        /// <summary>
        /// 任务调度器通过这个事件通知外部模块有事件发生
        /// </summary>
        public event Action<TaskDispatcher, TaskDispatcherEvent, object> PublishEvent;

        #endregion

        #region 实例变量

        private List<AsyncTask> asyncTasks; // 所有正在异步运行的任务

        private ModuleFactory taskFactory;

        #endregion

        #region 属性

        /// <summary>
        /// 运行时上下文信息
        /// </summary>
        public DispatcherContext Context { get; private set; }

        /// <summary>
        /// 当前正在运行的任务
        /// </summary>
        public TaskModule CurrentTask { get; private set; }

        #endregion

        #region 公开接口

        public void Initialize(DispatcherContext context)
        {
            this.Context = context;
            this.asyncTasks = new List<AsyncTask>();

            ModuleFactoryOptions factoryOptions = new ModuleFactoryOptions()
            {
                ModuleList = context.Tasks.Cast<ModuleDefinition>().ToList()
            };
            this.taskFactory = ModuleFactory.CreateFactory(factoryOptions);
            this.taskFactory.Initialize();
        }

        /// <summary>
        /// 执行工作流
        /// </summary>
        /// <param name="activities">要执行的工作流列表</param>
        /// <returns></returns>
        public int Start()
        {
            if (this.Context.Cycles == -1)
            {
                while (true)
                {
                    this.Reset();
                    this.ExecuteTasks();
                }
            }
            else if (this.Context.Cycles == 0)
            {
                this.Reset();
                this.ExecuteTasks();
            }
            else
            {
                for (int i = 0; i < this.Context.Cycles; i++)
                {
                    this.Reset();
                    this.ExecuteTasks();
                }
            }

            return ResponseCode.SUCCESS;
        }

        //public bool RunPreTasks(IEnumerable<IVTaskDefinition> preTasks)
        //{
        //    List<IVTaskDefinition> failedTasks;
        //    return this.ExecutePostTasks(preTasks, out failedTasks);
        //}

        #endregion

        #region 实例方法

        /// <summary>
        /// 执行工作流
        /// </summary>
        /// <param name="tasks">正常状态的工作流</param>
        /// <param name="postTasks">后期执行的工作流，无论失败或成功，最后都会执行</param>
        private void ExecuteTasks()
        {
            /* 存储运行失败了的Activity */
            List<TaskDefinition> failureTask1 = null;
            List<TaskDefinition> failureTask2 = null;
            List<bool> result = new List<bool>();

            List<TaskDefinition> normalTasks = this.Context.Tasks.Where(v => !v.PostTask).ToList();
            List<TaskDefinition> alwayPostTasks = this.Context.Tasks.Where(v => v.PostTask && ((PostTaskStrategy)v.PostTaskStrategy) == PostTaskStrategy.Always).ToList();
            List<TaskDefinition> onlyPassPostTasks = this.Context.Tasks.Where(v => v.PostTask && ((PostTaskStrategy)v.PostTaskStrategy) == PostTaskStrategy.OnlyPass).ToList();
            List<TaskDefinition> onlyFailPostTasks = this.Context.Tasks.Where(v => v.PostTask && ((PostTaskStrategy)v.PostTaskStrategy) == PostTaskStrategy.OnlyFail).ToList();

            this.ProcessEvent(TaskDispatcherEvent.Started, null);
            result.Add(this.ExecuteTasksFinally(normalTasks, out failureTask1));
            this.ExecutePostTasks(alwayPostTasks, out failureTask2);

            if (result.All(s => s))
            {
                List<TaskDefinition> failureTask3 = null;
                this.ExecutePostTasks(onlyPassPostTasks, out failureTask3);
                this.ProcessEvent(TaskDispatcherEvent.Completed, true);
            }
            else
            {
                List<TaskDefinition> failureTask4 = null;
                this.ExecutePostTasks(onlyFailPostTasks, out failureTask4);
                this.ProcessEvent(TaskDispatcherEvent.Completed, false);
            }
        }

        private bool ExecuteTasksFinally(IEnumerable<TaskDefinition> taskList, out List<TaskDefinition> failureTasks)
        {
            List<bool> result = new List<bool>();
            failureTasks = new List<TaskDefinition>();

            foreach (TaskDefinition toExecute in taskList)
            {
                bool ret = DelegateUtility.ContinuousInvoke<TaskDefinition>(this.ExecuteTask, toExecute, toExecute.RetryTimes);

                result.Add(ret);
                if (!ret)
                {
                    failureTasks.Add(toExecute);
                    if (!toExecute.HasFlag((int)TaskFlags.IgnoreFailure))
                    {
                        return false;
                    }
                }
            }
            return result.All(r => r);
        }

        private bool ExecutePostTasks(IEnumerable<TaskDefinition> postTasks, out List<TaskDefinition> failureTasks)
        {
            failureTasks = new List<TaskDefinition>();

            List<bool> result = new List<bool>();
            failureTasks = new List<TaskDefinition>();
            foreach (TaskDefinition task in postTasks)
            {
                bool ret = DelegateUtility.ContinuousInvoke<TaskDefinition>(this.ExecuteTask, task, task.RetryTimes);

                result.Add(ret);
                if (!ret)
                {
                    failureTasks.Add(task);
                }
            }
            return result.All(r => r);
        }

        /// <summary>
        /// 执行一个任务
        /// </summary>
        /// <param name="taskDef">要执行的任务信息</param>
        /// <param name="output">任务的输出参数</param>
        /// <returns></returns>
        private bool ExecuteTask(TaskDefinition taskDef)
        {
            TaskModule task = null;
            bool initialized = false;
            int code = ResponseCode.SUCCESS;

            try
            {
                // 使用缓存的Task实例
                task = this.taskFactory.LookupModule<TaskModule>(taskDef.ID);
                task.Context = this.Context;

                this.CurrentTask = task;

                // 如果Task有Disabled标记，那么跳过运行
                if (taskDef.HasFlag(ModuleFlags.Disabled))
                {
                    this.ProcessTaskStatusChanged(TaskModuleStatus.SKIP, task);
                    logger.InfoFormat("{0}未启用, 跳过", taskDef.Name);
                    this.PubMessage("{0}未启用, 跳过", taskDef.Name);
                    return true;
                }

                // 延时运行
                if (taskDef.Delay > 0)
                {
                    logger.InfoFormat("延时{0}秒运行{1}", (double)taskDef.Delay / (double)1000, taskDef.Name);
                    this.PubMessage("延时{0}秒运行{1}", (double)taskDef.Delay / (double)1000, taskDef.Name);
                    this.ProcessTaskStatusChanged(TaskModuleStatus.WAIT, task);
                    Thread.Sleep(taskDef.Delay);
                }

                logger.InfoFormat("开始运行:{0}", taskDef.Name);
                this.PubMessage("开始运行:{0}", taskDef.Name);

                this.ProcessTaskStatusChanged(TaskModuleStatus.RUN, task);

                //// 解析参数表达式
                IDictionary inputParams = taskDef.InputParameters;
                //if ((code = ExpressionUtility.EvaluateExpressions(taskDef.InputParameters, this.Context, out inputParams)) != ResponseCode.SUCCESS)
                //{
                //    this.ProcessTaskStatusChanged(IVTaskStatus.FAIL, task);
                //    logger.ErrorFormat("计算输入参数表达式失败, 运行任务失败, code = {0}, {1}", code, ResponseCode.GetMessage(code));
                //    this.PubMessage("计算输入参数表达式失败, 运行任务失败, code = {0}, {1}", code, ResponseCode.GetMessage(code));
                //    return false;
                //}

                // 保存输入参数，供其他的任务解析输入表达式使用
                this.Context.TaskInputs[taskDef.ID] = inputParams;

                if ((code = task.Initialize()) != ResponseCode.SUCCESS)
                {
                    this.ProcessTaskStatusChanged(TaskModuleStatus.FAIL, task);
                    logger.ErrorFormat("初始化{0}失败, code = {1}, {2}", taskDef.Name, code, ResponseCode.GetMessage(code));
                    this.PubMessage("初始化{0}失败, code = {1}, {2}", taskDef.Name, code, ResponseCode.GetMessage(code));
                    return false;
                }

                initialized = true;

                if ((code = task.Run()) != ResponseCode.SUCCESS)
                {
                    this.ProcessTaskStatusChanged(TaskModuleStatus.FAIL, task);
                    logger.ErrorFormat("运行{0}失败, code = {1}, {2}", taskDef.Name, code, ResponseCode.GetMessage(code));
                    this.PubMessage("运行{0}失败, code = {1}, {2}", taskDef.Name, code, ResponseCode.GetMessage(code));
                    return false;
                }

                this.ProcessTaskStatusChanged(TaskModuleStatus.PASS, task);
                logger.InfoFormat("运行{0}成功", taskDef.Name);
                this.PubMessage("运行{0}成功", taskDef.Name);
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("{0}出现异常", taskDef.Name), ex);
                this.PubMessage(string.Format("{0}出现异常, {1}", taskDef.Name, ex));
                if (taskDef.HasFlag((int)TaskFlags.IgnoreFailure))
                {
                    logger.WarnFormat("{0}运行出现异常, 执行下一个流程", taskDef.Name);
                    this.PubMessage("{0}运行出现异常, 执行下一个流程", taskDef.Name);
                }
                else
                {
                    logger.WarnFormat("{0}运行出现异常, 退出", taskDef.Name);
                    this.PubMessage("{0}运行出现异常, 退出", taskDef.Name);
                }
                this.ProcessTaskStatusChanged(TaskModuleStatus.EXCEPTION, task);
                return false;
            }
            finally
            {
                if (task != null)
                {
                    // 保存输出参数，供其他的任务解析输入表达式使用
                    this.Context.TaskProperties[task.ID] = task.Properties;

                    // 保存ReasonCode
                    //task.Properties[IVTaskProperties.PROP_REASON_CODE] = code;

                    if (initialized)
                    {
                        task.Reset();
                    }
                }

                this.CurrentTask = null;
            }
        }

        private void ProcessTaskStatusChanged(TaskModuleStatus status, TaskModule task)
        {
            if (this.PublishEvent != null)
            {
                TaskStatusChangedEventArgs eventArgs = new TaskStatusChangedEventArgs()
                {
                    Status = status,
                    Task = task
                };
                this.PublishEvent(this, TaskDispatcherEvent.TaskStatusChanged, eventArgs);
            }
            //task.Properties[IVTaskProperties.PROP_STATUS] = status;
        }

        private void ProcessEvent(TaskDispatcherEvent evt, object evParam = null)
        {
            if (this.PublishEvent != null)
            {
                this.PublishEvent(this, evt, evParam);
            }
        }

        private void PubMessage(string message, params object[] param)
        {
            if (this.PublishEvent != null)
            {
                this.PublishEvent(this, TaskDispatcherEvent.Message, string.Format(message, param));
            }
        }

        private void Reset()
        {
            this.Context.TaskInputs.Clear();
            this.Context.TaskProperties.Clear();
            this.asyncTasks.Clear();
        }

        #endregion

        #region 事件处理器

        #endregion
    }
}
