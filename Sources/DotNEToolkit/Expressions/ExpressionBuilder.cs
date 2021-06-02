using DotNEToolkit.Expressions.Evaluators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Expressions
{
    /// <summary>
    /// 参数表达式编译器
    /// 
    /// 1. 把表达式转换成一个树形结构
    /// 2. 从树形结构的儿子节点递归计算表达式的值，直到把根节点的值计算出来
    /// </summary>
    public class ExpressionBuilder
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("ExpressionBuilder");

        #endregion

        public event Action<object, string> Message;

        #region 实例变量

        private IEnumerable<ExpressionDefinition> definitions;
        private Dictionary<string, ExpressionEvaluator> evaluators;

        #endregion

        #region 属性

        public static ExpressionBuilder Instance = new ExpressionBuilder();

        #endregion

        private ExpressionBuilder()
        {
            this.definitions = JSONHelper.DeserializeJSONFile<ExpressionDefinition>(AppDomain.CurrentDomain.BaseDirectory, "expressions.json");
            this.evaluators = new Dictionary<string, ExpressionEvaluator>();
        }

        #region 公开接口

        /// <summary>
        /// 计算指定表达式的值
        /// </summary>
        /// <param name="expression">
        /// concat('text1', 'text2')
        /// concat(parameter('text', 'id'), 'text2')
        /// </param>
        /// <param name="context">
        /// 计算表达式需要的上下文数据
        /// </param>
        /// <returns></returns>
        public int Evaluate(string expression, EvaluationContext ctx, out object result)
        {
            string expr = expression.Substring(1);
            Expression exp = this.BuildExpressionTree(expression);
            return this.EvaluateExpression(exp, ctx, out result);
        }

        public int Evaluate<TResult>(string expression, EvaluationContext ctx, out TResult result)
        {
            result = default(TResult);

            object o;
            int rc = this.Evaluate(expression, ctx, out o);
            if (rc == DotNETCode.Success)
            {
                result = (TResult)o;
            }
            return rc;
        }

        /// <summary>
        /// 判断一个字符串是否是表达式类型
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool IsExpression(string text)
        {
            return !string.IsNullOrEmpty(text) && text.Length > 3 && text[0] == '$' && text[1] != '$';
        }

        #endregion

        #region 实例方法

        #region 构建表达式树形结构

        /// <summary>
        /// 构造表达式树
        /// </summary>
        /// <param name="exp"></param>
        /// <returns>表达式树根节点</returns>
        public Expression BuildExpressionTree(string exp)
        {
            Expression root = new Expression(); // 表达式根
            int name_len = exp.IndexOf('(');
            if (name_len != -1)
            {
                // 是普通表达式
                root.Name = exp.Substring(1, name_len - 1);
                root.ExpressionText = exp.Substring(name_len + 1, exp.Length - name_len - 2);
                this.BuildChildren(root);
            }
            else if (exp[1] == '{')
            {
                // 是一个宏定义
                int endIdx = exp.IndexOf('}');
                root.Name = "macro";
                root.State = ExpressionState.WaitEvaluation;
                root.Type = ExpressionTypes.MacroDefinition;
                root.ExpressionText = exp.Substring(2, endIdx - 2);
            }
            else
            {
                logger.ErrorFormat("不识别的表达式:{0}", exp);
            }
            return root;
        }

        /// <summary>
        /// 构建父表达式的子节点表达式
        /// </summary>
        /// <param name="text">
        /// 表达式分以下几种情况，解析器需要处理以下所有情况
        /// 表达式有可能会包含子表达式，是无限级递归结构
        /// 1. 'asd'
        /// 2. parameter('1')
        /// 3. concat('12','23')
        /// 4. 'a,b,c'
        /// 5. concat(parameter('a'), parameter('b'), 'c', 'd')
        /// </param>
        /// <returns>
        /// 返回解析了字符串的值
        /// </returns>
        private void BuildChildren(Expression parent)
        {
            List<Expression> expressions = this.SpliteExpressions(parent.ExpressionText);
            foreach (Expression expression in expressions)
            {
                switch (expression.Type)
                {
                    case ExpressionTypes.Function:
                        this.BuildChildren(expression);
                        break;

                    default:
                        break;
                }
            }
            parent.Children.AddRange(expressions);
        }

        /// <summary>
        /// 解析表达式数组
        /// 把表达式数组按照逗号分隔并返回
        /// </summary>
        /// <param name="parentExp">
        /// parameter('a'), parameter('b,3,c,3'), 'c', 'd', '2', parameter('abcd')
        /// </param>
        private List<Expression> SpliteExpressions(string parentExp)
        {
            List<Expression> expressions = new List<Expression>();

            ParserContext context = new ParserContext(parentExp.Trim());
            context.UserData = expressions;
            context.ExpressionReceived += this.ParserStateProcessor_ExpressionReceived;

            Dictionary<char, AbstractStateHandler> stateMap = ParserStateMaps.IDLEStateMap;

            for (int index = 0; index < context.Expression.Length; index++)
            {
                char current = parentExp[index];
                context.CurrentChar = current;
                context.CurrentIndex = index;
                context.RemainChar = context.Expression.Length - index - 1;

                AbstractStateHandler ash;
                if (!stateMap.TryGetValue(current, out ash))
                {
                    logger.ErrorFormat("没有{0}的处理器", current);
                    throw new NotImplementedException();
                }

                stateMap = ash.Process(context);

                if (context.CurrentIndex != index)
                {
                    index = context.CurrentIndex;
                }
            }

            context.ExpressionReceived -= this.ParserStateProcessor_ExpressionReceived;

            return expressions;
        }

        private void ParserStateProcessor_ExpressionReceived(string expression, int startIndex, int endIndex, ExpressionTypes type, object userdata)
        {
            List<Expression> expressions = userdata as List<Expression>;

            switch (type)
            {
                case ExpressionTypes.Function:
                    {
                        int name_len = expression.IndexOf('(');
                        string name = expression.Substring(0, name_len);
                        string content = expression.Substring(name_len + 1, expression.Length - name_len - 2);

                        expression = expression.Trim();

                        expressions.Add(new Expression()
                        {
                            StartIndex = startIndex,
                            EndIndex = endIndex,
                            Name = name,
                            ExpressionText = content,
                            State = ExpressionState.WaitEvaluation,
                            Type = ExpressionTypes.Function
                        });
                        logger.InfoFormat("解析函数表达式:{0}, {1}", name, content);
                        break;
                    }

                case ExpressionTypes.MacroDefinition:
                    {
                        break;
                    }

                case ExpressionTypes.StringConstant:
                    {
                        expressions.Add(new Expression()
                        {
                            StartIndex = startIndex,
                            EndIndex = endIndex,
                            Name = "string",
                            ExpressionText = expression,
                            Value = expression,
                            State = ExpressionState.HasEvaluation,
                            Type = ExpressionTypes.StringConstant
                        });
                        logger.InfoFormat("解析字符串表达式:{0}", expression);
                        break;
                    }
            }
        }

        #endregion

        #region 对表达式树进行求值

        private int EvaluateExpression(Expression parent, EvaluationContext context, out object result)
        {
            result = null;

            foreach (Expression expression in parent.Children)
            {
                switch (expression.State)
                {
                    case ExpressionState.HasEvaluation:
                        parent.Parameters.Add(expression.Value);
                        break;

                    case ExpressionState.WaitEvaluation:
                        {
                            if (expression.Children.Count > 0)
                            {
                                object valueObject;
                                int rc = this.EvaluateExpression(expression, context, out valueObject);
                                if (rc != DotNETCode.Success)
                                {
                                    return rc;
                                }
                                parent.Parameters.Add(valueObject);
                                break;
                            }
                            else
                            {
                                object resultObject;
                                int rc = this.GetEvaluator(expression.Name).Evaluate(expression, context, out resultObject);
                                if (rc != DotNETCode.Success)
                                {
                                    return rc;
                                }
                                parent.Parameters.Add(expression.Value);
                                break;
                            }
                        }

                    default:
                        throw new NotImplementedException();
                }
            }

            return this.GetEvaluator(parent.Name).Evaluate(parent, context, out result);
        }

        #endregion

        private ExpressionEvaluator GetEvaluator(string name)
        {
            ExpressionEvaluator evaluator;
            if (!this.evaluators.TryGetValue(name, out evaluator))
            {
                ExpressionDefinition expDef = this.definitions.FirstOrDefault(def => def.Name == name);
                if (expDef == null)
                {
                    logger.ErrorFormat("不存在{0}的求值程序", name);
                    return null;
                }

                try
                {
                    evaluator = ConfigFactory<ExpressionEvaluator>.CreateInstance(expDef.EntryClass);
                    evaluator.Name = expDef.Name;
                    evaluator.Description = expDef.Description;
                    evaluator.Category = expDef.Category;
                    evaluator.MinimalParameters = expDef.MinimalParameters;
                    evaluator.Message += this.Evaluator_Message;
                }
                catch (Exception ex)
                {
                    logger.Error(string.Format("实例化{0}求值程序异常", name), ex);
                    return null;
                }

                this.evaluators[name] = evaluator;
            }
            return evaluator;
        }

        private void Evaluator_Message(object sender, string message)
        {
            if (this.Message != null)
            {
                this.Message(this, message);
            }
        }

        #endregion
    }
}
