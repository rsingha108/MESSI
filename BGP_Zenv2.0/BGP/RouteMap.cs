using System;
using System.Collections.Generic;
using ZenLib;
using static ZenLib.Zen;

namespace BGP{
    /// <summary>
    /// Defining Infinity
    /// </summary>
    public class Constants{
        public const int INF = 1000000;
    }

    /// <summary>
    /// One entry in a route map
    /// </summary>
    public class RouteMapEntry{
        
        /// <summary>
        /// Permit or deny 
        /// </summary>
        public bool Permit;

        /// <summary>
        /// Match clauses
        /// </summary>
        public MatchClause MC;

        /// <summary>
        /// Set clauses
        /// </summary>
        public Option<SetClause> SC;

        /// <summary>
        /// Create a Zen route map entry
        /// </summary>
        /// <param name="permit">permit or deny (a boolean)</param>
        /// <param name="mc">match clause</param>
        /// <param name="sc">set clause</param>
        /// <returns>a Zen object</returns>
        public Zen<RouteMapEntry> Create(Zen<bool> permit, Zen<MatchClause> mc, Zen<Option<SetClause>> sc){
            return Zen.Create<RouteMapEntry>(
                ("Permit", permit),
                ("MC", mc),
                ("SC", sc)
            );
        }

        /// <summary>
        /// String representation of Route-Map Stanza
        /// </summary>
        /// <returns>a string</returns>
        public override string ToString(){
            return $"Permit: {Permit}\nMatch Clause: {MC}\nSet Clause: {SC}";
        }
    }

    /// <summary>
    /// Route-map entry extensions class
    /// </summary>
    public static class RouteMapEntryExtensions{
        /// <summary>
        /// Retrieve the permit/deny flag
        /// </summary>
        /// <param name="rme">route-map stanza</param>
        /// <returns>A boolean</returns>
        public static Zen<bool> GetPermit(this Zen<RouteMapEntry> rme) => rme.GetField<RouteMapEntry, bool>("Permit");
        
        /// <summary>
        /// Get the stanza's match clause
        /// </summary>
        /// <param name="rme">route-map stanza</param>
        /// <returns>the match clause</returns>
        public static Zen<MatchClause> GetMatchClause(this Zen<RouteMapEntry> rme) => rme.GetField<RouteMapEntry, MatchClause>("MC");
        
        /// <summary>
        /// Get the stanza's set clause
        /// </summary>
        /// <param name="rme">route-map stanza</param>
        /// <returns>the set clause</returns>
        public static Zen<Option<SetClause>> GetSetClause(this Zen<RouteMapEntry> rme) => rme.GetField<RouteMapEntry, Option<SetClause>>("SC");
        
        /// <summary>
        /// Check whether the route-map stanza is well-formed
        /// </summary>
        /// <param name="rme">the route-map stanza</param>
        /// <param name="regex_com">the list of allowed community regexes</param>
        /// <param name="pos_com">list of positive examples corresponding to each community regex</param>
        /// <param name="neg_com">list of negative examples corresponding to each community regex</param>
        /// <param name="delcom_regex">list of allowed delete-community regexes</param>
        /// <param name="delcom">list of positive examples corresponding to each delete-community regex</param>
        /// <param name="regex_as">list of allowed AS path regexes</param>
        /// <param name="pos_as">list of positive examples corresponding to each AS path regex</param>
        /// <param name="neg_as">list of negative examples corresponding to each AS path regex</param>
        /// <param name="aspathprepend">list of allowed AS path values that can be prepended</param>
        /// <param name="aspathexclude">list of allowed AS path values that can be deleted</param>
        /// <param name="num_prefixes">number of prefixes in the prefix-list: 1 or 3</param>
        /// <returns>A boolean</returns>
        public static Zen<bool> IsValidRouteMapEntry(this Zen<RouteMapEntry> rme, string[] regex_com, List<Array<FSeq<uint>, _3>> pos_com, List<Array<FSeq<uint>, _3>> neg_com, string[] delcom_regex, List<Array<uint, _3>> delcom,
                                                                                    string[] regex_as, List<Array<FSeq<uint>, _3>> pos_as, List<Array<FSeq<uint>, _3>> neg_as,
                                                                                    List<Array<uint, _3>> aspathprepend, List<Array<uint, _3>> aspathexclude, int num_prefixes){
            return And(
                rme.GetMatchClause().IsValidMatchClause(regex_com, pos_com, regex_as, pos_as, num_prefixes),
                Implies(Option.IsSome(rme.GetSetClause()), rme.GetSetClause().Value().IsValidSetClause(delcom_regex, delcom, aspathprepend, aspathexclude))
            );
        }

        /// <summary>
        /// Match the input route advertisement against the match clause ans apply the set actions
        /// </summary>
        /// <param name="rme">route-map stanza</param>
        /// <param name="ipa">input route</param>
        /// <returns>A triplet containing the tag, decision, and output route</returns>
        public static Zen<Pair<string, bool, Option<IPAttr>>> MatchAgainstEntry(this Zen<RouteMapEntry> rme, Zen<IPAttr> ipa){
            var dec = rme.GetMatchClause().MatchAgainstClause(ipa);
            var setipa = If(
                Option.IsSome(rme.GetSetClause()),
                rme.GetSetClause().Value().SetAttr(ipa),
                Pair.Create<string, Option<IPAttr>>("", Option.Create(ipa))
            );

            return If(
                dec.Item2(),
                If(
                    rme.GetPermit(),
                    Pair.Create<string, bool, Option<IPAttr>>(dec.Item1() + setipa.Item1(), True(), setipa.Item2()),
                    Pair.Create<string, bool, Option<IPAttr>>(dec.Item1(), False(), Option.None<IPAttr>())
                ),
                Pair.Create<string, bool, Option<IPAttr>>(dec.Item1(), False(), Option.None<IPAttr>())
            );
        }


        /// <summary>
        /// Calculates the difference between two route-map stanzas
        /// </summary>
        /// <param name="r1">the first stanza</param>
        /// <param name="r2">the second stanza</param>
        /// <returns>an integer</returns>
        public static Zen<int> GetDifference(this Zen<RouteMapEntry> r1, Zen<RouteMapEntry> r2){
            Zen<int> count = 0;
            count = MatchClauseExtensions.GetAttrDifference(r1.GetMatchClause(), r2.GetMatchClause());
            count = If<int>(
                r1.GetPermit() != r2.GetPermit(),
                count + 1,
                count
            );

            return count;
        }
        
        /// <summary>
        /// Check whether two route-map stanzas return different decisions for the same input route
        /// </summary>
        /// <param name="r1">the first stanza</param>
        /// <param name="r2">the second stanza</param>
        /// <param name="ipa">input route</param>
        /// <returns>A boolean</returns>
        public static Zen<bool> DecisionDiffer(Zen<RouteMapEntry> r1, Zen<RouteMapEntry> r2, Zen<IPAttr> ipa){
            var dec1 = r1.MatchAgainstEntry(ipa);
            var dec2 = r2.MatchAgainstEntry(ipa);

            return If<bool>(
                dec1.Item2() == dec2.Item2(),
                false,
                true
            );
        } 
    }
}