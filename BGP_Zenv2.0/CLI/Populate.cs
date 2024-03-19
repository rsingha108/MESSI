using System;
using System.Collections.Generic;
using System.Numerics;
using ZenLib;
using System.Linq;
using System.IO;
using static ZenLib.Zen;
using BGP;

namespace CLI{

    public static class Populate{

        public static void save_regex_pos_neg(
                            string filename, 
                            string[] regexs, 
                            List<Array<FSeq<uint>, _3>> pos, 
                            List<Array<FSeq<uint>, _3>> neg, 
                            string[] delcom_regex, 
                            List<Array<uint, _3>> delcom, 
                            string[] regexs_as, 
                            List<Array<FSeq<uint>, _3>> pos_as, 
                            List<Array<FSeq<uint>, _3>> neg_as,
                            List<Array<uint, _3>> asp_prep,
                            List<Array<uint, _3>> asp_excl){
            FileStream fs = new FileStream(filename, FileMode.Create);
            TextWriter tmp = Console.Out;
            StreamWriter sw = new StreamWriter(fs);
            Console.SetOut(sw);

            Console.WriteLine("regexs: \n"+string.Join("\n",regexs));
            Console.WriteLine("\npos-str : \n"+string.Join("\n",pos.Select(x => //  x is Array<FSeq<uint>, _3>
                        (new Array<FSeq<string>, _3>(x.ToArray().ToList().Select(y => // y is FSeq<uint>
                            (new FSeq<string>(y.ToList().Select(z => // z is uint
                                Utils.CommunityInt2Str(z)).ToArray()))))))));
            Console.WriteLine("\npos: \n"+string.Join("\n",pos));

            Console.WriteLine("\nneg-str : \n"+string.Join("\n",neg.Select(x => //  x is Array<FSeq<uint>, _3>
                        (new Array<FSeq<string>, _3>(x.ToArray().ToList().Select(y => // y is FSeq<uint>
                            (new FSeq<string>(y.ToList().Select(z => // z is uint
                                Utils.CommunityInt2Str(z)).ToArray()))))))));

            Console.WriteLine("\nneg: \n"+string.Join("\n",neg));

            Console.WriteLine("\ndelcom_regex: \n"+string.Join("\n",delcom_regex));

            Console.WriteLine("\ndelcom-str: \n"+string.Join("\n",delcom.Select(x => //  x is Array<uint, _3>
                        (new Array<string, _3>(x.ToArray().ToList().Select(y => // y is uint
                            Utils.CommunityInt2Str(y)).ToArray())))));

            Console.WriteLine("\ndelcom: \n"+string.Join("\n",delcom));

            Console.WriteLine("\n"+(new string('*',50))+"\n");

            Console.WriteLine("\nregexs_as: \n"+string.Join("\n",regexs_as));

            Console.WriteLine("\npos_as: \n"+string.Join("\n",pos_as));

            Console.WriteLine("\nneg_as: \n"+string.Join("\n",neg_as));

            Console.WriteLine("\nasp_prepend: \n"+string.Join("\n",asp_prep));

            Console.WriteLine("\nasp_exclude: \n"+string.Join("\n",asp_excl));

            Console.SetOut(tmp);
            sw.Close();


        }

        public static (string[], List<Array<FSeq<uint>, _3>>, List<Array<FSeq<uint>, _3>>, string[], List<Array<uint, _3>>) populate_com(int size, string[] regexs, List<Array<FSeq<uint>, _3>> pos, List<Array<FSeq<uint>, _3>> neg, string[] delcom_regex, List<Array<uint, _3>> delcom){
            for(int i = 0; i < size; i++){
                // populate regexs, pos, neg
                var pos_neg_com = ComRegexPosNeg();
                pos.Add(pos_neg_com.Item1);
                neg.Add(pos_neg_com.Item2);
                regexs[i] = pos_neg_com.Item3;

                var com = pos_neg_com.Item3;
                var pos_arr = pos_neg_com.Item1.ToArray(); // type = FSeq<uint>[]

                var delcom_pop = Delcom(i, com, pos_arr);
                delcom_regex[i] = delcom_pop.Item1;
                delcom.Add(delcom_pop.Item2);
            }
            return (regexs, pos, neg, delcom_regex, delcom);
        }

