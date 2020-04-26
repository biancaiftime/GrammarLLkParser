using System;
using System.Collections.Generic;
using System.Text;

namespace Proiect2
{
    public class LLk
    {
        public List<string> Nonterminals { get; set; } = new List<string>();

        public Dictionary<string, List<string>> Productions { get; set; } = new Dictionary<string, List<string>>();

        public Dictionary<Tuple<string, string>, int> ProductionNumber { get; set; } = new Dictionary<Tuple<string, string>, int>();

        public Dictionary<string, HashSet<string>> Follow { get; set; } = new Dictionary<string, HashSet<string>>();

        public Dictionary<Tuple<string, string>, Tuple<string, int>> ParserTable { get; set; } = new Dictionary<Tuple<string, string>, Tuple<string, int>>();


        public void ReadFromFile(string filePath)
        {
            int number = 1;
            string[] lines = System.IO.File.ReadAllLines(@filePath);
            foreach(string line in lines)
            {
                string[] parts = line.Split("-");
                Nonterminals.Add(parts[0]);
                string[] fileProductions = parts[1].Split("|");
                var havePoductions = Productions.TryGetValue(parts[0], out var productions);
                if (!havePoductions) productions = new List<string>();
                foreach (string production in fileProductions)
                {
                    productions.Add(production);
                    ProductionNumber.Add(Tuple.Create(parts[0], production), number);
                    number++;
                }
                Productions.Add(parts[0], productions);
            }
        }

        public void GenerateFollowK(int k)
        {
            Follow.Clear();
            var Finished = false;
            var init = new HashSet<string>();
            init.Add("#");
            Follow.Add("S", init);
            while (Finished == false)
            {
                Finished = true;
                Nonterminals.ForEach(n =>
                {
                    var hasProductions = Productions.TryGetValue(n, out var productions);
                    if (hasProductions)
                    {
                        productions.ForEach(p =>
                        {
                            if (HasNonterminal(p))
                            {
                                foreach (char Y in p)
                                {
                                    if (!Y.ToString().Equals(Y.ToString().ToLower()))
                                    {
                                    //is nonterminal
                                    // Followk(Y) = Firstk(beta * Followk(n))

                                    //get all the characters after the nonterminal
                                        var beta = p.Substring(p.IndexOf(Y) + 1);
                                        var words = new HashSet<string>();
                                        if (beta.Length < k && !HasNonterminal(beta)) words.Add(beta);
                                        else words = GenerateWords(beta, k);
                                        var hasFollow = Follow.TryGetValue(n, out var follows);
                                        var values = new HashSet<string>();
                                        foreach (var follow in follows)
                                        {
                                            foreach (var word in words)
                                            {
                                                if((word + follow).Length >= k ) values.Add((word + follow).Substring(0, k));
                                                else values.Add(word + follow);
                                            }
                                        }

                                        if (Follow.TryGetValue(Y.ToString(), out var results))
                                        {
                                            var length = results.Count;
                                            foreach (var value in values) results.Add(value);
                                            if (length != results.Count)
                                            {
                                                Finished = false;
                                                Follow[Y.ToString()] = results;
                                            }
                                        }
                                        else
                                        {
                                            Follow.Add(Y.ToString(), values);
                                            Finished = false;
                                        }
                                    }
                                }
                            }
                        });
                    }
                });
            }
        }
        public int VerifyLLk()
        {
            for(int k = 1; k <= 10; k++ )
            {
                GenerateFollowK(k);
                ParserTable.Clear();
                var result = true;
                Nonterminals.ForEach(n =>
                {
                    result = result && Verify(n, k);
                });
                if (result == true) return k;
            }
            return -1;
        }

