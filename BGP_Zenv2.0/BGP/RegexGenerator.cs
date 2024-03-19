using ZenLib;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Linq;
using static ZenLib.Zen;

namespace BGP{
    public class RegexGenerator{
        public Regex<char> reg;
        public Dictionary<long, HashSet<long>> G;
        public Dictionary<Tuple<long, long>, List<CharRange<char>>> Edges;
        public Dictionary<long, Dictionary<long, long>> ParentArrays;
        public HashSet<List<long>> Paths;
        public long Source;
        public List<long> Destinations; 

        private Dictionary<long, bool> vis;
        private Dictionary<long, long> parent;

        private void DFS(long u, long p){
            parent[u] = p;
            vis[u] = true;

            foreach(var v in G[u]){
                if(!vis[v])DFS(v, u);
            }    
        }

        private void reset(){
            vis = new Dictionary<long, bool>();
            parent = new Dictionary<long, long>();

            foreach(var u in G){
                vis.Add(u.Key, false);
                parent.Add(u.Key, 0);
            }
        }

        public void CreateGraph(Regex<char> r){
            reg = r;
            var a = r.ToAutomaton();

            G = new Dictionary<long, HashSet<long>>();
            ParentArrays = new Dictionary<long, Dictionary<long, long>>();
            Paths = new HashSet<List<long>>();
            Edges = new Dictionary<Tuple<long, long>, List<CharRange<char>>>();

            Source = a.InitialState.Id;
            Destinations = new List<long>();

            foreach(var node in a.FinalStates){
                Destinations.Add(node.Id);
            }

            foreach (var kv1 in a.Transitions){ // a.Transitions = {state_id : {char_range : state_id, ...}, ...}
                HashSet<long> neighbors = new HashSet<long>();
                foreach (var kv2 in kv1.Value){
                    neighbors.Add(kv2.Value.Id);
                    var t = Tuple.Create(kv1.Key.Id, kv2.Value.Id);
                    if(Edges.ContainsKey(t)){
                        Edges[t].Add(kv2.Key);
                    }
                    else{
                        List<CharRange<char>> temp = new List<CharRange<char>>();
                        temp.Add(kv2.Key);
                        Edges.Add(t, temp); 
                    }
                }
                G.Add(kv1.Key.Id, neighbors);
            }
            
            foreach(var u in G){
                reset();
                DFS(u.Key, 0);
                ParentArrays.Add(u.Key, parent);
            }
        }

        public void CreateComplementGraph(){
            Destinations = new List<long>();
            ParentArrays = new Dictionary<long, Dictionary<long, long>>();
            Paths = new HashSet<List<long>>();
            var a = reg.ToAutomaton();
            foreach(var node in a.States){
                if(!a.FinalStates.Contains(node)){
                    Destinations.Add(node.Id);
                }
            }
            foreach(var u in G){
                reset();
                DFS(u.Key, 0);
                ParentArrays.Add(u.Key, parent);
            }
        }

        public void NodeCover(){
            foreach(var node in G){
                //select the intermediate node
                // Console.WriteLine($"{node.Key}:");

                //Find a path from source node to this node
                List<long> p1 = new List<long>();
                long u = node.Key;
                while(ParentArrays[Source][u] != 0){
                    p1.Add(u);
                    u = ParentArrays[Source][u];
                }

                p1.Add(u);

                p1.Reverse();

                //Find paths from this node to all the final states
                foreach(var dst in Destinations){
                    List<long> p2 = new List<long>();
                    long v = dst;
                    while(ParentArrays[node.Key][v] != 0){
                        p2.Add(v);
                        v = ParentArrays[node.Key][v];
                    }

                    p2.Reverse();

                    // concatenate the two paths
                    List<long> p = new List<long>();
                    p.AddRange(p1);
                    p.AddRange(p2);

                    if(p.Last() != dst){
                        // Console.WriteLine($"No valid path found for accepting state {dst}");
                        continue;
                    }

                    Paths.Add(p);

                    // Console.WriteLine(string.Join(", ", p));
                }
                // Console.WriteLine();
            }
        }

