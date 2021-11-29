using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit.Expressions
{
    /// <summary>
    /// 实现一个CharEnumerator
    /// 和CharEnumerator不同的是，该Enumerator提供了Peek的功能
    /// </summary>
    public class StringEnumerator
    {
        private string str;
        private CharEnumerator enumerator;
        private int currentIndex;

        /// <summary>
        /// peek的次数
        /// 每次调用MoveNext的时候peeks会清零
        /// </summary>
        private int peeks;


        /// <summary>
        /// 当前的字符
        /// </summary>
        public char Current { get; private set; }

        /// <summary>
        /// 当前Peek的字符
        /// </summary>
        public char Peeked { get; private set; }

        public StringEnumerator(string str)
        {
            this.str = str;
            this.enumerator = this.str.GetEnumerator();
            this.currentIndex = 0;
        }

        public bool MoveNext()
        {
            this.peeks = 0;

            if (this.currentIndex >= this.str.Length)
            {
                return false;
            }

            this.Current = this.str[this.currentIndex++];
            return true;
        }

        public bool Peek()
        {
            this.peeks++;

            if (this.currentIndex + this.peeks >= this.str.Length)
            {
                return false;
            }

            this.Peeked = this.str[this.currentIndex + this.peeks];

            return true;
        }

        public void Reset()
        {
            this.currentIndex = 0;
            this.peeks = 0;
            this.Current = char.MinValue;
            this.Peeked = char.MinValue;
        }
    }
}