        // same for AS
        public static (string[], List<Array<FSeq<uint>, _3>>, List<Array<FSeq<uint>, _3>>, List<Array<uint, _3>>, List<Array<uint, _3>>) populate_as(int size, string[] regexs_as, List<Array<FSeq<uint>, _3>> pos_as, List<Array<FSeq<uint>, _3>> neg_as, List<Array<uint, _3>> asp_prep, List<Array<uint, _3>> asp_excl){
            for(int i = 0; i < size; i++){
                var as_pos_neg = ASRegexPosNeg();
                regexs_as[i] = as_pos_neg.Item1;
                pos_as.Add(as_pos_neg.Item2);
                neg_as.Add(as_pos_neg.Item3);
                // put 3 random AS in asp_prep
                asp_prep.Add(new Array<uint, _3>(Utils.GenerateRandomUniqueNumbers(3, 1, 50000).ToList()));
                
                var nested_poslist = as_pos_neg.Item2.ToArray().ToList().Select(x => x.ToList().ToList()).ToList();
                var poslist = Utils.FlattenList(nested_poslist);
                Utils.PrintList(poslist);

                var nested_neglist = as_pos_neg.Item3.ToArray().ToList().Select(x => x.ToList().ToList()).ToList();
                var neglist = Utils.FlattenList(nested_neglist);
                Utils.PrintList(neglist);

                var all_as_list = poslist.Union(neglist).ToList();
                Utils.ShuffleList(all_as_list);

                var excl_list = all_as_list.Take(2).ToList();
                var outside = Utils.FindSmallestMissingNumber(all_as_list);
                excl_list.Add(outside);
                asp_excl.Add(new Array<uint, _3>(excl_list));
                
            }
            

            return (regexs_as, pos_as, neg_as, asp_prep, asp_excl);
        }

        public static Tuple<Array<FSeq<uint>, _3>, Array<FSeq<uint>, _3>, string> ComRegexPosNeg(){

            var reg_gen = new RegexGenerator();

            // populate Pos and Neg and regex
            var unsorted_pos = new List<string>();
            var pos_arr = new FSeq<uint>[3];
            var neg_arr = new FSeq<uint>[3];
            var posneg_exmpls = reg_gen.GeneratePosNegExamples();
            var multi_pos = posneg_exmpls.Item1;
            var all_neg = posneg_exmpls.Item2;
            var com = posneg_exmpls.Item3;

            while(true){
                posneg_exmpls = reg_gen.GeneratePosNegExamples();
                multi_pos = posneg_exmpls.Item1;
                all_neg = posneg_exmpls.Item2;
                com = posneg_exmpls.Item3;

                var count = 0;
                foreach (var p in multi_pos){
                    if (Utils.IsSortedCommunity(p)){
                        if (count < 3){
                            pos_arr[count] = new FSeq<uint>(p.Split(" ").ToList().Select(x => Utils.CommunityStr2Int(x)).ToArray());
                            Console.WriteLine("pos: "+p);
                            count++;
                        }
                    }
                    else{
                        unsorted_pos.Add(p);
                    }
                }

                if (count < 3 || unsorted_pos.Count == 0) {continue;} 
                else {break;} 
            }

            var pos_arr1 = new Array<FSeq<uint>, _3>(pos_arr); 
            
            // Neg example population from unsorted pos

            // neg_arr[0] = new FSeq<uint>(unsorted_pos[0].Split(" ")[0].Split(" ").ToList().Select(x => Utils.CommunityStr2Int(x)).ToArray());
            // Console.WriteLine("neg: "+unsorted_pos[0].Split(" ")[0]);
            // neg_arr[1] = new FSeq<uint>(unsorted_pos[0].Split(" ")[1].Split(" ").ToList().Select(x => Utils.CommunityStr2Int(x)).ToArray());
            // Console.WriteLine("neg: "+unsorted_pos[0].Split(" ")[1]);
            // // Console.WriteLine(unsorted_pos.Count);
            // neg_arr[2] = new FSeq<uint>(unsorted_pos[0].Split(" ").ToList().Select(x => Utils.CommunityStr2Int(x)).ToArray());
            // Console.WriteLine("neg: "+unsorted_pos[0]);

            // Neg example population from all_neg
            var count1 = 0;
            foreach (var n in all_neg){
                if (count1 < 3){
                    neg_arr[count1] = new FSeq<uint>(n.Split(" ").ToList().Select(x => Utils.CommunityStr2Int(x)).ToArray());
                    Console.WriteLine("neg: "+n);
                    count1++;
                }
            }

            var neg_arr1 = new Array<FSeq<uint>, _3>(neg_arr);

            return new Tuple<Array<FSeq<uint>, _3>, Array<FSeq<uint>, _3>, string>(pos_arr1, neg_arr1, com);
        }

