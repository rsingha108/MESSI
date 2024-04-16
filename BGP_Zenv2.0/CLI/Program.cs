using System;
using System.Numerics;
using System.Collections.Generic;
using ZenLib;
using System.IO;
using System.Linq;
using static ZenLib.Zen;
using BGP;
using System.Security.Cryptography;
using System.IO.Pipes;
using System.Threading;
using System.Text.RegularExpressions;
using CommandLine;

namespace CLI{
    public class Program{
        // declaring variables for community regexes
        private static string[] regexs; // ith element -> A:B C:D
        private static List<Array<FSeq<uint>, _3>> pos;  // ith element -> 3 positive examples a1:b1 c1:d1, a2:b2 c2:d2, a3:b3 c3:d3
        private static List<Array<FSeq<uint>, _3>> neg;  // ith element -> 3 negative examples 'x:x' 'x:x' 'x:x'
        private static string[] delcom_regex; // ith element -> for some i, populate with (A:B or C:D) and for some i, populate with (A:B U C:D)' i.e. disjoint from A:B and C:D
        private static List<Array<uint, _3>> delcom; // ith element -> 3 positive examples of delcom_regex[i]. If delcom_regex[i] is (A:B or C:D) then delcom[i] should be in same as 3 examples in pos, else delcom[i] can be any 3 positive examples of the delcom_regex[i] which is (A:B U C:D)' i.e. disjoint from A:B and C:D

        // declaring variables for AS regexes
        private static string[] regexs_as;
        private static List<Array<FSeq<uint>, _3>> pos_as;
        private static List<Array<FSeq<uint>, _3>> neg_as;
        private static List<Array<uint, _3>> aspathprepend;
        private static List<Array<uint, _3>> aspathexclude;

        // i-th element. p1,p2,p3 and q1,q2,q3. 
        // aspathprepend[i] = [randomly 3 integers]
        // aspathexclude[i] = [any two from (p1 U p2 U p3 U q1 U q2 U q3) and 1 from outside]

        private static Zen<bool> ConstrainDecisionDifferRouteMapSearchSpace(Zen<RouteMapEntry> rme1, Zen<RouteMapEntry> rme2, Zen<IPAttr> ipa){
            var cond1 = And(
                rme1.IsValidRouteMapEntry(regexs, pos, neg, delcom_regex, delcom, regexs_as, pos_as, neg_as, aspathprepend, aspathexclude, 1),
                rme1.GetSetClause().IsNone(), 
                ipa.IsValidIPAttr(pos, neg, pos_as, neg_as),
                Implies(Option.IsSome(rme1.GetMatchClause().GetCommunityList()),
                rme1.GetMatchClause().GetCommunityList().Value().GetIndex() == ipa.GetIndex()),
                Implies(Option.IsSome(rme1.GetMatchClause().GetASPathList()),
                rme1.GetMatchClause().GetASPathList().Value().GetIndex() == ipa.GetIndexAS()),
                Implies(Option.IsSome(rme1.GetSetClause()),
                rme1.GetMatchClause().GetCommunityList().Value().GetIndex() == rme1.GetSetClause().Value().GetIndexCom()),
                Implies(Option.IsSome(rme1.GetSetClause()),
                rme1.GetMatchClause().GetASPathList().Value().GetIndex() == rme1.GetSetClause().Value().GetIndexAs())
            );

            var cond2 = And(
                rme2.IsValidRouteMapEntry(regexs, pos, neg, delcom_regex, delcom, regexs_as, pos_as, neg_as, aspathprepend, aspathexclude, 1),
                rme2.GetSetClause().IsNone(), 
                ipa.IsValidIPAttr(pos, neg, pos_as, neg_as),
                Implies(Option.IsSome(rme2.GetMatchClause().GetCommunityList()),
                rme2.GetMatchClause().GetCommunityList().Value().GetIndex() == ipa.GetIndex()),
                Implies(Option.IsSome(rme2.GetMatchClause().GetASPathList()),
                rme2.GetMatchClause().GetASPathList().Value().GetIndex() == ipa.GetIndexAS()),
                Implies(Option.IsSome(rme2.GetSetClause()),
                rme2.GetMatchClause().GetCommunityList().Value().GetIndex() == rme2.GetSetClause().Value().GetIndexCom()),
                Implies(Option.IsSome(rme2.GetSetClause()),
                rme2.GetMatchClause().GetASPathList().Value().GetIndex() == rme2.GetSetClause().Value().GetIndexAs())
            );

            var cond3 = RouteMapEntryExtensions.DecisionDiffer(rme1, rme2, ipa);
            var cond4 = RouteMapEntryExtensions.GetDifference(rme1, rme2) <= 1;

            return And(cond1, cond2, cond3);
        }

