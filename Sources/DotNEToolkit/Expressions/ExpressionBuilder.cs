using DotNEToolkit.Expressions.Evaluators;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Expressions
{
    /// <summary>
    /// 表达式解析器
    /// 1. 把表达式字符串解析为一个树形结构
    /// 2. 对树形结构进行求值计算
    /// </summary>
    public class ExpressionParser
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("ExpressionParser");

        private static readonly string DefaultDefinitionFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "generic.exp.json");

        #endregion

        #region 实例变量

        private ParserState state;
        private StringEnumerator enumerator;

        private string paramter = string.Empty;

        /// <summary>
        /// 记录左括号的数量
        /// </summary>
        private int leftBracket;

        /// <summary>
        /// 记录右括号的数量
        /// </summary>
        private int rightBracket;

        private Expression root;
        private Expression exp;

        /// <summary>
        /// 缓存所有的表达式计算器
        /// </summary>
        private Dictionary<string, ExpressionEvaluator> evaluators;
        private List<ExpressionDefinition> definitions;

        #endregion

        #region 构造方法

        public ExpressionParser()
        {
            this.evaluators = new Dictionary<string, ExpressionEvaluator>();
            this.InitializeDefinitions();
        }

        #endregion

        #region Action

        private void ActionIgnore(char ch)
        {

        }

        private void ActionCollect(char ch)
        {
            this.paramter += ch;
        }

        private void ActionClear()
        {
            this.paramter = string.Empty;
        }

        #endregion

        #region EnterState

        private void EnterParamFunction()
        {
            this.state = ParserState.ParamFunction;
        }

        private void EnterParamStringState()
        {
            this.state = ParserState.ParamString;
            this.ActionClear();
        }

        private void EnterParamTermination(Expression currentParent)
        {
            this.exp = currentParent;
            this.state = ParserState.ParamTermination;
            this.ActionClear();
        }

        private void EnterGroundState(Expression currentParent)
        {
            this.exp = currentParent;
            this.state = ParserState.Ground;
            this.ActionClear();
        }

        private void EnterAccessMemberEntry(Expression currentParent)
        {
            this.exp = currentParent;
            this.state = ParserState.AccessMemberEntry;
            this.ActionClear();
        }

        private void EnterGrammarErrorState()
        {
            this.state = ParserState.GrammarError;
        }

        #endregion

        /// <summary>
        /// 遍历expression的父节点，直到父节点为函数类型的表达式为止
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        private Expression LookupFunctionParent(Expression expression)
        {
            Expression current = expression.Parent;

            while (current != null && current.Type != ExpressionTypes.Function)
            {
                current = current.Parent;
            }

            return current;
        }

        /// <summary>
        /// 判断是否是空格，如果是空格那么忽略掉
        /// </summary>
        private bool IsSpace(char ch)
        {
            return ch == ' ';
        }

        /// <summary>
        /// 是否是函数表达式字符
        /// </summary>
        /// <returns></returns>
        private bool IsParamFunctionIndicator(char ch)
        {
            return ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z' || ch >= '0' && ch <= '9';
        }

        private bool IsParamEndIndicator(char ch)
        {
            return ch == ')';
        }

        /// <summary>
        /// 判断是否开始解析函数参数
        /// 如果遇到了‘(’左括号，那么表示开始解析函数参数
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        private bool IsParamEntryIndicator(char ch)
        {
            return ch == '(';
        }

        /// <summary>
        /// 判断是否是有效的字符串参数
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        private bool IsParamStringValid(char ch)
        {
            return ch >= 32 && ch < 127;
        }

        /// <summary>
        /// 判断是否是字符串参数
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        private bool IsParamStringIndicator(char ch)
        {
            return ch == '\'';
        }

        /// <summary>
        /// 判断是否是参数分隔符
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        private bool IsParamSplitter(char ch)
        {
            return ch == ',';
        }

        /// <summary>
        /// 判断是否是成员访问符
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        private bool IsMemberEntryIndicator(char ch)
        {
            return ch == '.';
        }

        /// <summary>
        /// 判断是否是有效的成员参数
        /// </summary>
        /// <param name="ch">要判断的字符</param>
        /// <returns></returns>
        private bool IsParamMemberValid(char ch)
        {
            return ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z' || ch >= '0' && ch <= '9';
        }

        /// <summary>
        /// 判断字符是否是访问数组成员
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        private bool IsAccessArrayIndicator(char ch)
        {
            return ch == '[';
        }

        /// <summary>
        /// 判断字符是否是访问函数成员
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        private bool IsAccessFunctionIndicator(char ch)
        {
            return ch == '(';
        }

        #region Dispatch

        private Expression DispatchParamString(string param, Expression parent)
        {
            Expression expression = new Expression()
            {
                Name = param,
                State = ExpressionState.HasEvaluation,
                Value = param,
                Parent = parent,
                Type = ExpressionTypes.StringConstant
            };

            parent.Children.Add(expression);

            return expression;
        }

        private Expression DispatchParamFunction(string param, Expression parent)
        {
            // 遇到了一个新的函数，那么要新建一个表达式，并把当前表达式设置为这个新建的表达式
            Expression expression = new Expression()
            {
                Name = param,
                State = ExpressionState.WaitEvaluation,
                Parent = parent,
                Type = ExpressionTypes.Function
            };

            parent.Children.Add(expression);

            return expression;
        }

        private Expression DispatchAccessProperty(string property, Expression parent)
        {
            Expression expression = new Expression()
            {
                Name = property,
                State = ExpressionState.WaitEvaluation,
                Parent = parent,
                Type = ExpressionTypes.AccessProperty,
            };

            parent.Children.Add(expression);

            return expression;
        }

        private Expression DispatchAccessArray(string key, Expression parent)
        {
            Expression expression = new Expression()
            {
                Name = key,
                State = ExpressionState.WaitEvaluation,
                Parent = parent,
                Type = ExpressionTypes.AccessArray
            };

            parent.Children.Add(expression);

            return expression;
        }

        #endregion

        #region Event

        /// <summary>
        /// 基态
        /// 该函数处理以下几件事：
        /// 1. 忽略空格键
        /// 2. 判断是否是字符串参数，如果是则转到字符串参数状态
        /// 3. 判断是否是函数参数，如果是则转到函数参数状态
        /// 4. 判断是否是属性参数，如果是则转到属性参数状态
        /// 5. 如果是参数分隔符则清空当前的参数
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="parent"></param>
        private void EventGround(char ch, Expression parent)
        {
            if (this.IsSpace(ch))
            {
                this.ActionIgnore(ch);
            }
            else if (this.IsParamStringIndicator(ch))
            {
                // 是字符串参数
                this.EnterParamStringState();
            }
            else if (this.IsParamFunctionIndicator(ch))
            {
                // 是函数参数
                this.ActionCollect(ch);
                this.EnterParamFunction();
            }
            else
            {
                logger.WarnFormat("EventGround未处理的字符 = {0}, 位置 = {1}", ch, this.enumerator.CharPosition);
            }
        }

        /// <summary>
        /// 处理函数状态
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="parent"></param>
        private void EventParamFunction(char ch, Expression parent)
        {
            if (this.IsSpace(ch))
            {
                this.ActionIgnore(ch);
            }
            else if (this.IsParamFunctionIndicator(ch))
            {
                // 都是函数名，收集起来
                this.ActionCollect(ch);
            }
            else if (this.IsParamEntryIndicator(ch))
            {
                // 出现了左括号函数参数开始
                this.leftBracket++;
                Expression expression = this.DispatchParamFunction(this.paramter, parent);
                this.EnterGroundState(expression);
            }
            else
            {
                logger.WarnFormat("EventParamFunction未处理的字符 = {0}, 位置 = {1}", ch, this.enumerator.CharPosition);
            }
        }

        private void EventParamString(char ch, Expression parent)
        {
            if (this.IsParamStringIndicator(ch))
            {
                // 在字符串参数状态下又出现了'，说明是字符串参数结束了，返回函数状态
                Expression expression = this.DispatchParamString(this.paramter, parent);
                this.EnterParamTermination(expression);
            }
            else if (this.IsParamStringValid(ch))
            {
                this.ActionCollect(ch);
            }
            else
            {
                logger.WarnFormat("EventParamString未处理的字符, {0}, 位置 = {1}", ch, this.enumerator.CharPosition);
            }
        }

        /// <summary>
        /// 处理ParamTermination状态下出现的字符
        /// 字符串状态下出现了第二个单引号触发该状态
        /// 
        /// 该函数做以下几件事：
        /// 1. 判断如果出现的左括号的次数等于右括号的次数，那么说明整个函数解析完了，跳转到基态
        /// 2. 判断函数结束后面是否有成员访问符
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="parent"></param>
        private void EventParamTermination(char ch, Expression parent)
        {
            if (this.IsSpace(ch))
            {
                this.ActionIgnore(ch);
            }
            else if (this.IsParamEndIndicator(ch))
            {
                // 出现了右括号
                this.rightBracket++;
                if (this.leftBracket == this.rightBracket)
                {
                    // 左括号数和右括号数匹配，说明一个函数解析完了
                    this.leftBracket = 0;
                    this.rightBracket = 0;

                    // 跳转到基态的时候顺便把当前解析的表达式设置为上一个层级
                }
                else
                {
                    // 还在解析子参数...
                }

                this.exp = parent.Parent;
            }
            else if (this.IsParamSplitter(ch))
            {
                this.EnterGroundState(parent);
            }
            else if (this.IsMemberEntryIndicator(ch))
            {
                this.EnterAccessMemberEntry(parent);
            }
            else
            {
                logger.WarnFormat("EventParamTermination未处理的字符, {0}, 位置 = {1}", ch, this.enumerator.CharPosition);
            }
        }

        private void EventAccessMemberEntry(char ch, Expression parent)
        {
            if (this.IsSpace(ch))
            {
                this.ActionIgnore(ch);
            }
            //else if (this.IsAccessArrayIndicator(ch))
            //{
            //    string key;
            //    if (this.enumerator.MoveNext(']', out key))
            //    {
            //        this.DispatchAccessArray(key, parent);
            //    }
            //    else
            //    {
            //        // 没找到数组结束符，那么说明语法错误
            //        this.EnterGrammarErrorState();
            //    }
            //}
            //else if (this.IsAccessFunctionIndicator(ch))
            //{
            //    // 目前只支持访问没有参数的函数
            //    string param;
            //    if (this.enumerator.MoveNext(')', out param) && string.IsNullOrEmpty(param))
            //    {
            //    }
            //    else
            //    {
            //        // 语法错误
            //        this.EnterGrammarErrorState();
            //    }
            //}
            else if (this.IsParamSplitter(ch))
            {
                // 出现了逗号，说明成员访问结束
                this.DispatchAccessProperty(this.paramter, parent);

                Expression expression = this.LookupFunctionParent(parent);
                this.EnterGroundState(expression);
            }
            else if (this.IsParamEndIndicator(ch))
            {
                // 出现了右括号，说明函数结束了
                this.DispatchAccessProperty(this.paramter, parent);

                this.rightBracket++;

                // 直接跳转到参数结束状态去处理
                Expression expression = this.LookupFunctionParent(parent);
                this.EnterParamTermination(expression);
            }
            else if (this.IsMemberEntryIndicator(ch))
            {
                // 属性参数状态下又出现了一个点，那么说明是访问属性的属性
                // 先创建一个对于上一个属性的表达式节点，然后继续按照属性参数状态解析
                Expression expression = this.DispatchAccessProperty(this.paramter, parent);

                // 把父节点设置为属性节点
                this.EnterAccessMemberEntry(expression);
            }
            else if (this.IsParamMemberValid(ch))
            {
                // 出现的是成员字符串，收集起来
                this.ActionCollect(ch);
            }
            else
            {
                logger.WarnFormat("EventParamMember未处理的字符, {0}, 位置 = {1}", ch, this.enumerator.CharPosition);
            }
        }

        private void EventGrammarError(char ch)
        {

        }

        #endregion

        #region 公开接口

        /// <summary>
        /// 构造表达式树形列表
        /// </summary>
        /// <param name="expression"></param>
        public Expression BuildExpressionTree(string expression)
        {
            if (expression.Length == 0 || expression[0] != '$')
            {
                return null;
            }

            string text = expression.Substring(1);
            this.exp = this.root = new Expression();
            this.state = ParserState.Ground;
            this.enumerator = new StringEnumerator(text);

            while (this.enumerator.MoveNext())
            {
                char ch = this.enumerator.Current;

                switch (this.state)
                {
                    case ParserState.Ground:
                        {
                            this.EventGround(ch, this.exp);
                            break;
                        }

                    case ParserState.ParamFunction:
                        {
                            this.EventParamFunction(ch, this.exp);
                            break;
                        }

                    case ParserState.ParamString:
                        {
                            this.EventParamString(ch, this.exp);
                            break;
                        }

                    case ParserState.ParamTermination:
                        {
                            this.EventParamTermination(ch, this.exp);
                            break;
                        }

                    case ParserState.AccessMemberEntry:
                        {
                            this.EventAccessMemberEntry(ch, this.exp);
                            break;
                        }

                    case ParserState.GrammarError:
                        {
                            this.EventGrammarError(ch);
                            break;
                        }

                    default:
                        logger.WarnFormat("未处理的状态:{0}", this.state);
                        break;
                }
            }

            if (this.state == ParserState.AccessMemberEntry && !string.IsNullOrEmpty(this.paramter))
            {
                this.DispatchAccessProperty(this.paramter, this.exp);
            }

            return this.root;
        }

        /// <summary>
        /// 对表达式树进行求值
        /// 从树形结构的儿子节点递归计算表达式的值，直到把根节点的值计算出来
        /// </summary>
        /// <param name="parent">要计算的表达式根节点/param>
        /// <paramref name="context">计算表达式的时候需要的额外数据</paramref>
        /// <returns></returns>
        public object Evaluate(Expression parent, IEvaluationContext context)
        {
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
                                // 有子节点，先计算子节点
                                object result = this.Evaluate(expression, context);
                                parent.Parameters.Add(result);
                                break;
                            }
                            else
                            {
                                // 没有子节点，直接计算
                                object result = this.EvaluateExpression(expression, context);
                                parent.Parameters.Add(result);
                                break;
                            }
                        }

                    default:
                        throw new NotImplementedException();
                }
            }

            return this.EvaluateExpression(parent, context);
        }

        #endregion

        #region 实例方法

        private void InitializeDefinitions()
        {
            logger.InfoFormat("开始加载表达式定义文件, {0}", DefaultDefinitionFile);

            try
            {
                this.definitions = JSONHelper.ParseFile<List<ExpressionDefinition>>(DefaultDefinitionFile);
            }
            catch (Exception ex)
            {
                logger.Error("加载表达式定义文件异常", ex);
                this.definitions = new List<ExpressionDefinition>();
            }

            logger.InfoFormat("当前系统里注册的表达式总数 = {0}", this.definitions.Count);
        }

        /// <summary>
        /// 根据表达式名字获取表达式求值程序的实例
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
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
                    evaluator = ConfigFactory<ExpressionEvaluator>.CreateInstance(expDef.ClassName);
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

        /// <summary>
        /// 计算指定表达式的值
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private object EvaluateExpression(Expression expression, IEvaluationContext context)
        {
            ExpressionEvaluator evaluator = this.GetEvaluator(expression.Name);
            return evaluator == null ? null : evaluator.Evaluate(expression, context);
        }

        #endregion
    }
}