        public void EdgeCover(){
            foreach(var e in Edges){
                long u = e.Key.Item1;
                long v = e.Key.Item2;

                // Console.WriteLine($"({u}, {v}):");

                List<long> p1 = new List<long>();
                while(ParentArrays[Source][u] != 0){
                    p1.Add(u);
                    u = ParentArrays[Source][u];
                }

                p1.Add(u);
                p1.Reverse();

                foreach(var dst in Destinations){
                    long d = dst;
                    List<long> p2 = new List<long>();
                    while(ParentArrays[v][d] != 0){
                        p2.Add(d);
                        d = ParentArrays[v][d];
                    }

                    p2.Add(d);
                    p2.Reverse();

                    if(p2[0] != v){
                        // Console.WriteLine($"No valid path found for accepting state {dst}");
                        continue;
                    }
                    List<long> p = new List<long>();
                    p.AddRange(p1);
                    p.AddRange(p2);

                    Paths.Add(p);

                    // Console.WriteLine(string.Join(", ", p));
                }
                // Console.WriteLine();
            }
        }

        public List<string> GenerateExamples(){
            HashSet<string> generated_examples = new HashSet<string>();
            // var valid_char_ranges = new List<CharRange<char>> {new CharRange<char>('$','$'), new CharRange<char>('*','+'), new CharRange<char>('0','9'), new CharRange<char>(':',':'), new CharRange<char>('?','?'), new CharRange<char>('^','^')};

            foreach(var path in Paths){
                string s1 = "";
                Random rnd = new Random();
                int n = path.Count();
                for(int i=0;i<n-1;i++){
                    var t = Tuple.Create(path[i], path[i+1]);
                    var possibleTransitions = Edges[t];
                    // List<CharRange<char>> possibleTransitions =  new List<CharRange<char>> ();
                    // foreach(var pt in _possibleTransitions){
                    //    foreach(var vcr in valid_char_ranges){
                    //        var isec = vcr.Intersect(pt);
                    //        if (!isec.IsEmpty()){
                    //            if(!possibleTransitions.Contains(isec)){
                    //                possibleTransitions.Add(isec);
                    //            }
                    //        }
                    //    }
                    // }
                    // if (possibleTransitions.Count == 0) continue;
                    int pd = rnd.Next(possibleTransitions.Count);
                    char c = (char)rnd.Next(possibleTransitions[pd].Low, possibleTransitions[pd].High+1);
                    s1 += c;
                }

                generated_examples.Add(s1);
            }

            List<string> final_examples = new List<string>(generated_examples);

            return final_examples;
        }

        public List<string> GenerateSinglePosEx(string com){

            string regex = "(0|[1-9][0-9]{0,3}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])"; // all numbers from 0-65535 accecpted
            regex = "(" + regex + ":" + regex + ")"; // now it should accept single communities of the form AA:NN
            Regex<char> r0 = Regex.Parse("^" + regex + "$"); // valid community regex of the form "AA:NN" (single)
            Regex<char> r2 = Regex.Parse("^"+com+"$"); // given regex
            var r = Regex.Intersect(r0, r2);
            var a = r.ToAutomaton();
            CreateGraph(r);
            NodeCover();
            EdgeCover();
            var single_pos = GenerateExamples();
            return single_pos;
        }

        public Tuple<List<string>,List<string>,string> GeneratePosNegExamples(){

            // string regex = "(0|[1-9][0-9]{0,3}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])"; // all numbers from 0-65535 accecpted
            string regex = "([1-9][0-9]{0,3}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])"; // all numbers from 0-65535 accecpted
            regex = "(" + regex + ":" + regex + ")"; // now it should accept single communities of the form AA:NN
            Regex<char> r0 = Regex.Parse("^" + regex + "$"); // valid community regex of the form "AA:NN" (single)
            regex = "^(" + regex + " )+" + regex + "$"; // now you can accept communities of the form "AA:NN AA:NN AA:NN AA:NN..."
            Regex<char> r1 = Regex.Parse(regex); // valid regex multi

            
            string com = nums2();
            Console.WriteLine("com regex : "+com);
            Regex<char> r2 = Regex.Parse(com); // randomly generated regex

            // multi pos community
            var r = Regex.Intersect(r1, r2);
            var a = r.ToAutomaton();
            CreateGraph(r);
            NodeCover();
            EdgeCover();
            var multi_pos = GenerateExamples();
            Console.WriteLine("Number of multi-com +ve examples: " + string.Join("\n",multi_pos.Count)); // multiple community AA:NN AA:NN

            // neg community
            var parts = com.Split(' ');

            Regex<char> r21 = Regex.Parse(parts[0]);
            Regex<char> r22 = Regex.Parse(parts[1]); 
            r21 = Regex.Negation(r21);
            r22 = Regex.Negation(r22);
            var r_1 = Regex.Intersect(r0, r21);
            var r_2 = Regex.Intersect(r0, r22);
            CreateGraph(r_1);
            NodeCover();
            EdgeCover();
            var all_neg_1 = GenerateExamples();
            CreateGraph(r_2);
            NodeCover();
            EdgeCover();
            var all_neg_2 = GenerateExamples();
            var all_neg = Utils.ComputeCrossProduct(all_neg_1, all_neg_2);
            Utils.ShuffleList(all_neg);
            Console.WriteLine("Number of all -ve examples: " + string.Join("\n",all_neg.Count)); 

            return new Tuple<List<string>,List<string>,string>(multi_pos, all_neg, com);
        }