        private static void GenerateAggregationTestCases(){
            var f = new ZenFunction<Router, IPAttr, IPAttr, FSeq<IPAttr>>(RouterExtensions.GetRouteAdvertisements);

            int n_tests = 0;
            foreach(var v in f.GenerateInputs(precondition: (rt, ipa1, ipa2) => And(rt.IsValidRouterConfig(), ipa1.IsValidIPAttr(pos, neg, pos_as, neg_as), ipa2.IsValidIPAttr(pos, neg, pos_as, neg_as)))){
                Console.WriteLine(v.Item1);
                Console.WriteLine(v.Item2);
                Console.WriteLine(v.Item3);
                var res = f.Evaluate(v.Item1, v.Item2, v.Item3);
                Console.WriteLine(res);
                Console.WriteLine();

                JsonUtils.CreateJson(v.Item1, v.Item2, v.Item3, res, n_tests);
                n_tests++;
            }
        }

        private static void GenerateRouteMapTestCases(){
            var f = new ZenFunction<RouteMapEntry, IPAttr, Pair<string, bool, Option<IPAttr>>>(RouteMapEntryExtensions.MatchAgainstEntry);

            int n_tests = 0;
            foreach(var v in f.GenerateInputs(precondition: (rme, ipa) => 
                And(
                    rme.IsValidRouteMapEntry(regexs, pos, neg, delcom_regex, delcom, regexs_as, pos_as, neg_as, aspathprepend, aspathexclude, 3),
                    ipa.IsValidIPAttr(pos, neg, pos_as, neg_as),
                    Implies(Option.IsSome(rme.GetMatchClause().GetCommunityList()),
                    rme.GetMatchClause().GetCommunityList().Value().GetIndex() == ipa.GetIndex()),
                    Implies(Option.IsSome(rme.GetMatchClause().GetASPathList()),
                    rme.GetMatchClause().GetASPathList().Value().GetIndex() == ipa.GetIndexAS()),
                    Implies(Option.IsSome(rme.GetSetClause()),
                    rme.GetMatchClause().GetCommunityList().Value().GetIndex() == rme.GetSetClause().Value().GetIndexCom()),
                    Implies(Option.IsSome(rme.GetSetClause()),
                    rme.GetMatchClause().GetASPathList().Value().GetIndex() == rme.GetSetClause().Value().GetIndexAs())
                ))){

                var test = JsonUtils.CreateJson(v.Item1, v.Item2, f, n_tests);

                Console.Write("Route Map Entry: ");
                Console.WriteLine(v.Item1);
                Console.Write("Incoming Packet: ");
                Console.WriteLine(v.Item2);

                var res = f.Evaluate(v.Item1, v.Item2);
                Console.Write("Tag: ");
                Console.WriteLine(res.Item1);
                Console.Write("Allowed: ");
                Console.WriteLine(res.Item2);
                Console.Write("Rewritten Packet: ");
                Console.WriteLine(res.Item3);
                Console.WriteLine();

                n_tests++;
            }

            Console.WriteLine(n_tests);
        }

        private static void GenerateRouteMapDifferenceTestCases(){
            var f = new ZenFunction<RouteMapEntry, IPAttr, Pair<string, bool, Option<IPAttr>>>(RouteMapEntryExtensions.MatchAgainstEntry);
            var f2 = new ZenFunction<RouteMapEntry, RouteMapEntry, IPAttr, bool>(RouteMapEntryExtensions.DecisionDiffer);
            var f3 = new ZenFunction<RouteMapEntry, RouteMapEntry, int>(RouteMapEntryExtensions.GetDifference);

            int n_tests = 0;
            foreach(var v in f2.GenerateInputs(precondition: (rme1, rme2, ipa) => ConstrainDecisionDifferRouteMapSearchSpace(rme1, rme2, ipa))){
                Console.WriteLine("Route Map 1:");
                Console.WriteLine(v.Item1);

                Console.WriteLine("Route Map 2:");
                Console.WriteLine(v.Item2);

                Console.WriteLine("Route:");
                Console.WriteLine(v.Item3);


                Console.Write("Decision 1: ");
                var dec1 = f.Evaluate(v.Item1, v.Item3).Item2;
                Console.WriteLine(dec1);

                Console.Write("Decision 2: ");
                var dec2 = f.Evaluate(v.Item2, v.Item3).Item2;
                Console.WriteLine(dec2);


                Console.Write("Difference: ");
                var diff = f3.Evaluate(v.Item1, v.Item2);
                Console.WriteLine(diff);

                Console.Write("\n\n\n");
                JsonUtils.CreateJson(v.Item1, v.Item2, v.Item3, dec1, dec2, diff, n_tests);
                n_tests++;
            }

            Console.WriteLine(n_tests);
        }

