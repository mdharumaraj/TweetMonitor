using System;
using System.Collections.Generic;

namespace TweetMonitorLib
{
    public class AhoCorasickMatcher
    {
        private Trie trie;

        public AhoCorasickMatcher(string[] keywords)
        {
            BuildTrie(keywords);
            AddSuffixLinks();
            AddOutputLinks();
        }

        public void RunMatch(string body, out string bannedWordsMatched)
        {
            bannedWordsMatched = string.Empty;

            int l = 0;
            int c = 0;

            TrieVertex vertex = trie.Root;
            char buffer = char.MinValue;
            int bodyLength = body.Length;
            do
            {
                if (c >= bodyLength)
                {
                    break;
                }

                buffer = body[c];
                if (vertex.Edges.ContainsKey(buffer))
                {
                    vertex = vertex.Edges[buffer];
                    if (vertex.PatternIndex != -1)
                    {
                        bannedWordsMatched += " " + vertex.Label + " ";
                    }

                    if (vertex.OutputLinks.Count > 0)
                    {
                        //string outputLinkStrings = string.Empty;
                        foreach (TrieVertex v in vertex.OutputLinks)
                        {
                            bannedWordsMatched += " " + v.Label + " ";
                            //outputLinkStrings += v.Label + " ";
                        }
                        //Console.WriteLine("OL; Position:{1} Found match:{0}", outputLinkStrings, l);
                    }
                }
                else
                {
                    vertex = vertex.SuffixLink;
                    l = c - vertex.Label.Length;
                }

                c++;
            }
            while (true);
        }

        private void BuildTrie(string[] keywords)
        {
            trie = new Trie();
            for (int i = 0; i < keywords.Length; i++)
            {
                string word = keywords[i];
                TrieVertex current = trie.Root;
                for (int j = 0; j < word.Length; j++)
                {
                    char c = word[j];

                    if (!current.Edges.ContainsKey(c))
                    {
                        current.Edges[c] = new TrieVertex(current, current.Label + c);
                    }

                    current = current.Edges[c];
                }

                current.PatternIndex = i;
            }
        }

        private void AddSuffixLinks()
        {
            Queue<Tuple<TrieVertex, char>> queue = new Queue<Tuple<TrieVertex, char>>();
            queue.Enqueue(new Tuple<TrieVertex, char>(trie.Root, char.MinValue));

            while (queue.Count > 0)
            {
                Tuple<TrieVertex, char> t = queue.Dequeue();

                TrieVertex vertex = t.Item1;
                char edgeChar = t.Item2;
                SetSuffixLink(vertex, edgeChar);

                foreach (char c in vertex.Edges.Keys)
                {
                    queue.Enqueue(new Tuple<TrieVertex, char>(vertex.Edges[c], c));
                }
            }
        }

        private void SetSuffixLink(TrieVertex vertex, char edgeChar)
        {
            if (vertex == trie.Root)
            {
                vertex.SuffixLink = trie.Root;
                return;
            }

            if (vertex.Parent == trie.Root)
            {
                vertex.SuffixLink = trie.Root;
                return;
            }

            TrieVertex parent = vertex.Parent;
            TrieVertex suffixLinkForParent = parent.SuffixLink;

            TrieVertex aVertex = suffixLinkForParent;
            while (!aVertex.Edges.ContainsKey(edgeChar) && aVertex != trie.Root)
            {
                aVertex = aVertex.SuffixLink;
            }

            if (aVertex.Edges.ContainsKey(edgeChar))
            {
                vertex.SuffixLink = aVertex.Edges[edgeChar];
            }
            else
            {
                vertex.SuffixLink = trie.Root;
            }
        }

        private void AddOutputLinks()
        {
            Queue<TrieVertex> queue = new Queue<TrieVertex>();
            queue.Enqueue(trie.Root);

            while (queue.Count > 0)
            {
                TrieVertex vertex = queue.Dequeue();

                TrieVertex suffixLinkVertex = vertex.SuffixLink;
                while (suffixLinkVertex != trie.Root)
                {
                    if (suffixLinkVertex.PatternIndex != -1)
                    {
                        vertex.OutputLinks.Add(suffixLinkVertex);
                    }

                    suffixLinkVertex = suffixLinkVertex.SuffixLink;
                }

                foreach (char c in vertex.Edges.Keys)
                {
                    queue.Enqueue(vertex.Edges[c]);
                }
            }
        }
    }

    class Trie
    {
        public TrieVertex Root { get; private set; }

        public Trie()
        {
            Root = new TrieVertex(null, string.Empty);
        }
    }

    class TrieVertex
    {
        public int PatternIndex { get; set; }
        public IDictionary<char, TrieVertex> Edges { get; private set; }
        public TrieVertex SuffixLink { get; set; }
        public TrieVertex Parent { get; private set; }
        public string Label { get; private set; }
        public IList<TrieVertex> OutputLinks { get; internal set; }

        private TrieVertex(TrieVertex parent)
        {
            PatternIndex = -1;
            Edges = new Dictionary<char, TrieVertex>();
            SuffixLink = null;
            Parent = parent;
            this.OutputLinks = new List<TrieVertex>();
        }

        public TrieVertex(TrieVertex parent, string label)
            : this(parent)
        {
            this.Label = label;
        }

        public override string ToString()
        {
            string outputLinkStrings = string.Empty;
            foreach (TrieVertex v in this.OutputLinks)
            {
                outputLinkStrings += v.Label + " ";
            }

            return string.Format("Label:{0} SuffixLink:{1} PatternIndex:{2} OutputLinks:{3}", Label, SuffixLink?.Label, PatternIndex, outputLinkStrings);
        }
    }
}
