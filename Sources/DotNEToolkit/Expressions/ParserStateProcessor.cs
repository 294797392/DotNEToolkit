using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Expressions
{
    internal abstract class AbstractStateHandler
    {
        public abstract ParserState State { get; }

        public abstract Dictionary<char, AbstractStateHandler> Process(ParserContext context);
    }

    //internal class IDLEStateHandler : AbstractStateHandler
    //{
    //    public override ParserState State { get { return ParserState.IDLE; } }

    //    public override Dictionary<char, AbstractStateHandler> Process(ParserContext context)
    //    {
    //        return ParserStateMaps.IDLEStateMap;
    //    }
    //}

    ///// <summary>
    ///// 在IDLE状态下，遇到单引号触发
    ///// 解析一个完整的表达式
    ///// 
    ///// IN  : 'a,b,cd', parameter('a'), parameter('b,3,c,3'),  'c', 'd', '2', parameter('abcd')
    ///// OUT : 'a,b,cd'
    ///// </summary>
    //internal class StringStateHandler : AbstractStateHandler
    //{
    //    public override ParserState State { get { return ParserState.String; } }

    //    public override Dictionary<char, AbstractStateHandler> Process(ParserContext context)
    //    {
    //        string expression = string.Empty;
    //        //expression += context.CurrentChar;

    //        bool end = false;

    //        for (int index = context.CurrentIndex + 1; index < context.Expression.Length; index++)
    //        {
    //            char current = context.Expression[index];
    //            switch (current)
    //            {
    //                case '\'':
    //                    {
    //                        end = true;
    //                        break;
    //                    }

    //                default:
    //                    {
    //                        expression += current;
    //                        break;
    //                    }
    //            }

    //            if (end)
    //            {
    //                context.RaiseExpressionReceived(expression, context.CurrentIndex, index, ExpressionTypes.StringConstant);
    //                context.CurrentIndex = index + 1;
    //                break;
    //            }
    //        }

    //        return ParserStateMaps.IDLEStateMap;
    //    }
    //}

    ///// <summary>
    ///// 在IDLE状态下，遇到函数关键字触发
    ///// 解析一个完整的表达式
    ///// 
    ///// IN  : parameter('a'), parameter('b,3,c,3'),  'c', 'd', '2', parameter('abcd')
    ///// OUT : parameter('a')
    ///// </summary>
    //internal class ExpressionStateHandler : AbstractStateHandler
    //{
    //    public override ParserState State { get { return ParserState.Expression; } }

    //    public override Dictionary<char, AbstractStateHandler> Process(ParserContext context)
    //    {
    //        // 

    //        int left_bracket = 0, right_bracket = 0;
    //        bool str_start = false;
    //        bool end = false;

    //        string expression = string.Empty;
    //        expression += context.CurrentChar;

    //        for (int index = context.CurrentIndex + 1; index < context.Expression.Length; index++)
    //        {
    //            char current = context.Expression[index];
    //            expression += current;
    //            switch (current)
    //            {
    //                case '\'':
    //                    {
    //                        if (str_start)
    //                        {
    //                            str_start = false;
    //                        }
    //                        else
    //                        {
    //                            str_start = true;
    //                        }
    //                        break;
    //                    }

    //                case '(':
    //                    {
    //                        if (str_start)
    //                        {
    //                            // 字符串里的左括号
    //                        }
    //                        else
    //                        {
    //                            left_bracket++;
    //                        }
    //                        break;
    //                    }

    //                case ')':
    //                    {
    //                        if (str_start)
    //                        {
    //                            // 字符串里的右括号
    //                        }
    //                        else
    //                        {
    //                            right_bracket++;

    //                            if (right_bracket == left_bracket)
    //                            {
    //                                // 表达式解析结束
    //                                end = true;
    //                                break;
    //                            }
    //                        }
    //                        break;
    //                    }

    //                default:
    //                    {
    //                        continue;
    //                    }
    //            }

    //            if (end)
    //            {
    //                context.RaiseExpressionReceived(expression, context.CurrentIndex, index, ExpressionTypes.Function);
    //                context.CurrentIndex = index + 1;
    //                break;
    //            }
    //        }

    //        return ParserStateMaps.IDLEStateMap;
    //    }
    //}

    //internal class MacroDefinitionStateHandler : AbstractStateHandler
    //{
    //    public override ParserState State { get { return ParserState.MarcoDefinition; } }

    //    public override Dictionary<char, AbstractStateHandler> Process(ParserContext context)
    //    {
    //        string expression = string.Empty;

    //        bool end = false;

    //        for (int index = context.CurrentIndex + 1; index < context.Expression.Length; index++)
    //        {
    //            char current = context.Expression[index];
    //            switch (current)
    //            {
    //                case '}':
    //                    {
    //                        end = true;
    //                        break;
    //                    }

    //                default:
    //                    {
    //                        expression += current;
    //                        break;
    //                    }
    //            }

    //            if (end)
    //            {
    //                context.RaiseExpressionReceived(expression, context.CurrentIndex, index, ExpressionTypes.MacroDefinition);
    //                context.CurrentIndex = index + 1;
    //                break;
    //            }
    //        }

    //        return ParserStateMaps.IDLEStateMap;
    //    }
    //}
}
