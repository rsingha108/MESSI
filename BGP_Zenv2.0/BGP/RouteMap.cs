using System;
using System.Collections.Generic;
using ZenLib;
using static ZenLib.Zen;

namespace BGP{
    public class Constants{
        public const int INF = 1000000;
    }

    public class RouteMapEntry{
        public bool Permit;
        public MatchClause MC;
        public Option<SetClause> SC;

        public Zen<RouteMapEntry> Create(Zen<bool> permit, Zen<MatchClause> mc, Zen<Option<SetClause>> sc){
            return Zen.Create<RouteMapEntry>(
                ("Permit", permit),
                ("MC", mc),
                ("SC", sc)
            );
        }

        public override string ToString(){
            return $"Permit: {Permit}\nMatch Clause: {MC}\nSet Clause: {SC}";
        }
    }

    public static class RouteMapEntryExtensions{
        public static Zen<bool> GetPermit(this Zen<RouteMapEntry> rme) => rme.GetField<RouteMapEntry, bool>("Permit");
        public static Zen<MatchClause> GetMatchClause(this Zen<RouteMapEntry> rme) => rme.GetField<RouteMapEntry, MatchClause>("MC");
        public static Zen<Option<SetClause>> GetSetClause(this Zen<RouteMapEntry> rme) => rme.GetField<RouteMapEntry, Option<SetClause>>("SC");
        public static Zen<bool> IsValidRouteMapEntry(this Zen<RouteMapEntry> rme, string[] regex_com, List<Array<FSeq<uint>, _3>> pos_com, List<Array<FSeq<uint>, _3>> neg_com, string[] delcom_regex, List<Array<uint, _3>> delcom,
                                                                                    string[] regex_as, List<Array<FSeq<uint>, _3>> pos_as, List<Array<FSeq<uint>, _3>> neg_as,
                                                                                    List<Array<uint, _3>> aspathprepend, List<Array<uint, _3>> aspathexclude){
            return And(
                rme.GetMatchClause().IsValidMatchClause(regex_com, pos_com, regex_as, pos_as),
                Implies(Option.IsSome(rme.GetSetClause()), rme.GetSetClause().Value().IsValidSetClause(delcom_regex, delcom, aspathprepend, aspathexclude))
            );
        }
        public static Zen<Pair<string, bool, Option<IPAttr>>> MatchAgainstEntry(this Zen<RouteMapEntry> rme, Zen<IPAttr> ipa){
            var dec = rme.GetMatchClause().MatchAgainstClause(ipa);
            var setipa = If(
                Option.IsSome(rme.GetSetClause()),
                rme.GetSetClause().Value().SetLP(ipa),
                Pair.Create<string, Option<IPAttr>>("", Option.Create(ipa))
            );

            /*return If(
                dec.Item2(),
                If(
                    Option.IsSome(rme.GetSetClause()),
                    If(
                        rme.GetPermit(),
                        Pair.Create<bool, Option<IPAttr>>(true, rme.GetSetClause().Value().SetLP(ipa)),
                        Pair.Create<bool, Option<IPAttr>>(false, rme.GetSetClause().Value().SetLP(ipa))
                    ),
                    If(
                        rme.GetPermit(),
                        Pair.Create<bool, Option<IPAttr>>(true, Option.None<IPAttr>()),
                        Pair.Create<bool, Option<IPAttr>>(false, Option.None<IPAttr>())
                    )
                ),
                Pair.Create<bool, Option<IPAttr>>(False(), Option.None<IPAttr>())
            );*/

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