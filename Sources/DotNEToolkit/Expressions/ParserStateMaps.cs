using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Expressions
{
    internal static class ParserStateMaps
    {
        private static IDLEStateHandler ISH = new IDLEStateHandler();
        private static StringStateHandler SSH = new StringStateHandler();
        private static ExpressionStateHandler ESH = new ExpressionStateHandler();
        private static MacroDefinitionStateHandler MDSH = new MacroDefinitionStateHandler();

        public static Dictionary<char, AbstractStateHandler> IDLEStateMap = new Dictionary<char, AbstractStateHandler>()
        {
            { 's', ESH }, { 'p', ESH }, { 'm', ESH }, { 'v', ESH }, { 'i', ESH }, { 'r', ESH }, { 'u', ESH },
            { '\'', SSH }, { ' ',  ISH }, { ',', ISH },

            { '{', MDSH }
        };
    }
}