        private static Zen<bool> ConstrainDecisionDifferAggregationSearchSpace(Zen<Router> r1, Zen<Router> r2, Zen<IPAttr> ipa1, Zen<IPAttr> ipa2){
            var cond1 = And(
                r1.IsValidRouterConfig(),
                r2.IsValidRouterConfig(),
                ipa1.IsValidIPAttr(pos, neg, pos_as, neg_as),
                ipa2.IsValidIPAttr(pos, neg, pos_as, neg_as)
            );

            var cond2 = RouterExtensions.RouterDifferences(r1, r2) <= 1;

            var cond3 = RouterExtensions.DecisionDiffer(r1, r2, ipa1, ipa2);
            
            return And(
                cond1,
                cond2,
                cond3
            );
        }

        private static Zen<bool> ConstrainAggregateRouteSearchSpace(Zen<Router> rt, Zen<IPAttr> ipa1, Zen<IPAttr> ipa2, Zen<IPAttr> ipa3){
            var cond1 = IPAttrExtensions.GetDifference(ipa2, ipa3) <= 1;
            var cond2 = RouterExtensions.DecisionDiffer2(rt, ipa1, ipa2, ipa3);
            var cond3 = And(
                rt.IsValidRouterConfig(),
                ipa1.IsValidIPAttr(pos, neg, pos_as, neg_as),
                ipa2.IsValidIPAttr(pos, neg, pos_as, neg_as),
                ipa3.IsValidIPAttr(pos, neg, pos_as, neg_as)
            );

            return And(cond1, cond2, cond3);
        }

        private static void GenerateAggregationDifferenceTestCases(){
            var f = new ZenFunction<Router, Router, IPAttr, IPAttr, bool>(RouterExtensions.DecisionDiffer);
            var f2 = new ZenFunction<Router, IPAttr, IPAttr, FSeq<IPAttr>>(RouterExtensions.GetRouteAdvertisements);
            var f3 = new ZenFunction<Router, Router, int>(RouterExtensions.RouterDifferences);

            int n_tests = 0;
            foreach(var v in f.GenerateInputs(precondition: (r1, r2, ipa1, ipa2) => ConstrainDecisionDifferAggregationSearchSpace(r1, r2, ipa1, ipa2))){
                Console.WriteLine(n_tests);
                
                Console.Write("Router 1: ");
                Console.WriteLine(v.Item1);

                Console.Write("Router 2: ");
                Console.WriteLine(v.Item2);

                Console.Write("Advertisement 1: ");
                Console.WriteLine(v.Item3);
                Console.Write("Advertisement 2: ");
                Console.WriteLine(v.Item4);

                Console.WriteLine("Decision 1: ");
                var dec1 = f2.Evaluate(v.Item1, v.Item3, v.Item4);
                Console.WriteLine(dec1);

                Console.WriteLine("Decision 2: ");
                var dec2 = f2.Evaluate(v.Item2, v.Item3, v.Item4);
                Console.WriteLine(dec2);

                Console.Write("Config Difference: ");
                Console.WriteLine(f3.Evaluate(v.Item1, v.Item2));

                Console.WriteLine(f.Evaluate(v.Item1, v.Item2, v.Item3, v.Item4));
                
                Console.WriteLine("\n\n");

                JsonUtils.CreateJson(v.Item1, v.Item2, v.Item3, v.Item4, dec1, dec2, n_tests);

                n_tests++;
            }
            Console.WriteLine("\n\nTotal tests generated: "+n_tests);
        }

