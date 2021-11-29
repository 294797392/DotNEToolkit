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
    public class ExpressionParser
    {
        #region 类变量

        private static log4net.ILog logger = log4net.LogManager.GetLogger("ExpressionParser");

        #endregion

        #region 实例变量

        private ParserState state;
        private StringEnumerator enumerator;

        /// <summary>
        /// 当前解析的字符的索引位置
        /// </summary>
        private int charIndex;

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


        #endregion

        #region 构造方法

        public ExpressionParser()
        {
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

        private void EnterParamMemberState(Expression currentParent)
        {
            this.exp = currentParent;
            this.state = ParserState.ParamMemberAccess;
            this.ActionClear();
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

        private Expression DispatchParamMember(string property, Expression parent)
        {
            Expression expression = new Expression()
            {
                Name = property,
                State = ExpressionState.WaitEvaluation,
                Parent = parent,
                Type = ExpressionTypes.Property,
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
                logger.WarnFormat("EventGround未处理的字符 = {0}", ch);
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
            else if (this.IsParamStringIndicator(ch))
            {
                // 出现了单引号，说明是一个字符串参数，跳转到字符串参数状态
                this.EnterParamStringState();
            }
            else
            {
                logger.WarnFormat("EventParamFunction未处理的字符 = {0}, 索引 = {1}", ch, this.charIndex);
            }
        }

        private void EventParamString(char ch, Expression parent)
        {
            if (this.IsParamStringValid(ch))
            {
                // 在字符串参数状态下又出现了'，说明是字符串参数结束了，返回函数状态
                if (this.IsParamStringIndicator(ch))
                {
                    Expression expression = this.DispatchParamString(this.paramter, parent);
                    this.EnterParamTermination(expression);
                }
                else
                {
                    this.ActionCollect(ch);
                }
            }
            else
            {
                logger.WarnFormat("EventParamString未处理的字符, {0}, 索引 = {1}", ch, this.charIndex);
                //this.ActionCollect(ch);
            }
        }

        private void EventParamMember(char ch, Expression parent)
        {
            if (this.IsSpace(ch))
            {
                this.ActionIgnore(ch);
            }
            else if (this.IsParamSplitter(ch))
            {
                // 出现了逗号，说明成员访问结束
                this.DispatchParamMember(this.paramter, parent);

                Expression expression = this.LookupFunctionParent(parent);
                this.EnterGroundState(expression);
            }
            else if (this.IsParamEndIndicator(ch))
            {
                // 出现了右括号，说明函数结束了
                this.DispatchParamMember(this.paramter, parent);

                this.rightBracket++;

                // 直接跳转到参数结束状态去处理
                Expression expression = this.LookupFunctionParent(parent);
                this.EnterParamTermination(expression);
            }
            else if (this.IsMemberEntryIndicator(ch))
            {
                // 属性参数状态下又出现了一个点，那么说明是访问属性的属性
                // 先创建一个对于上一个属性的表达式节点，然后继续按照属性参数状态解析
                Expression expression = this.DispatchParamMember(this.paramter, parent);

                // 把父节点设置为属性节点
                this.EnterParamMemberState(expression);
            }
            else if (this.IsParamMemberValid(ch))
            {
                // 出现的是成员字符串，收集起来
                this.ActionCollect(ch);
            }
            else
            {
                logger.WarnFormat("EventParamMember未处理的字符, {0}, 索引 = {1}", ch, this.charIndex);
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
                this.EnterParamMemberState(parent);
            }
            else
            {
                logger.WarnFormat("EventParamTermination未处理的字符, {0}, 索引 = {1}", ch, this.charIndex);
            }
        }

        #endregion

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
            this.charIndex = 0;

            while (this.enumerator.MoveNext())
            {
                this.charIndex++;

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

                    case ParserState.ParamMemberAccess:
                        {
                            this.EventParamMember(ch, this.exp);
                            break;
                        }

                    case ParserState.ParamTermination:
                        {
                            this.EventParamTermination(ch, this.exp);
                            break;
                        }

                    default:
                        logger.WarnFormat("未处理的状态:{0}", this.state);
                        break;
                }
            }

            if (this.state == ParserState.ParamMemberAccess && !string.IsNullOrEmpty(this.paramter))
            {
                this.DispatchParamMember(this.paramter, this.exp);
            }

            return this.root;
        }
    }
}

