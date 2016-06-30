using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blqw
{
    /// <summary> 捕获单词
    /// </summary>
    public class MatchWord
    {
        internal MatchWord(string text, int startindex, int length)
        {
            Value = text.Substring(startindex, length);
            StartIndex = startindex;
            Length = length;
        }
        /// <summary> 单词
        /// </summary>
        public string Value { get; private set; }
        /// <summary> 在文本中的起始位置
        /// </summary>
        public int StartIndex { get; private set; }
        /// <summary> 单词长度
        /// </summary>
        public int Length { get; private set; }
        /// <summary>
        /// 直接返回Value
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Value;
        }
    }
}
