using System;
using System.Numerics;
using System.Collections.Generic;
using ZenLib;
using System.IO;
using System.Linq;
using static ZenLib.Zen;
using BGP;
using CLI;

ZenLib.ZenSettings.PreserveBranches=true;

var size = 4;

var regexs = new string[size]; // ith element -> A:B C:D
var pos = new List<Array<FSeq<uint>, _3>>(); // ith element -> 3 positive examples a1:b1 c1:d1, a2:b2 c2:d2, a3:b3 c3:d3 
var neg = new List<Array<FSeq<uint>, _3>>(); // ith element -> 3 negative examples 'x:x' 'x:x' 'x:x'
var delcom_regex = new string[size]; // ith element -> for some i, populate with (A:B or C:D) and for some i, populate with (A:B U C:D)' i.e. disjoint from A:B and C:D
var delcom = new List<Array<uint, _3>>(); // ith element -> 3 positive examples of delcom_regex[i]. If delcom_regex[i] is (A:B or C:D) then delcom[i] should be in same as 3 examples in pos, else delcom[i] can be any 3 positive examples of the delcom_regex[i] which is (A:B U C:D)' i.e. disjoint from A:B and C:D

// Populate Community and AS Regexes with Positive and Negative Examples

(regexs, pos, neg, delcom_regex, delcom) = Populate.populate_com(size, regexs, pos, neg, delcom_regex, delcom);

var regexs_as = new string[size];
var pos_as = new List<Array<FSeq<uint>, _3>>();
var neg_as = new List<Array<FSeq<uint>, _3>>();
var aspathprepend = new List<Array<uint, _3>>();
var aspathexclude = new List<Array<uint, _3>>(); 

// i-th element. p1,p2,p3 and q1,q2,q3. 
// aspathprepend[i] = [randomly 3 integers]
// aspathexclude[i] = [any two from (p1 U p2 U p3 U q1 U q2 U q3) and 1 from outside]

(regexs_as, pos_as, neg_as, aspathprepend, aspathexclude) = Populate.populate_as(size, regexs_as, pos_as, neg_as, aspathprepend, aspathexclude);

var reg_gen = new RegexGenerator();

// Save Regexes and Positive and Negative Examples to File

Populate.save_regex_pos_neg("regex-pos-neg.txt", regexs, pos, neg, delcom_regex, delcom, regexs_as, pos_as, neg_as, aspathprepend, aspathexclude);

Zen<bool> ConstrainDecisionDifferRouteMapSearchSpace(Zen<RouteMapEntry> rme1, Zen<RouteMapEntry> rme2, Zen<IPAttr> ipa){
    var cond1 = And(
        rme1.IsValidRouteMapEntry(regexs, pos, neg, delcom_regex, delcom, regexs_as, pos_as, neg_as, aspathprepend, aspathexclude), 
        ipa.IsValidIPAttr(pos, neg, pos_as, neg_as),
        Implies(Option.IsSome(rme1.GetMatchClause().GetCommunityList()),
        rme1.GetMatchClause().GetCommunityList().Value().GetIndex() == ipa.GetIndex()),
        Implies(Option.IsSome(rme1.GetMatchClause().GetASPathList()),
        rme1.GetMatchClause().GetASPathList().Value().GetIndex() == ipa.GetIndexAS()),
        Implies(Option.IsSome(rme1.GetSetClause()),
        rme1.GetMatchClause().GetCommunityList().Value().GetIndex() == rme1.GetSetClause().Value().GetIndex()),
        Implies(Option.IsSome(rme1.GetSetClause()),
        rme1.GetMatchClause().GetASPathList().Value().GetIndex() == rme1.GetSetClause().Value().GetIndexAs())
    );

    var cond2 = And(
        rme2.IsValidRouteMapEntry(regexs, pos, neg, delcom_regex, delcom, regexs_as, pos_as, neg_as, aspathprepend, aspathexclude), 
        ipa.IsValidIPAttr(pos, neg, pos_as, neg_as),
        Implies(Option.IsSome(rme2.GetMatchClause().GetCommunityList()),
        rme2.GetMatchClause().GetCommunityList().Value().GetIndex() == ipa.GetIndex()),
        Implies(Option.IsSome(rme2.GetMatchClause().GetASPathList()),
        rme2.GetMatchClause().GetASPathList().Value().GetIndex() == ipa.GetIndexAS()),
        Implies(Option.IsSome(rme2.GetSetClause()),
        rme2.GetMatchClause().GetCommunityList().Value().GetIndex() == rme2.GetSetClause().Value().GetIndex()),
        Implies(Option.IsSome(rme2.GetSetClause()),
        rme2.GetMatchClause().GetASPathList().Value().GetIndex() == rme2.GetSetClause().Value().GetIndexAs())
    );

    var cond3 = RouteMapEntryExtensions.DecisionDiffer(rme1, rme2, ipa);

    var cond5 = Implies(
            And(
                rme1.GetMatchClause().GetPrefixList().IsSome(),
                rme2.GetMatchClause().GetPrefixList().IsSome()
            ),
            
            And(
                rme1.GetMatchClause().GetPrefixList().Value().GetValue().ToArray()[1].GetAny(),
                rme1.GetMatchClause().GetPrefixList().Value().GetValue().ToArray()[2].GetAny(),
                rme2.GetMatchClause().GetPrefixList().Value().GetValue().ToArray()[1].GetAny(),
                rme2.GetMatchClause().GetPrefixList().Value().GetValue().ToArray()[2].GetAny(),
                Not(rme1.GetMatchClause().GetPrefixList().Value().GetValue().ToArray()[1].GetPermit()),
                Not(rme1.GetMatchClause().GetPrefixList().Value().GetValue().ToArray()[2].GetPermit()),
                Not(rme2.GetMatchClause().GetPrefixList().Value().GetValue().ToArray()[1].GetPermit()),
                Not(rme2.GetMatchClause().GetPrefixList().Value().GetValue().ToArray()[2].GetPermit())
            )
    );

    var cond4 = RouteMapEntryExtensions.GetDifference(rme1, rme2) <= 1;

    return And(cond1, cond2, cond3, cond4, cond5);
}

