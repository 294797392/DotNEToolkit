using DotNEToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkitConsole
{
    public class TestStringEnumerator
    {
        public void Peek(string text)
        {
            StringEnumerator enumerator = new StringEnumerator(text);

            enumerator.MoveNext();

            while (enumerator.Peek())
            {
                Console.WriteLine(enumerator.Peeked);
            }
        }


    }
}
