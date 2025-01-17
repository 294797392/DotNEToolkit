using DotNEToolkit.Modular;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            start:

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

            // 测试流结束运行之后的行为
            switch (this.Context.CompletedBehavior)
            {
                case CompletedBehaviors.None:
                    {
                        break;
                    }

                case CompletedBehaviors.Restart:
                    {
                        this.ProcessEvent(TaskDispatcherEvent.Restart);
                        goto start;
                    }

                default:
                    throw new NotImplementedException();
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

            List<TaskDefinition> normalTasks = this.Context.Tasks.Where(v => (PostTaskStrategy)v.PostTask == PostTaskStrategy.None).ToList();
            List<TaskDefinition> alwayPostTasks = this.Context.Tasks.Where(v => ((PostTaskStrategy)v.PostTask) == PostTaskStrategy.Always).ToList();
            List<TaskDefinition> onlyPassPostTasks = this.Context.Tasks.Where(v => ((PostTaskStrategy)v.PostTask) == PostTaskStrategy.OnlyPass).ToList();
            List<TaskDefinition> onlyFailPostTasks = this.Context.Tasks.Where(v => ((PostTaskStrategy)v.PostTask) == PostTaskStrategy.OnlyFail).ToList();

            this.ProcessEvent(TaskDispatcherEvent.Started, null);
            result.Add(this.ExecuteTasksFinally(normalTasks, out failureTask1));

            bool success = result.All(s => s);

            if (success)
            {
                List<TaskDefinition> failureTask3 = null;
                result.Add(this.ExecuteTasksFinally(onlyPassPostTasks, out failureTask3));
            }
            else
            {
                List<TaskDefinition> failureTask4 = null;
                result.Add(this.ExecuteTasksFinally(onlyFailPostTasks, out failureTask4));
            }

            // 最后执行PostTask
            result.Add(this.ExecuteTasksFinally(alwayPostTasks, out failureTask2));

            // PostTask也算总结果
            success = result.All(s => s);

            // 所有Task都运行完了再通知运行结束事件
            this.ProcessEvent(TaskDispatcherEvent.Completed, success);
        }

        private bool ExecuteTasksFinally(IEnumerable<TaskDefinition> taskList, out List<TaskDefinition> failureTasks)
        {
            List<bool> result = new List<bool>();
            failureTasks = new List<TaskDefinition>();

            foreach (TaskDefinition toExecute in taskList)
            {
                bool ret = DelegateUtility.ContinuousInvoke<TaskDefinition>(this.ExecuteTask, toExecute, toExecute.RetryTimes, toExecute.RetryInterval);

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

        private void HandleMessage(string msg, params object[] param)
        {
            logger.InfoFormat(msg, param);
            this.PubMessage(msg, param);
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
            TaskModuleStatus taskStatus = TaskModuleStatus.WAIT;

            try
            {
                // 使用缓存的Task实例
                task = this.taskFactory.LookupModule<TaskModule>(taskDef.ID);
                task.Context = this.Context;

                this.CurrentTask = task;

                // 如果Task有Disabled标记，那么跳过运行
                if (taskDef.HasFlag((int)TaskFlags.Disabled))
                {
                    taskStatus = TaskModuleStatus.SKIP;
                    this.ProcessTaskStatusChanged(TaskModuleStatus.SKIP, task);
                    this.HandleMessage("{0}未启用, 跳过", taskDef.Name);
                    return true;
                }

                // 延时运行
                if (taskDef.Delay > 0)
                {
                    taskStatus = TaskModuleStatus.WAIT;
                    this.ProcessTaskStatusChanged(TaskModuleStatus.WAIT, task);
                    this.HandleMessage("延时{0}秒运行{1}", (double)taskDef.Delay / (double)1000, taskDef.Name);
                    Thread.Sleep(taskDef.Delay);
                }

                taskStatus = TaskModuleStatus.RUN;
                this.ProcessTaskStatusChanged(TaskModuleStatus.RUN, task);
                this.HandleMessage("开始运行:{0}", taskDef.Name);

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
                    taskStatus = TaskModuleStatus.FAIL;
                    this.ProcessTaskStatusChanged(TaskModuleStatus.FAIL, task);
                    this.HandleMessage("初始化{0}失败, code = {1}, {2}", taskDef.Name, code, ResponseCode.GetMessage(code));
                    return false;
                }

                initialized = true;

                if ((code = task.Run()) != ResponseCode.SUCCESS)
                {
                    taskStatus = TaskModuleStatus.FAIL;
                    this.ProcessTaskStatusChanged(TaskModuleStatus.FAIL, task);
                    this.HandleMessage("运行{0}失败, code = {1}, {2}", taskDef.Name, code, ResponseCode.GetMessage(code));
                    return false;
                }

                taskStatus = TaskModuleStatus.PASS;
                this.ProcessTaskStatusChanged(TaskModuleStatus.PASS, task);
                this.HandleMessage("运行{0}成功", taskDef.Name);
                return true;
            }
            catch (Exception ex)
            {
                taskStatus = TaskModuleStatus.EXCEPTION;
                this.ProcessTaskStatusChanged(TaskModuleStatus.EXCEPTION, task);
                this.HandleMessage("{0}出现异常, {1}", taskDef.Name, ex);
                if (taskDef.HasFlag((int)TaskFlags.IgnoreFailure))
                {
                    this.HandleMessage("{0}运行出现异常, 执行下一个流程", taskDef.Name);
                }
                else
                {
                    this.HandleMessage("{0}运行出现异常, 退出", taskDef.Name);
                }
                return false;
            }
            finally
            {
                if (task != null)
                {
                    // 保存输出参数，供其他的任务解析输入表达式使用
                    this.Context.TaskProperties[task.ID] = task.Properties;

                    // 如果反射创建task失败，则会出现task为空
                    if (task != null)
                    {
                        this.Context.TaskResults.Add(new TaskResult()
                        {
                            Status = taskStatus,
                            TaskDefinition = taskDef,
                            Message = task.Message,
                            Properties = task.Properties
                        });
                    }

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
            this.Context.TaskResults.Clear();
            this.Context.CompletedBehavior = CompletedBehaviors.None;
            //this.Context.GloablParameters.Clear();
            this.asyncTasks.Clear();
        }

        #endregion

        #region 事件处理器

        #endregion
    }
}