        private static void GenerateRouteDifferenceTestCases(){
            var f = new ZenFunction<RouteMapEntry, IPAttr, IPAttr, bool>(IPAttrExtensions.DecisionDiffer);
            var f2 = new ZenFunction<RouteMapEntry, IPAttr, Pair<string, bool, Option<IPAttr>>>(RouteMapEntryExtensions.MatchAgainstEntry);
            var f3 = new ZenFunction<IPAttr, IPAttr, int>(IPAttrExtensions.GetDifference);

            int n_tests = 0;

            foreach(var v in f.GenerateInputs(precondition: (rme, ipa1, ipa2) => ConstrainDecisionDifferRouteSearchSpace(rme, ipa1, ipa2))){
                Console.WriteLine(n_tests);
                Console.WriteLine("Route Map: ");
                Console.WriteLine(v.Item1);

                Console.WriteLine("Route 1: ");
                Console.WriteLine(v.Item2);

                Console.WriteLine("Route 2: ");
                Console.WriteLine(v.Item3);

                Console.Write("Decision 1: ");
                bool dec1 = f2.Evaluate(v.Item1, v.Item2).Item2;
                Console.WriteLine(dec1);

                Console.Write("Decision 2: ");
                bool dec2 = f2.Evaluate(v.Item1, v.Item3).Item2;
                Console.WriteLine(dec2);

                Console.Write("Difference: ");
                int diff = f3.Evaluate(v.Item2, v.Item3);
                Console.WriteLine(diff);


                Console.WriteLine("\n");
                JsonUtils.CreateJson(v.Item1, v.Item2, v.Item3, dec1, dec2, diff, n_tests);
                n_tests++;
            }

            Console.WriteLine(n_tests);
        }

        private static Zen<bool> ConstrainDecisionDifferRouteSearchSpace(Zen<RouteMapEntry> rme, Zen<IPAttr> ipa1, Zen<IPAttr> ipa2){
            var cond0 = rme.IsValidRouteMapEntry(regexs, pos, neg, delcom_regex, delcom, regexs_as, pos_as, neg_as, aspathprepend, aspathexclude, 1);

            var cond1 = And( 
                ipa1.IsValidIPAttr(pos, neg, pos_as, neg_as),
                Implies(Option.IsSome(rme.GetMatchClause().GetCommunityList()),
                rme.GetMatchClause().GetCommunityList().Value().GetIndex() == ipa1.GetIndex()),
                Implies(Option.IsSome(rme.GetMatchClause().GetASPathList()),
                rme.GetMatchClause().GetASPathList().Value().GetIndex() == ipa1.GetIndexAS()),
                Implies(Option.IsSome(rme.GetSetClause()),
                rme.GetMatchClause().GetCommunityList().Value().GetIndex() == rme.GetSetClause().Value().GetIndexCom()),
                Implies(Option.IsSome(rme.GetSetClause()),
                rme.GetMatchClause().GetASPathList().Value().GetIndex() == rme.GetSetClause().Value().GetIndexAs())
            );

            var cond2 = And( 
                ipa2.IsValidIPAttr(pos, neg, pos_as, neg_as),
                Implies(Option.IsSome(rme.GetMatchClause().GetCommunityList()),
                rme.GetMatchClause().GetCommunityList().Value().GetIndex() == ipa2.GetIndex()),
                Implies(Option.IsSome(rme.GetMatchClause().GetASPathList()),
                rme.GetMatchClause().GetASPathList().Value().GetIndex() == ipa2.GetIndexAS()),
                rme.GetSetClause().IsNone()
            );

            var cond3 = IPAttrExtensions.GetDifference(ipa1, ipa2) <= 2;

            var cond4 = IPAttrExtensions.DecisionDiffer(rme, ipa1, ipa2);

            return And(cond0, cond1, cond2, cond3, cond4);

        }

        private static void GenerateRouteDifferenceTestCases2(){
            var f = new ZenFunction<Router, IPAttr, IPAttr, IPAttr, bool>(RouterExtensions.DecisionDiffer2);
            var f2 = new ZenFunction<Router, IPAttr, IPAttr, FSeq<IPAttr>>(RouterExtensions.GetRouteAdvertisements);
            int n_tests = 0;

            foreach(var v in f.GenerateInputs(precondition: (rt, ipa1, ipa2, ipa3) => ConstrainAggregateRouteSearchSpace(rt, ipa1, ipa2, ipa3))){                
                // ip1 stays same. ip2 --> ip3
                Console.WriteLine(n_tests);
                Console.WriteLine("Router: ");
                Console.WriteLine(v.Item1);
                Console.WriteLine("Route1: ");
                Console.WriteLine(v.Item2);
                Console.WriteLine("Route2: ");
                Console.WriteLine(v.Item3);
                Console.WriteLine("Route3: ");
                Console.WriteLine(v.Item4);
                Console.WriteLine("Decision1: ");
                var dec1 = f2.Evaluate(v.Item1, v.Item2, v.Item3);
                Console.WriteLine(dec1);
                Console.WriteLine("Decision2: ");
                var dec2 = f2.Evaluate(v.Item1, v.Item2, v.Item4);
                Console.WriteLine(dec2);

                JsonUtils.CreateJson(v.Item1, v.Item2, v.Item3, v.Item4, dec1, dec2, n_tests);
                n_tests++;
            }
            Console.WriteLine("\n\nTotal tests generated: "+n_tests);
        }

