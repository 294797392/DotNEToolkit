using DotNEToolkit.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkitConsole
{
    public class TestExpressionBuilder
    {
        public void TestProperty()
        {
            ExpressionParser parser = new ExpressionParser();
            //Expression expression = parser.BuildExpressionTree("(a('0').Property, b('1'.Property1,'2').Property2, c('3'.Property3).Property4, d(e('1').Property5).Property6)");
            //Expression expression = parser.BuildExpressionTree("$a('0').b");
            //Expression expression = parser.BuildExpressionTree("$a('0'.b).c");
            Expression expression = parser.BuildExpressionTree("$a('0'.b.m.n.b.v, e('1').d, f(g('1'.p).l)).c");

            Console.ReadLine();
        }

        public void TestBuild()
        {
            ExpressionParser parser = new ExpressionParser();
            Expression expression = parser.BuildExpressionTree("entry(a('0'), b('1','2'), c('3'), d(e('1')), f(g('5'), h('6')))");
            //Expression expression = parser.BuildExpressionTree("f(g('5'), h('6'))");
            //Expression expression = parser.BuildExpressionTree("d(e('5')), f(g('5'), h('6'))");

            Console.ReadLine();
        }
    }
}