        public List<string> GenerateASPosEx(string asp){

            string regex = "(0|[1-9][0-9]{0,3}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])"; // all numbers from 0-65535 accecpted
            Regex<char> r0 = Regex.Parse("^" + regex + "$"); // valid AS regex of the form "AA" (single)
            regex = "^(" + regex + " )*" + regex + "$"; // now you can accept AS of the form "AA" or "AA AA AA AA..."
            Regex<char> r1 = Regex.Parse(regex); // valid regex multi
            Regex<char> r2 = Regex.Parse(asp); // given regex
            var r = Regex.Intersect(r1, r2);
            var a = r.ToAutomaton();
            CreateGraph(r);
            NodeCover();
            EdgeCover();
            var posex = GenerateExamples();
            return posex;
        }

        public List<string> GenerateASNegEx(string asp){

            string regex = "(0|[1-9][0-9]{0,3}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])"; // all numbers from 0-65535 accecpted
            Regex<char> r0 = Regex.Parse("^" + regex + "$"); // valid AS regex of the form "AA" (single)
            regex = "^(" + regex + " )*" + regex + "$"; // now you can accept AS of the form "AA" or "AA AA AA AA..."
            Regex<char> r1 = Regex.Parse(regex); // valid regex multi
            Regex<char> r2 = Regex.Parse(asp); // given regex
            r2 = Regex.Negation(r2);
            var r = Regex.Intersect(r1, r2);
            var a = r.ToAutomaton();
            CreateGraph(r);
            NodeCover();
            EdgeCover();
            var negex = GenerateExamples();
            return negex;
        }

        // Hyperparams for nums() functions for generating random regex
        int nd = 3;
        int repeat = 2;
        public string nums (){
            Random rng = new Random();
            string rgs = "";
            var mylist = Enumerable.Range(0, nd).OrderBy(t => rng.Next()).ToList();
            if (rng.Next()%2 == 0){
                rgs = "[";
                for(int i=0;i<rng.Next(1,nd+1);i++) {rgs += mylist[i].ToString();}
                rgs += "]";
            }
            else{
                var s = rng.Next(0,nd-1);
                var e = rng.Next(s+1,nd);
                rgs = "[" + s.ToString() + '-' + e.ToString() + ']';
            }
            return rgs;
        }

        public string nums1(){
            Random rng = new Random();
            string n = nums();
            Console.WriteLine("nums() --> "+n);
            var rpt = rng.Next(1,repeat+1);
            string n1 = "";
            if(n=="[0]") {n1 = n1 + "[" + rng.Next(1,nd).ToString() + "]"; rpt--;}
            for (int i=0; i<rpt; i++) {n1 += n;}
            var x = rng.Next()%4; // * + ? None
            if (x == 0){
                n1 += '*';
            }
            else {
                if (x == 1){
                    n1 += '+';
                }
                else{
                    if (x == 2){
                        n1 += '?';
                    }
                }
            }
            return n1; 
        }

        public string nums2(){
            string inits = "^";
            string ends = "$";
            Random rng = new Random();
            var rpt = rng.Next(1,repeat);
            string s;
            string n2;
            if(rng.Next()%2==0){
                s = nums1() + ":" + nums1();
                for (int i=0; i<rpt; i++) {s = s+" "+nums1()+":"+nums1();}
                n2 = inits + s + ends;
            }
            else{
                s = rng.Next(0,65535).ToString() + ":" + rng.Next(0,65535).ToString();
                for (int i=0; i<rpt; i++) {s = s+" "+rng.Next(0,65535).ToString() + ":" + rng.Next(0,65535).ToString();}
                n2 = inits + s + ends;
            }
        
            return n2;
        }