        private static void GeneratePathSelectionTestCases(){
            var f = new ZenFunction<Router, RoutesForDecisionProcess, RoutesForDecisionProcess, RoutesForDecisionProcess>(RouterExtensions.PathSelection);

            int n_tests = 0;

            foreach(var v in f.GenerateInputs(precondition: (rt, r1, r2) => And(rt.IsValidRouterConfig(), r1.IsValidRoute(), r2.IsValidRoute(),(r1.GetNgbr()!=r2.GetNgbr())))){
                Console.Write("Router AS Number: ");
                Console.WriteLine(v.Item1.AS);
                Console.Write("Route 1: ");
                Console.WriteLine(v.Item2);
                Console.Write("Route 2: ");
                Console.WriteLine(v.Item3);

                var dec = f.Evaluate(v.Item1, v.Item2, v.Item3);
                Console.WriteLine(dec);
                Console.WriteLine("\n\n\n");
                
                JsonUtils.CreateJson(v.Item1, v.Item2, v.Item3, dec, n_tests);
                n_tests++;
            }

            Console.Write("Total tests generated: ");
            Console.WriteLine(n_tests);

        }

        public class Options{
            [Option('l', "length", Default = 4, HelpText = "The maximum number of regular expressions used for generating AS path and community examples")]
            public int MaximumLength { get; set; }

            [Option('s', "path-selection", Default=false, HelpText = "Flag to enable generation of path selection test cases")]
            public bool PathSelection {get; set; }

            [Option('m', "route-map", Default=false, HelpText = "Flag to enable generation of route-map test cases")]
            public bool RouteMap {get; set; }

            [Option('a', "aggregation", Default=false, HelpText = "Flag to enable generation of aggregation test cases")]
            public bool Aggregation {get; set; }

            [Option('d', "dynamic", Default=false, HelpText = "Flag to enable generation of dynamic test cases. Must be used in combination with a and m flags")]
            public bool Dynamics {get; set; }

            [Option('r', "route", Default=false, HelpText = "Flag to enable generation of route dynamics test-cases. Must be used in combination with a and m flags")]
            public bool Route {get; set; }
        }

        public static void Main(string[] args){
            ZenLib.ZenSettings.PreserveBranches=true;

            var parser = new Parser(with =>
            {
                // ignore case for enum values
                with.CaseInsensitiveEnumValues = true;
                with.HelpWriter = Parser.Default.Settings.HelpWriter;
            });

            parser.ParseArguments<Options>(args)
                   .WithParsed(o => {
                        int size = o.MaximumLength;

                        regexs = new string[size];
                        delcom_regex = new string[size]; 
                        pos = new List<Array<FSeq<uint>, _3>>();
                        neg = new List<Array<FSeq<uint>, _3>>();
                        delcom = new List<Array<uint, _3>>();

                        (regexs, pos, neg, delcom_regex, delcom) = Populate.populate_com(size, regexs, pos, neg, delcom_regex, delcom);

                        regexs_as = new string[size];
                        pos_as = new List<Array<FSeq<uint>, _3>>();
                        neg_as = new List<Array<FSeq<uint>, _3>>();
                        aspathprepend = new List<Array<uint, _3>>();
                        aspathexclude = new List<Array<uint, _3>>();
                        (regexs_as, pos_as, neg_as, aspathprepend, aspathexclude) = Populate.populate_as(size, regexs_as, pos_as, neg_as, aspathprepend, aspathexclude);

                        Populate.save_regex_pos_neg("regex-pos-neg.txt", regexs, pos, neg, delcom_regex, delcom, regexs_as, pos_as, neg_as, aspathprepend, aspathexclude);

                        Console.WriteLine("Generating inputs...\n");
                        if(o.PathSelection){
                                GeneratePathSelectionTestCases();
                        }
                        else if(o.Aggregation){
                                if(o.Dynamics) GenerateAggregationDifferenceTestCases();
                                else if(o.Route) GenerateRouteDifferenceTestCases2();
                                else GenerateAggregationTestCases();
                        }
                        else if(o.RouteMap){
                                if(o.Dynamics) GenerateRouteMapDifferenceTestCases();
                                else if(o.Route) GenerateRouteDifferenceTestCases();
                                else GenerateRouteMapTestCases();
                        }
                   });          
        }
    }
}
