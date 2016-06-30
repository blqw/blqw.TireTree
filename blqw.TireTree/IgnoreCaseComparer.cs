using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blqw
{
    /// <summary>
    /// char比较方法,忽略大小写
    /// </summary>
    class IgnoreCaseComparer : IEqualityComparer<char>
    {
        public readonly static IgnoreCaseComparer Instance = new IgnoreCaseComparer();
        private IgnoreCaseComparer()
        {

        }
        public bool Equals(char x, char y)
        {
            var a = x - y;
            if (a == 0)
            {
                return true;
            }
            else if (a == ('A' - 'a'))
            {
                return x >= 'A' && x <= 'Z';
            }
            else if (a == ('a' - 'A'))
            {
                return x >= 'a' && x <= 'z';
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode(char c)
        {
            if (c >= 'a' && c <= 'z')
            {
                return c + ('A' - 'a');
            }
            return (int)c;
        }
    }

}
