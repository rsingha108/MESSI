using System;
using System.Collections.Generic;
using ZenLib;
using static ZenLib.Zen;

namespace BGP
{
    public class MatchClause{
        //public Option<LocalPreference> LP;
        //public Option<MultiExitDiscriminator> MED;
        public Option<uint> LP;
        public Option<uint> MED;
        public Option<PrefixList> PreList;
        public Option<CommunityListEntry> ComList;
        public Option<ASPathListEntry> ASPathList;

        public Zen<MatchClause> Create(Zen<Option<uint>> lp, Zen<Option<uint>> med, Zen<Option<PrefixList>> prelist, Zen<Option<CommunityListEntry>> cle, Zen<Option<ASPathListEntry>> aspath){
            return Zen.Create<MatchClause>(("LP", lp), ("MED", med), ("PreList", prelist), ("ComList", cle), ("ASPathList", aspath));
        }

        public override string ToString(){
            return $"Local Preference: {LP}, MED: {MED}, Prefix List: {PreList}, Community List: {ComList}, AS Path List: {ASPathList}";
        }
    }

    public static class MatchClauseExtensions{
        public static Zen<Option<uint>> GetLP(this Zen<MatchClause> m) => m.GetField<MatchClause, Option<uint>>("LP");
        public static Zen<Option<uint>> GetMED(this Zen<MatchClause> m) => m.GetField<MatchClause, Option<uint>>("MED");
        public static Zen<Option<PrefixList>> GetPrefixList(this Zen<MatchClause> m) => m.GetField<MatchClause, Option<PrefixList>>("PreList");
        public static Zen<Option<CommunityListEntry>> GetCommunityList(this Zen<MatchClause> m) => m.GetField<MatchClause, Option<CommunityListEntry>>("ComList");
        public static Zen<Option<ASPathListEntry>> GetASPathList(this Zen<MatchClause> m) => m.GetField<MatchClause, Option<ASPathListEntry>>("ASPathList");

        public static Zen<bool> IsValidMatchClause(this Zen<MatchClause> mc, string[] regex_com, List<Array<FSeq<uint>, _3>> pos_com,
                                                                            string[] regex_as, List<Array<FSeq<uint>, _3>> pos_as){
            var cond = Not(And(Option.IsNone(mc.GetLP()), 
                               Option.IsNone(mc.GetMED()), 
                               Option.IsNone(mc.GetPrefixList()),
                               Option.IsNone(mc.GetCommunityList()),
                               Option.IsNone(mc.GetASPathList())
                               )
                        ); // All entries cannot be null

            var cond2 = Or(
                And(
                    Option.IsSome(mc.GetLP()),
                    Option.IsNone(mc.GetMED()), 
                    Option.IsNone(mc.GetPrefixList()),
                    Option.IsNone(mc.GetCommunityList()),
                    Option.IsNone(mc.GetASPathList())
                ),
                And(
                    Option.IsNone(mc.GetLP()),
                    Option.IsSome(mc.GetMED()), 
                    Option.IsNone(mc.GetPrefixList()),
                    Option.IsNone(mc.GetCommunityList()),
                    Option.IsNone(mc.GetASPathList())
                ),
                And(
                    Option.IsNone(mc.GetLP()),
                    Option.IsNone(mc.GetMED()), 
                    Option.IsSome(mc.GetPrefixList()),
                    Option.IsNone(mc.GetCommunityList()),
                    Option.IsNone(mc.GetASPathList())
                ),
                And(
                    Option.IsNone(mc.GetLP()),
                    Option.IsNone(mc.GetMED()), 
                    Option.IsNone(mc.GetPrefixList()),
                    Option.IsSome(mc.GetCommunityList()),
                    Option.IsNone(mc.GetASPathList())
                ),
                And(
                    Option.IsNone(mc.GetLP()),
                    Option.IsNone(mc.GetMED()), 
                    Option.IsNone(mc.GetPrefixList()),
                    Option.IsNone(mc.GetCommunityList()),
                    Option.IsSome(mc.GetASPathList())
                ),
                And(
                    Option.IsNone(mc.GetLP()),
                    Option.IsNone(mc.GetMED()), 
                    Option.IsNone(mc.GetPrefixList()),
                    Option.IsNone(mc.GetCommunityList()),
                    Option.IsNone(mc.GetASPathList())
                )
            );
            
            return And(Implies(Option.IsSome(mc.GetLP()), And(mc.GetLP().Value() >= 100, mc.GetLP().Value() <= 900)),
                        Implies(Option.IsSome(mc.GetMED()), And(mc.GetMED().Value() > 0, mc.GetMED().Value() <= 800)),
                        Implies(Option.IsSome(mc.GetPrefixList()), mc.GetPrefixList().Value().IsValidPrefixList()), 
                        Implies(Option.IsSome(mc.GetCommunityList()), mc.GetCommunityList().Value().IsValidCommunityListEntry(regex_com, pos_com)), 
                        // Implies(Option.IsSome(mc.GetASPathList()), mc.GetASPathList().Value().IsValidASPathListEntry(regex_as, pos_as)), cond, cond2);
                        Implies(Option.IsSome(mc.GetASPathList()), mc.GetASPathList().Value().IsValidASPathListEntry(regex_as, pos_as)), cond2);
        }

