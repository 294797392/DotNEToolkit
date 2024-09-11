using DotNEToolkit.Modular;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Factory.NET
{
    /// <summary>
    /// 存储所有Task运行时的上下文数据
    /// </summary>
    public class DispatcherContext
    {
        /// <summary>
        /// 要执行的工作流列表
        /// </summary>
        public List<TaskDefinition> Tasks { get; set; }

        /// <summary>
        /// 项目的运行次数
        /// 取值范围：
        /// 小于0：运行无数次
        /// 0：只运行一次
        /// 大于0：指定运行次数
        /// </summary>
        public int Cycles { get; set; }

        /// <summary>
        /// 全局输入参数，由所有测试流程共享
        /// </summary>
        public Dictionary<string, object> GloablParameters { get; set; }

        /// <summary>
        /// 控制测试流程是否停止运行
        /// 如果设置为True，那么未完成的所有测试流程将不会继续运行，包括PostTask也不会运行
        /// </summary>
        public bool IsStop { get; set; }

        #region Internal属性

        /// <summary>
        /// 所有工作流的输入参数
        /// 如果输入参数里有表达式，那么存储表达式的值
        /// </summary>
        internal Dictionary<string, IDictionary> TaskInputs { get; private set; }

        /// <summary>
        /// 存储工作流中到目前为止所有的Task的输出
        /// 在重新开始运行工作流的时候，会清空
        /// 
        /// TaskID -> OutputParameters
        /// </summary>
        internal Dictionary<string, IDictionary> TaskProperties { get; private set; }

        /// <summary>
        /// 存储到目前为止已经运行了的测试项的测试结果
        /// </summary>
        internal List<TaskResult> TaskResults { get; private set; }

        #endregion

        public DispatcherContext()
        {
            this.TaskInputs = new Dictionary<string, IDictionary>();
            this.TaskProperties = new Dictionary<string, IDictionary>();
            this.GloablParameters = new Dictionary<string, object>();
            this.TaskResults = new List<TaskResult>();
        }
    }
}
