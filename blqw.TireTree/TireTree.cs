using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace blqw
{
    /// <summary> 关键词字典
    /// </summary>
    public class TireTree
    {
        /// <summary> 更新关键词库的时间,调用Load方法成功后更新该字段
        /// </summary>
        public DateTime UpdatedTime { get; private set; }
        /// <summary> 关键词库
        /// </summary>
        public ReadOnlyCollection<string> Words { get; set; }
        /// <summary> 字符节点
        /// </summary>
        private TireTreeNode Nodes { get; set; }

        /// <summary> 载入禁用词
        /// </summary>
        public void Load(IList<string> words)
        {
            Words = new ReadOnlyCollection<string>(words);
            Nodes = new TireTreeNode(Words, true);
            UpdatedTime = DateTime.Now;
        }

        /// <summary> 文本中是否存在禁用词
        /// </summary> 
        /// <param name="text"></param>
        /// <returns></returns>
        public bool Exists(string text)
        {
            return Nodes.IsMatch(text);
        }

        /// <summary> 替换文本中存在的禁用词
        /// </summary> 
        /// <param name="text"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public string Replace(string text, string flag)
        {
            return Nodes.Replace(text, flag);
        }

        /// <summary> 匹配文本中的所有关键词,并返回关键词的位置
        /// </summary>
        /// <param name="text">需要匹配的文本</param>
        /// <returns></returns>
        public IEnumerable<MatchWord> Match(string text)
        {
            return Nodes.Match(text);
        }


        /// <summary> 字符地图
        /// </summary>
        class TireTreeNode : Dictionary<char, TireTreeNode>
        {
            public bool IgnoreCase { get; private set; }
            /// <summary> 私有构造函数
            /// </summary>
            /// <param name="depth">深度</param>
            /// <param name="chr">标识字符</param>
            /// <param name="ignoreCase">是否忽略大小写</param>
            private TireTreeNode(int depth, char chr, bool ignoreCase)
                : base(ignoreCase ? IgnoreCaseComparer.Instance : null)
            {
                Depth = depth;
                Char = chr;
                IgnoreCase = ignoreCase;
            }
            /// <summary> 构造函数
            /// </summary>
            /// <param name="words">单词库</param>
            /// <param name="ignoreCase">是否忽略大小写</param>
            public TireTreeNode(IEnumerable<string> words, bool ignoreCase)
                : base(ignoreCase ? IgnoreCaseComparer.Instance : null)
            {
                IgnoreCase = ignoreCase;
                Depth = 0;
                foreach (var word in words)
                {
                    this.Add(word);
                }
            }

            /// <summary> 深度
            /// </summary>
            public int Depth { get; private set; }
            /// <summary> 标识字符
            /// </summary>
            public Char Char { get; private set; }
            /// <summary> 是否是完整单词
            /// </summary>
            public bool IsWords { get; private set; }
            /// <summary> 父路径
            /// </summary>
            public TireTreeNode Parent { get; private set; }

            /// <summary> 追加单词
            /// </summary>
            /// <param name="word"></param>
            public void Add(string word)
            {
                if (word.Length == Depth)
                {
                    IsWords = true;
                }
                else
                {
                    var c = word[Depth];
                    TireTreeNode map;
                    if (this.TryGetValue(c, out map) == false)
                    {
                        map = new TireTreeNode(Depth + 1, c, IgnoreCase);
                        map.Parent = this;
                        this.Add(c, map);
                    }
                    map.Add(word);
                }
            }

            /// <summary> 判断文本中是否存在单词库中的单词
            /// </summary>
            public bool IsMatch(string text)
            {
                return IsMatch(text, 0, text.Length);
            }
            /// <summary> 判断文本中是否存在单词库中的单词
            /// </summary>
            public bool IsMatch(string text, int start, int length)
            {
                if (length <= 0)
                {
                    throw new ArgumentException("不能小于0", "length");
                }
                var chars = text.ToCharArray();
                var end = start + length;
                for (int i = start; i < end;)
                {
                    var chr = chars[i];
                    i++;
                    TireTreeNode node;

                    if (TryGetValue(chr, out node))
                    {
                        if (node.IsWords)
                        {
                            return true;
                        }
                        for (int j = i; j < end; j++)
                        {
                            var chr2 = chars[j];
                            if (node.TryGetValue(chr2, out node))
                            {
                                if (node.IsWords)
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                return false;
            }

            /// <summary> 在文本中替换单词库中出现的单词
            /// </summary>
            public string Replace(string text, string flag)
            {
                return Replace(text, flag, 0, text.Length);
            }
            /// <summary> 在文本中替换单词库中出现的单词
            /// </summary>
            public string Replace(string text, string flag, int start, int length)
            {
                if (length <= 0)
                {
                    throw new ArgumentException("不能小于0", "length");
                }

                using (MemoryStream ms = new MemoryStream())
                using (StreamWriter sw = new StreamWriter(ms, Encoding.Unicode))
                {
                    var end = start + length;
                    for (int i = start; i < end;)
                    {
                        var firstChar = text[i++];
                        if (this.ContainsKey(firstChar))
                        {
                            TireTreeNode parent = this[firstChar];
                            int j = i;
                            bool recall = false;
                            for (; j < end; j++)
                            {
                                var c = text[j];
                                if (parent.ContainsKey(c))
                                {
                                    parent = parent[c];
                                    if (recall == false)
                                    {
                                        recall = parent.IsWords;
                                    }
                                }
                                else
                                {
                                    if (recall)
                                    {
                                        while (parent.IsWords == false && parent.Parent != null)
                                        {
                                            parent = parent.Parent;
                                            j--;
                                        }
                                    }
                                    break;
                                }
                            }
                            if (parent.IsWords)
                            {
                                sw.Write(flag);
                                i = j;
                                continue;
                            }
                        }
                        sw.Write(firstChar);
                    }

                    sw.Flush();
                    var bytes = ms.ToArray();
                    return Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);
                }
            }

            /// <summary> 在文本中捕获单词库中出现的单词并返回单词本身及所在位置
            /// </summary>
            public IEnumerable<MatchWord> Match(string text)
            {
                return Match(text, 0, text.Length);
            }
            /// <summary> 在文本中捕获单词库中出现的单词并返回单词本身及所在位置
            /// </summary>
            public IEnumerable<MatchWord> Match(string text, int start, int length)
            {
                if (length <= 0)
                {
                    throw new ArgumentException("不能小于0", "length");
                }
                return _Match(text, start, length);
            }

            /// <summary> 在文本中捕获单词库中出现的单词并返回单词本身及所在位置的枚举
            /// </summary>
            private IEnumerable<MatchWord> _Match(string text, int start, int length)
            {
                var end = start + length;
                for (int i = start; i < end;)
                {
                    var firstChar = text[i++];
                    if (this.ContainsKey(firstChar))
                    {
                        TireTreeNode parent = this[firstChar];
                        int j = i;
                        for (; j < end; j++)
                        {
                            var c = text[j];
                            if (parent.ContainsKey(c))
                            {
                                parent = parent[c];
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (parent.IsWords)
                        {
                            yield return new MatchWord(text, j - parent.Depth, parent.Depth);
                            i = j + 1;
                        }
                    }
                }
            }
        }

    }


}