        public static Zen<int> GetAttrDifference(this Zen<MatchClause> me1, Zen<MatchClause> me2){
            Zen<int> count = 0;
            count = count + ASPathListEntryExtensions.GetDifference(me1.GetASPathList(), me2.GetASPathList());
            count = count + CommunityListEntryExtensions.GetDifference(me1.GetCommunityList(), me2.GetCommunityList());
            count = count + PrefixListExtensions.GetDifference(me1.GetPrefixList(), me2.GetPrefixList());

            count = If<int>(
                me1.GetLP().IsNone(),
                If<int>(
                    me2.GetLP().IsNone(),
                    count,
                    Constants.INF
                ),
                If<int>(
                    me2.GetLP().IsNone(),
                    Constants.INF,
                    If(me1.GetLP() != me2.GetLP(), count + 1, count)
                )
            );

            count = If<int>(
                me1.GetMED().IsNone(),
                If<int>(
                    me2.GetMED().IsNone(),
                    count,
                    Constants.INF
                ),
                If<int>(
                    me2.GetMED().IsNone(),
                    Constants.INF,
                    If<int>(me1.GetMED().Value() != me2.GetMED().Value(), count + 1, count)
                )
            );

            return count;
        }

        public static Zen<Pair<string, bool>> MatchAgainstClause(this Zen<MatchClause> m, Zen<IPAttr> ipa){
            var lp_clause = m.GetLP();
            var med_clause = m.GetMED();
            var pre_clause = m.GetPrefixList();
            var com_clause = m.GetCommunityList();
            var as_clause = m.GetASPathList();
            // if option.some then check else return true
            // none means empty clause so default pass through

            var expr1 = If<bool>(
                Option.IsSome(lp_clause),
                If<bool>(
                    lp_clause.Value() == ipa.GetLP(),
                    true,  
                    false
                ),
                true
            );


            // s is the tag
            var s = If<string>(
                Option.IsSome(lp_clause),
                If<string>(
                    lp_clause.Value() == ipa.GetLP(),
                    "L1",
                    "L2"
                ),
                ""
            );

            var expr2 = If<bool>(
                Option.IsSome(med_clause),
                If<bool>(
                    med_clause.Value() == ipa.GetMED(),
                    true, 
                    false
                ),
                true
            );

            s = If<string>(
                Option.IsSome(med_clause),
                If<string>(
                    med_clause.Value() == ipa.GetMED(),
                    s + "M1", 
                    s + "M2"
                ),
                s + ""
            );

            var expr3 = If<bool>(
                Option.IsSome(pre_clause),
                If<bool>(
                   pre_clause.Value().MatchAgainstPrefixList(ipa),
                   true, 
                   false
                ),
                true
            );

            s = If<string>(
                Option.IsSome(pre_clause),
                If<string>(
                   pre_clause.Value().MatchAgainstPrefixList(ipa),
                   s + "P1", 
                   s + "P2"
                ),
                s + ""
            );


            var expr4 = If<bool>(
                Option.IsSome(com_clause),
                If<bool>(
                    com_clause.Value().MatchAgainstCommunityListEntry(ipa),
                    true,
                    false
                ),
                true
            );

            s = If<string>(
                Option.IsSome(com_clause),
                If<string>(
                    com_clause.Value().MatchAgainstCommunityListEntry(ipa),
                    s + "C1",
                    s + "C2"
                ),
                s + ""
            );

            var expr5 = If<bool>(
                Option.IsSome(as_clause),
                If<bool>(
                    as_clause.Value().MatchAgainstASPathListEntry(ipa),
                    true,
                    false
                ),
                true
            );

            s = If<string>(
                Option.IsSome(as_clause),
                If<string>(
                    as_clause.Value().MatchAgainstASPathListEntry(ipa),
                    s + "A1",
                    s + "A2"
                ),
                s + ""
            );

            
            return Pair.Create<string, bool>(
                s,
                If<bool>(
                    Utils.AndIf(expr1, Utils.AndIf(expr2, Utils.AndIf(expr3, Utils.AndIf(expr4, expr5)))),
                    true,
                    false
                )
            );
        }
    }
}