        public static Tuple<string, Array<uint, _3>> Delcom(int i, string com, FSeq<uint>[] pos_arr){
            
            var reg_gen = new RegexGenerator();
            if ((i) % 2 == 0){ // disjoint from com
                
                while(true){
                    var delcom_regex = reg_gen.disjoint_nums2(); 
                    var spx = reg_gen.GenerateSinglePosEx(delcom_regex);
                    // Console.WriteLine("delcom_regex: "+delcom_regex);
                    // Console.WriteLine("spx: "+spx.Count);
                    // Console.WriteLine("spx: "+string.Join(" ",spx.GetRange(0,Math.Min(5,spx.Count))));
                    if (spx.Count >= 3){
                        var spx3 = spx.GetRange(0,3).Select(x => Utils.CommunityStr2Int(x));
                        // delcom.Add(new Array<uint, _3>(spx3));
                        return new Tuple<string, Array<uint, _3>>(delcom_regex, new Array<uint, _3>(spx3));
                    }
                    else{
                        continue;
                    }
                }
            }

            else{ // one of com
                Random rnd = new Random();
                var com_split = com.Substring(1,com.Length-2).Split(" ").ToList();
                var idx = rnd.Next(0,com_split.Count); // 0 or 1
                var delcom_regex = com_split[idx];
                // Console.WriteLine("delcom_regex: "+delcom_regex[i]);
                var spx3 = new List<uint>();
                for (int j = 0; j < 3; j++){
                    spx3.Add(pos_arr[j].ToList()[idx]);
                }
                // Console.WriteLine("spx3: "+string.Join(" ",spx3));
                // delcom.Add(new Array<uint, _3>(spx3)); 
                return new Tuple<string, Array<uint, _3>>(delcom_regex, new Array<uint, _3>(spx3));
            }
        }

        public static Tuple<string, Array<FSeq<uint>, _3>, Array<FSeq<uint>, _3>> ASRegexPosNeg(){
            var reg_gen = new RegexGenerator();
            var regexs_as = reg_gen.nums2_asp();
            var pexs3_fseq = new List<FSeq<uint>>();
            var nexs3_fseq = new List<FSeq<uint>>();
            while(true){
                regexs_as = reg_gen.nums2_asp();
                Console.WriteLine($"populated regexs_as = {regexs_as}\n");
                var pexs = reg_gen.GenerateASPosEx(regexs_as); // list of all positive examples "AA BB" 
                var nexs = reg_gen.GenerateASNegEx(regexs_as); // list of all negative examples "AA BB"
                // Console.WriteLine(pexs[0]);
                if (pexs.Count >= 3 && nexs.Count >= 3){

                    var pexs3 = pexs.GetRange(0,3); // list of 3 strings "AA BB"
                    Console.WriteLine("pexs3: "+string.Join("|",pexs3));
                    // list of 3 FSeq<uint> 
                    pexs3_fseq = pexs3.Select(x => new FSeq<uint>(x.Split(" ").ToList().Select(y => uint.Parse(y)).ToArray())).ToList();
                    Console.WriteLine("pexs3_fseq: "+string.Join("|",pexs3_fseq));
                    // pos_as.Add(new Array<FSeq<uint>, _3>(pexs3_fseq));

                    var nexs3 = nexs.GetRange(0,3); // list of 3 strings "AA BB"
                    Console.WriteLine("nexs3: "+string.Join("|",nexs3));
                    // list of 3 FSeq<uint> 
                    nexs3_fseq = nexs3.Select(x => new FSeq<uint>(x.Split(" ").ToList().Select(y => uint.Parse(y)).ToArray())).ToList();
                    Console.WriteLine("nexs3_fseq: "+string.Join("|",nexs3_fseq));
                    // neg_as.Add(new Array<FSeq<uint>, _3>(nexs3_fseq));
                    break;
                }
                else{
                    continue;
                }
            }

            return new Tuple<string, Array<FSeq<uint>, _3>, Array<FSeq<uint>, _3>>(regexs_as, new Array<FSeq<uint>, _3>(pexs3_fseq), new Array<FSeq<uint>, _3>(nexs3_fseq));
        }

    }
}

