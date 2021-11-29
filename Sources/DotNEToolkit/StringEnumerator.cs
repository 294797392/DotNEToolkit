using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit
{
    /// <summary>
    /// 实现一个增强版的CharEnumerator
    /// 1. 该Enumerator提供了Peek的功能
    /// 2. 提供了MoveUntil函数
    /// 3. 提供了CharPosition属性标识当前字符的位置
    /// </summary>
    public class StringEnumerator
    {
        private string str;
        private int charIndex;

        /// <summary>
        /// peek的次数
        /// 每次调用MoveNext的时候peeks会清零
        /// </summary>
        private int peeks;


        /// <summary>
        /// 获取当前字符的位置
        /// </summary>
        public int CharPosition { get { return this.charIndex; } }

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
            this.Reset();
        }

        public bool MoveNext()
        {
            this.peeks = 0;

            if (this.charIndex >= this.str.Length)
            {
                return false;
            }

            this.Current = this.str[this.charIndex++];
            return true;
        }

        /// <summary>
        /// 向前Move，直到Move到until为止
        /// </summary>
        /// <param name="until">要move到的字符</param>
        /// <param name="moved">保存move到的字符串，不包含until</param>
        /// <returns>
        /// 如果Move到了until，那么返回true，否则返回false
        /// </returns>
        public bool MoveNext(char until, out string moved)
        {
            moved = string.Empty;

            while (this.MoveNext())
            {
                if (this.Current == until)
                {
                    return true;
                }
                else
                {
                    moved += this.Current;
                }
            }

            return false;
        }

        public bool Peek()
        {
            this.peeks++;

            if (this.charIndex + this.peeks >= this.str.Length)
            {
                return false;
            }

            this.Peeked = this.str[this.charIndex + this.peeks];

            return true;
        }

        //public bool Peek(char until, out string peeked)
        //{

        //}

        public void Reset()
        {
            this.charIndex = 0;
            this.peeks = 0;
            this.Current = char.MinValue;
            this.Peeked = char.MinValue;
        }
    }
}