        public string disjoint_nums1(List<string> nr){
            var s = new List<string> {"*","+","?"};
            Random rng = new Random();
            var n = nr[rng.Next(0,nr.Count())];
            var rx = n + n + s[rng.Next(0,s.Count())];
            return rx;
        }

        public string disjoint_nums2(){
            var nr = new List<string> {"[3-4]","[3]","[4]"};
            var rx = disjoint_nums1(nr)+":"+disjoint_nums1(nr);
            return rx;
        }

        public string regex_bgp2zen(string r){ // mainly translate space. A B => A|B|A B
            
            if ((r[0] != '^') && (r[r.Length-1] != '$')){
                r = "^" + r + "$";
            }
            
            string[] words = r.Split(' ');

            string r1 = string.Join("$|^", words);

            r1 = r1 + "|" + r;

            return r1;
        }


        public string nums3(){ // Add | (not required to do this bcz space takes care of OR)
            Random rng = new Random();
            var rpt = rng.Next(0,repeat);
            string n3 = nums2();
            for (int i=0;i<rpt; i++){
                n3 = n3 + "|" + nums2();
            }
            return n3;
        }

        public string nums_non_zero(){
            Random rng = new Random();
            string rgs = "";
            var mylist = Enumerable.Range(1, nd).OrderBy(t => rng.Next()).ToList();
            if (rng.Next()%2 == 0){
                rgs = "[";
                for(int i=0;i<rng.Next(1,nd);i++) {rgs += mylist[i].ToString();}
                rgs += "]";
            }
            else{
                var s = rng.Next(1,nd-1);
                var e = rng.Next(s+1,nd);
                rgs = "[" + s.ToString() + '-' + e.ToString() + ']';
            }
            return rgs;
        }

        public string nums1_asp(){
            Random rng = new Random();
            string n = nums();
            Console.WriteLine("nums() --> "+n);
            var rpt = rng.Next(1,repeat+1);
            string n1 = "";
            n1 = n1 + nums_non_zero();
            rpt--;
            for (int i=0; i<rpt; i++) {n1 += n;}
            var symb = new List<string> {"*","+","?",""};
            n1 = n1 + symb[rng.Next(0,symb.Count())];
            return n1; 
        }

        public string nums2_asp(){
            Random rng = new Random();
            string inits = "^";
            string ends = "$";
            string n2;
            var rpt = rng.Next(0,repeat);
            string s;
            if(rng.Next()%2==0){
                s = nums1_asp();
                for (int i=0; i<rpt; i++) {s = s+" "+nums1_asp();}
            }
            else{
                s = rng.Next(1,65535).ToString();
                for (int i=0; i<rpt; i++) {s = s+" "+rng.Next(1,65535).ToString();}
            }
            n2 = inits + s + ends;
            return n2;
        }


        public List<string> comb (int k, int m){ // k = length, m = max number
            if(k==1){
                return Enumerable.Range(0,m+1).ToList().ConvertAll<string>(x => x.ToString());
            }
            List<string> C1 = comb(k-1,m);
            List<string> C = new List<string>();
            for (int j=0; j<C1.Count; j++){
                for(int v=0; v<m+1; v++){
                    //C.Add(C1[j]*10+v);
                    C.Add(C1[j]+v.ToString());
                } 
            }
            C = C1.Concat(C).ToList();
            return C;
        }

        public List<string> comb1(int k, int m){
            List<string> a = comb(k,m);
            List<string> b = comb(k,m);
            List<string> c = new List<string>();
            foreach (string sa in a){
                foreach (string sb in b){
                    c.Add(sa+":"+sb);
                }
            }
            return c;
        }

        public List<string> comb2 (int k, int m, int n){ // n = max number of communities
            if (n==1){
                return comb1(k,m);
            }
            List<string> C1 = comb2(k,m,n-1);
            List<string> C = new List<string>();

            for (int j=0; j<C1.Count; j++){
                foreach (string s in comb1(k,m)){
                    C.Add(s+"-"+C1[j]);
                }
            }
            C = C1.Concat(C).ToList();
            return C;
        }
    }
}