        public bool Verify(string state, int k)
        {
            var results = new HashSet<string>();
            var LLkTable = new List<Tuple<string, string, int>>();
            var length = 0;
            if(Productions.TryGetValue(state, out var productions))
            {
                
                productions.ForEach(p =>
                {
                    if (p == "?") p = "";
                    var words = new HashSet<string>();
                    if (p.Length < k && !HasNonterminal(p)) words.Add(p);
                    else words = GenerateWords(p, k);
                    var hasFollow = Follow.TryGetValue(state, out var follows);
                    var values = new HashSet<string>();

                    foreach (var follow in follows)
                    {
                        foreach (var word in words)
                        {
                            if ((word + follow).Length >= k) values.Add((word + follow).Substring(0, k));
                            else values.Add(word + follow);
                        }
                    }
                    foreach (var value in values)
                    {
                        results.Add(value);
                        //ParserTable.Add(Tuple.Create(state, value), Tuple.Create(p, number));
                        ProductionNumber.TryGetValue(Tuple.Create(state, (p == "" ? "?" : p)), out var number);
                        LLkTable.Add(Tuple.Create(value, p, number));
                    }

                    length += values.Count;
                });
            }
            if (length == results.Count)
            {
                LLkTable.ForEach(l =>
                {
                    ParserTable.Add(Tuple.Create(state, l.Item1), Tuple.Create((l.Item2 == "" ? "?" : l.Item2), l.Item3));
                });
                return true;
            }
            return false;

        }

        public HashSet<string> GenerateWords(string beta, int k)
        {
            var words = new HashSet<string>();
            //first k characters are terminals
            if (beta.Length >= k && beta.Substring(0, k).Equals(beta.Substring(0, k).ToLower())) { words.Add(beta.Substring(0, k)); return words; }

            var queue = new Queue<string>();
            queue.Enqueue(beta);
            var finished = false;
            while(queue.Count > 0 && !finished)
            {
                var l = queue.Dequeue();
                foreach (char c in l)
                {
                    if (!c.ToString().Equals(c.ToString().ToLower()))
                    {
                        if (Productions.TryGetValue(c.ToString(), out var productions))
                        {
                            productions.ForEach(p =>
                            {
                                if (p == "?") p = "";
                                var pp = l.Replace(c.ToString(), p);
                                if (pp.Length <= k && !HasNonterminal(pp))
                                {
                                    if (!words.Contains(pp))
                                    {
                                        words.Add(pp);
                                    }
                                    else finished = true;
                                }

                                else if (pp.Length > k && !HasNonterminal(pp.Substring(0, k)))
                                {
                                    if (!words.Contains(pp.Substring(0, k)))
                                    {
                                        words.Add(pp.Substring(0, k));
                                    }
                                    else finished = true;
                                }
                                else queue.Enqueue(pp);
                            });

                        }
                    }
                }
            }
            return words;
        }

        public HashSet<string> WordsFromGrammar(string state, int length)
        {
            var results = new HashSet<string>();
            var hasProductions = Productions.TryGetValue(state, out var productions);
            bool next = true;
            while(next)
            {
                productions.ForEach(p =>
                {
                    if (p.Equals("?")) results.Add("");
                    else if (p.Equals(p.ToLower())) results.Add(p.Substring(0, length)); 
                });
            }
            return results;
        }

        public bool HasNonterminal(string s)
        {
           return !s.Equals(s.ToLower());
        }

        public bool IsAccepted(string input, int k, out string output)
        {
            string word = input + "#";
            string state = "S#";
            output = "";
            
            while(word != "")
            {
                var index = 0;
                //erase terminal characters
                foreach (var c in state)
                {
                    if (c.ToString().Equals(c.ToString().ToLower()))
                    {
                        if (word.StartsWith(c.ToString())) { word = word.Substring(1); index++; }
                        else return false;
                    }
                    else break;
                }
                if (word == "") break;
                state = state.Substring(index);

                //search in parser table
                if (ParserTable.TryGetValue(Tuple.Create(state.Substring(0, 1), word.Substring(0, k)), out var result))
                {
                    state = (result.Item1 == "?" ? "" : result.Item1) + state.Substring(1);
                    output += result.Item2.ToString();
                }
                else return false;
            }
            return true;
        }

        public void ParseWordsFromFile(string filePath, int k)
        {
            string[] lines = System.IO.File.ReadAllLines(@filePath);
            foreach (string line in lines)
            {
                if (IsAccepted(line, k, out var output)) Console.WriteLine("Word: " + line + ", output: " + output);
                else Console.WriteLine("Word: " + line +" error!");
            }
        }
    }
}
