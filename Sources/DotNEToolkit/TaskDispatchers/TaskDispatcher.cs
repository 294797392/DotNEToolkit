using DotNEToolkit.Expressions;
using DotNEToolkit.Modular;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace DotNEToolkit.TaskDispatchers
{
    /// <summary>
    /// 工作流/状态机执行引擎
    /// </summary>
    public class TaskDispatcher
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("TaskDispatcher");

        #endregion 

        #region 事件

        /// <summary>
        /// 任务调度器状态改变
        /// </summary>
        public event Action<TaskDispatcher, TaskDispatcherStatus> StatusChanged;

        /// <summary>
        /// 任务状态改变
        /// </summary>
        public event Action<TaskDispatcher, WorkflowStatus, WorkflowTask> TaskStatusChanged;

        #endregion

        #region 实例变量

        /// <summary>
        /// 保存每个任务在运行期间的属性
        /// </summary>
        private Dictionary<string, IDictionary> taskProperties;

        /// <summary>
        /// 执行任务的队列
        /// </summary>
        private TaskQueue queue;

        #endregion

        #region 属性

        /// <summary>
        /// 表达式上下文信息
        /// </summary>
        public IEvaluationContext EvaluationContext { get; internal set; }

        /// <summary>
        /// 模块工厂
        /// </summary>
        public ModuleFactory ModuleFactory { get; internal set; }

        /// <summary>
        /// 任务触发器列表
        /// </summary>
        public List<TaskTrigger> TaskTriggers { get; internal set; }

        /// <summary>
        /// 要执行的任务列表
        /// </summary>
        public List<TaskDefinition> TaskList { get; internal set; }

        /// <summary>
        /// 表示在所有任务运行完后，要运行的任务（不管之前的任务运行失败与否都会运行）
        /// </summary>
        public List<TaskDefinition> PostTasks { get; internal set; }

        /// <summary>
        /// 任务队列类型
        /// </summary>
        public TaskQueueType QueueType { get; internal set; }

        /// <summary>
        /// 表示任务队列的运行次数
        /// </summary>
        public Repetitions Repetitions { get; internal set; }

        /// <summary>
        /// 如果Repetitions == Continuously
        /// 那么该字段指定任务的运行次数
        /// </summary>
        public int Repeats { get; internal set; }

        #endregion

        #region 公开接口

        public void Initialize()
        {
            this.taskProperties = new Dictionary<string, IDictionary>();

            this.queue = TaskQueueFactory.Create(this.QueueType);
            this.queue.TaskList = this.TaskList;
            this.queue.Initialize();
        }

        /// <summary>
        /// 执行工作流
        /// </summary>
        /// <returns></returns>
        public int Start()
        {
            switch (this.Repetitions)
            {
                case Repetitions.Continuously:
                    {
                        if (this.Repeats == 0)
                        {
                            while (true)
                            {
                                this.Reset();
                                this.ExecuteTasks(this.TaskList, this.PostTasks);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < this.Repeats; i++)
                            {
                                this.Reset();
                                this.ExecuteTasks(this.TaskList, this.PostTasks);
                            }
                            break;
                        }
                    }

                case Repetitions.Single:
                    {
                        this.Reset();
                        this.ExecuteTasks(this.TaskList, this.PostTasks);
                        break;
                    }

                default:
                    throw new NotImplementedException();
            }

            return DotNETCode.SUCCESS;
        }

        #endregion

        #region 实例方法

        /// <summary>
        /// 执行工作流
        /// </summary>
        /// <param name="tasks">正常状态的工作流</param>
        /// <param name="postTasks">后期执行的工作流，无论失败或成功，最后都会执行</param>
        private void ExecuteTasks(IEnumerable<TaskDefinition> tasks, IEnumerable<TaskDefinition> postTasks)
        {
            /* 存储运行失败了的Activity */
            List<TaskDefinition> failureTasks = null;
            List<TaskDefinition> failureTasks2 = null;
            List<bool> result = new List<bool>();

            this.ProcessStatusChanged(TaskDispatcherStatus.RUN);
            result.Add(this.ExecuteTasksFinally(tasks, out failureTasks));
            result.Add(this.ExecutePostTasks(postTasks, out failureTasks2));

            if (result.All(s => s))
            {
                this.ProcessStatusChanged(TaskDispatcherStatus.PASS);
            }
            else
            {
                this.ProcessStatusChanged(TaskDispatcherStatus.FAIL);
            }
        }

        private bool ExecuteTasksFinally(IEnumerable<TaskDefinition> taskInfos, out List<TaskDefinition> failureTasks)
        {
            List<bool> result = new List<bool>();
            failureTasks = new List<TaskDefinition>();

            TaskDefinition toExecute;
            while ((toExecute = this.queue.Dequeue()) != null)
            {
                bool ret = DelegateUtility.ContinuousInvoke<TaskDefinition>(this.ExecuteTask, toExecute, toExecute.RetryTimes);

                // 运行触发器逻辑
                if (!this.ExecuteTrigger(toExecute))
                {
                    // 触发器执行失败直接退出
                    return false;
                }

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
            List<bool> result = new List<bool>();
            failureTasks = new List<TaskDefinition>();
            foreach (TaskDefinition task in postTasks)
            {
                bool ret = DelegateUtility.ContinuousInvoke<TaskDefinition>(this.ExecuteTask, task, task.RetryTimes);

                // 运行触发器逻辑
                this.ExecuteTrigger(task);
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
            WorkflowTask task = null;
            bool initialized = false;
            int rc = DotNETCode.SUCCESS;

            try
            {
                task = this.ModuleFactory.CreateInstance<WorkflowTask>(taskDef);

                // 如果Task有Disabled标记，那么跳过运行
                if (taskDef.HasFlag(ModuleFlags.Disabled))
                {
                    this.ProcessTaskStatusChanged(WorkflowStatus.SKIP, task);
                    logger.InfoFormat("{0}未启用, 跳过", taskDef.Name);
                    return true;
                }

                // 延时运行
                if (taskDef.Delay > 0)
                {
                    logger.InfoFormat("延时{0}秒运行{1}", (double)taskDef.Delay / (double)1000, taskDef.Name);
                    this.ProcessTaskStatusChanged(WorkflowStatus.WAIT, task);
                    Thread.Sleep(taskDef.Delay);
                }

                logger.InfoFormat("开始运行:{0}", taskDef.Name);

                this.ProcessTaskStatusChanged(WorkflowStatus.RUN, task);

                // 解析参数表达式
                IDictionary inputParams;
                if ((inputParams = ExpressionUtility.EvaluateExpressions(taskDef.InputParameters, this.EvaluationContext)) == null)
                {
                    this.ProcessTaskStatusChanged(WorkflowStatus.FAIL, task);
                    logger.ErrorFormat("计算输入参数表达式失败, 运行任务失败, code = {0}, {1}", rc, DotNETCode.GetMessage(rc));
                    return false;
                }

                if ((rc = task.Initialize(inputParams)) != DotNETCode.SUCCESS)
                {
                    this.ProcessTaskStatusChanged(WorkflowStatus.FAIL, task);
                    logger.ErrorFormat("初始化{0}失败, code = {1}, {2}", taskDef.Name, rc, DotNETCode.GetMessage(rc));
                    return false;
                }

                initialized = true;

                if ((rc = task.Run()) != DotNETCode.SUCCESS)
                {
                    this.ProcessTaskStatusChanged(WorkflowStatus.FAIL, task);
                    logger.ErrorFormat("运行{0}失败, code = {1}, {2}", taskDef.Name, rc, DotNETCode.GetMessage(rc));
                    return false;
                }

                this.ProcessTaskStatusChanged(WorkflowStatus.PASS, task);
                logger.InfoFormat("运行{0}成功", taskDef.Name);
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("{0}出现异常", taskDef.Name), ex);
                if (taskDef.HasFlag((int)TaskFlags.IgnoreFailure))
                {
                    logger.WarnFormat("{0}运行出现异常, 执行下一个流程", taskDef.Name);
                }
                else
                {
                    logger.WarnFormat("{0}运行出现异常, 退出", taskDef.Name);
                }
                this.ProcessTaskStatusChanged(WorkflowStatus.EXCEPTION, task);
                return false;
            }
            finally
            {
                if (task != null)
                {
                    // 保存输出参数，供其他的任务解析输入表达式使用
                    this.taskProperties[task.ID] = task.Properties;

                    if (initialized)
                    {
                        task.Reset();
                    }
                }
            }
        }

        /// <summary>
        /// 执行指定Task的触发器
        /// </summary>
        /// <param name="task">当前运行完了的Task</param>
        /// <returns>返回Activity的所有触发器是否执行成功</returns>
        private bool ExecuteTrigger(TaskDefinition task)
        {
            List<TaskTrigger> triggers = this.TaskTriggers.Where(v => v.TaskID == task.ID).ToList();
            if (triggers == null || triggers.Count == 0)
            {
                logger.DebugFormat("{0}不存在触发器，跳过", task.Name);
                return true;
            }

            logger.DebugFormat("{0}存在触发器，执行触发器", task.Name);

            foreach (TaskTrigger trigger in triggers)
            {
                bool canExecute = this.CheckTriggerConditions(task, trigger);

                if (canExecute)
                {
                    logger.DebugFormat("开始执行触发器:{0}", trigger.Name);

                    if (!this.ExecuteTriggerFinal(trigger))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 执行指定的触发器
        /// </summary>
        /// <param name="trigger">要执行的触发器</param>
        /// <returns>返回触发器执行是否成功</returns>
        private bool ExecuteTriggerFinal(TaskTrigger trigger)
        {
            foreach (TaskDefinition task in trigger.TaskList)
            {
                // 触发器触发的Task也执行重试逻辑
                bool ret = DelegateUtility.ContinuousInvoke<TaskDefinition>(this.ExecuteTask, task, task.RetryTimes);
                if (!ret)
                {
                    // 执行失败，
                    return false;
                }
                else
                {
                    // 执行成功，继续执行下一个
                }
            }

            return true;
        }

        /// <summary>
        /// 检查触发器是否达到了触发条件
        /// </summary>
        /// <param name="ivtask"></param>
        /// <param name="trigger"></param>
        /// <returns></returns>
        private bool CheckTriggerConditions(TaskDefinition task, TaskTrigger trigger)
        {
            logger.DebugFormat("开始检查{0}的输出触发器", task.Name);

            IDictionary properties;
            if (!this.taskProperties.TryGetValue(task.ID, out properties))
            {
                logger.DebugFormat("流程{0}的{1}触发器失败", task.Name, trigger.Name);
                return false;
            }

            if (!properties.Contains(trigger.Property))
            {
                logger.DebugFormat("流程{0}的{1}触发条件触发失败, PropertyKey不存在:{2}", task.Name, trigger.Name, trigger.Property);
                return false;
            }

            // TODO: 根据不同的值类型做不同的判断，比如int，枚举
            if (properties[trigger.Property].ToString() != trigger.Value.ToString())
            {
                logger.DebugFormat("流程{0}的{1}触发条件触发失败, PropertyValue不同:{2}", task.Name, trigger.Name, properties[trigger.Property]);
                return false;
            }

            return true;
        }

        private void ProcessTaskStatusChanged(WorkflowStatus status, WorkflowTask task)
        {
            task.Properties[WorkflowProperties.WP_STATUS] = status;

            if (this.TaskStatusChanged != null)
            {
                this.TaskStatusChanged(this, status, task);
            }
        }

        private void ProcessStatusChanged(TaskDispatcherStatus status)
        {
            if (this.StatusChanged != null)
            {
                this.StatusChanged(this, status);
            }
        }

        private void Reset()
        {
            this.taskProperties.Clear();
            this.queue.Reset();
        }

        #endregion
    }
}
