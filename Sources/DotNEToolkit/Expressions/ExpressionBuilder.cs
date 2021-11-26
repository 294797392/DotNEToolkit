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
        private CharEnumerator enumerator;

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

        private void EnterFunctionState()
        {
            this.state = ParserState.ParamFunction;
        }

        private void EnterParamStringState()
        {
            this.state = ParserState.ParamString;
            this.ActionClear();
        }

        private void EnterGroundState(Expression exp)
        {
            this.exp = exp;
            this.state = ParserState.Ground;
            this.ActionClear();
        }

        private void EnterParamEnd()
        {
            this.state = ParserState.ParamEnd;
        }

        #endregion

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

        private bool IsParamEndIndicator(char ch)
        {
            return ch == ')';
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

        private bool IsFunctionNameValid(char ch)
        {
            return ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z' || ch >= '0' && ch <= '9'; ;
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

        #region Dispatch

        private void DispatchParamString(string param, Expression parent)
        {
            Expression expression = new Expression()
            {
                Name = param,
                State = ExpressionState.HasEvaluation,
                Value = param,
                Parent = parent
            };

            parent.Children.Add(expression);
        }

        private void DispatchFunctionName(string param, Expression parent)
        {
            // 遇到了一个新的函数，那么要新建一个表达式，并把当前表达式设置为这个新建的表达式
            Expression expression = new Expression()
            {
                Name = param,
                State = ExpressionState.WaitEvaluation,
                Parent = parent
            };

            parent.Children.Add(expression);

            this.exp = expression;
        }

        private void ActionParamEnd(char ch, Expression parent)
        {
            this.rightBracket++;
            if (this.leftBracket == this.rightBracket)
            {
                // 左括号数和右括号数匹配，说明一个参数解析完了
                this.leftBracket = 0;
                this.rightBracket = 0;
                // 跳转到基态的时候顺便把当前解析的表达式设置为上一个层级
                this.EnterGroundState(parent.Parent == null ? parent : parent.Parent);
            }
            else
            {
                // 还在解析子参数...
            }
        }

        #endregion

        #region Event

        /// <summary>
        /// 基态
        /// 该函数处理以下几件事：
        /// 1. 忽略空格键
        /// 2. 判断是否是字符串参数，如果是则转到字符串参数状态
        /// 3. 判断是否是函数参数，如果是则转到函数参数状态
        /// 4. 如果是参数分隔符则清空当前的参数
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
                this.EnterFunctionState();
            }
            else if (this.IsParamSplitter(ch))
            {
                // 参数分隔符
                this.ActionClear();
            }
            else
            {
                logger.WarnFormat("EventGround未处理的字符 = {0}", ch);
            }
        }

        private void EventParamFunction(char ch, Expression parent)
        {
            if (this.IsSpace(ch))
            {
                this.ActionIgnore(ch);
            }
            else if (this.IsFunctionNameValid(ch))
            {
                // 都是函数名，收集起来
                this.ActionCollect(ch);
            }
            else if (this.IsParamEntryIndicator(ch))
            {
                // 出现了左括号函数参数开始
                this.leftBracket++;
                this.DispatchFunctionName(this.paramter, parent);
                this.ActionClear();
                this.EnterFunctionState();
            }
            else if (this.IsParamEndIndicator(ch))
            {
                // 出现了右括号函数参数结束
                this.EnterParamEnd();
            }
            else if (this.IsParamStringIndicator(ch))
            {
                // 出现了单引号，说明是一个字符串参数，跳转到字符串参数状态
                this.EnterParamStringState();
            }
            else
            {
                logger.WarnFormat("EventFunction未处理的字符 = {0}", ch);
            }
        }

        private void EventParamString(char ch, Expression parent)
        {
            if (this.IsParamStringValid(ch))
            {
                // 在字符串参数状态下又出现了'，说明是字符串参数结束了，返回函数状态
                if (this.IsParamStringIndicator(ch))
                {
                    this.DispatchParamString(this.paramter, parent);
                    this.EnterParamEnd();
                }
                else
                {
                    this.ActionCollect(ch);
                }
            }
            else
            {
                logger.WarnFormat("EventParamString未处理的字符, {0}", ch);
                //this.ActionCollect(ch);
            }
        }

        private void EventParamEnd(char ch, Expression parent)
        {
            if (this.IsParamEndIndicator(ch))
            {
                // 遇到了右括号，说明参数全部解析完了
                this.ActionParamEnd(ch, parent);
            }
            else if (this.IsParamSplitter(ch))
            {
                // 参数还没解析完，继续解析
                this.EnterFunctionState();
            }
            else
            {
                logger.WarnFormat("EventParamEnd未处理的字符, {0}", ch);
            }
        }

        #endregion

        /// <summary>
        /// 构造表达式树形列表
        /// </summary>
        /// <param name="expression"></param>
        public Expression BuildExpressionTree(string expression)
        {
            this.exp = this.root = new Expression();

            this.state = ParserState.Ground;
            this.enumerator = expression.GetEnumerator();

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

                    case ParserState.ParamEnd:
                        {
                            this.EventParamEnd(ch, this.exp);
                            break;
                        }

                    default:
                        logger.WarnFormat("未处理的状态:{0}", this.state);
                        break;
                }
            }

            return this.root;
        }
    }
}