///////////// AGGREGATE ///////////////


void GenerateAggregationTestCases(){
    var f = new ZenFunction<Router, IPAttr, IPAttr, FSeq<IPAttr>>(RouterExtensions.GetRouteAdvertisements);

    foreach(var v in f.GenerateInputs(precondition: (rt, ipa1, ipa2) => And(rt.IsValidRouterConfig(), ipa1.IsValidIPAttr(pos, neg, pos_as, neg_as), ipa2.IsValidIPAttr(pos, neg, pos_as, neg_as)))){
        Console.WriteLine(v.Item1);
        Console.WriteLine(v.Item2);
        Console.WriteLine(v.Item3);

        Console.WriteLine(f.Evaluate(v.Item1, v.Item2, v.Item3));

        Console.WriteLine();
    }
}

///////////// ROUTE-MAP LOGIC ///////////////


void GenerateRouteMapTestCases(){
    var f = new ZenFunction<RouteMapEntry, IPAttr, Pair<string, bool, Option<IPAttr>>>(RouteMapEntryExtensions.MatchAgainstEntry);

    int n_tests = 0;
    foreach(var v in f.GenerateInputs(precondition: (rme, ipa) => 
        And(
            rme.IsValidRouteMapEntry(regexs, pos, neg, delcom_regex, delcom, regexs_as, pos_as, neg_as, aspathprepend, aspathexclude), 
            ipa.IsValidIPAttr(pos, neg, pos_as, neg_as),
            Implies(Option.IsSome(rme.GetMatchClause().GetCommunityList()),
            rme.GetMatchClause().GetCommunityList().Value().GetIndex() == ipa.GetIndex()),
            Implies(Option.IsSome(rme.GetMatchClause().GetASPathList()),
            rme.GetMatchClause().GetASPathList().Value().GetIndex() == ipa.GetIndexAS()),
            Implies(Option.IsSome(rme.GetSetClause()),
            rme.GetMatchClause().GetCommunityList().Value().GetIndex() == rme.GetSetClause().Value().GetIndex()),
            Implies(Option.IsSome(rme.GetSetClause()),
            rme.GetMatchClause().GetASPathList().Value().GetIndex() == rme.GetSetClause().Value().GetIndexAs())
        ))){

        var test = JsonUtils.CreateJson(v.Item1, v.Item2, f, n_tests);

        Console.WriteLine("Test Case : "+n_tests);

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

void GenerateRouteMapDifferenceTestCases(){
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

        Console.Write("\n\n\n\n");

        JsonUtils.CreateJson(v.Item1, v.Item2, v.Item3, dec1, dec2, diff, n_tests);
        n_tests++;
    }
}

Console.WriteLine("generating inputs...\n");

// GenerateAggregationTestCases();
GenerateRouteMapDifferenceTestCases();
// GenerateRouteMapTestCases();