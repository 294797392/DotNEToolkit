using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkit
{
    /// <summary>
    /// 指定StringEnumerator使用的换行符
    /// </summary>
    public enum StringTerminator
    {
        Default,

        /// <summary>
        /// StringEnumerator会把换行符转换成CR
        /// </summary>
        CR,

        /// <summary>
        /// StringEnumerator会把换行符转换成LF
        /// </summary>
        LF
    }

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

        /// <summary>
        /// 向前Move一个字符
        /// 每次Move都会重置Peek指针
        /// </summary>
        /// <returns></returns>
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
        /// 向前move step个字符
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public bool MoveNext(int step)
        {
            this.peeks = 0;

            int next = this.charIndex + step - 1;

            if (next >= this.str.Length)
            {
                return false;
            }

            this.Current = this.str[next];

            this.charIndex = next;

            return true;
        }

        /// <summary>
        /// 向前move，遇到ignore字符直接忽略并继续move
        /// </summary>
        /// <param name="ignore">要忽略的字符</param>
        /// <returns></returns>
        public bool MoveNext(char ignore)
        {
            while (this.MoveNext())
            {
                if (this.Current == ignore)
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 向前Move，直到Move到until为止
        /// 每次Move都会重置Peek指针
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

        /// <summary>
        /// 向前peek，会移动Peek指针
        /// </summary>
        /// <returns></returns>
        public bool Peek()
        {
            if (this.charIndex + this.peeks >= this.str.Length)
            {
                return false;
            }

            this.Peeked = this.str[this.charIndex + this.peeks];

            this.peeks++;

            return true;
        }

        public bool Peek(out char ch)
        {
            ch = char.MinValue;

            if (!this.Peek())
            {
                return false;
            }

            ch = this.Peeked;

            return true;
        }

        /// <summary>
        /// 重置指针状态
        /// </summary>
        public void Reset()
        {
            this.charIndex = 0;
            this.peeks = 0;
            this.Current = char.MinValue;
            this.Peeked = char.MinValue;
        }
    }
}






