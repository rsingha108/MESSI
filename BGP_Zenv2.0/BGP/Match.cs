using System;
using System.Collections.Generic;
using System.IO;
using ZenLib;
using static ZenLib.Zen;

namespace BGP
{
    /// <summary>
    /// Match clause class
    /// </summary>
    public class MatchClause{
        
        /// <summary>
        /// Local preference
        /// </summary>
        public Option<uint> LP;

        /// <summary>
        /// Metric
        /// </summary>
        public Option<uint> MED;

        /// <summary>
        /// Prefix list
        /// </summary>
        public Option<PrefixList> PrList;

        /// <summary>
        /// Community List (currently only supports one entry)
        /// </summary>
        public Option<CommunityListEntry> ComList;

        /// <summary>
        /// AS Path List (currently only supports one entry)
        /// </summary>
        public Option<ASPathListEntry> ASPathList;

        /// <summary>
        /// Create a Zen match clause
        /// </summary>
        /// <param name="lp"> local preference </param>
        /// <param name="med"> metric </param>
        /// <param name="prelist"> prefix list </param>
        /// <param name="cle"> community list entry </param>
        /// <param name="aspath"> AS Path list entry </param>
        /// <returns> Zen match clause </returns>
        public Zen<MatchClause> Create(
            Zen<Option<uint>> lp, 
            Zen<Option<uint>> med, 
            Zen<Option<PrefixList>> prelist, 
            Zen<Option<CommunityListEntry>> cle, 
            Zen<Option<ASPathListEntry>> aspath
        ){
            return Zen.Create<MatchClause>(
                ("LP", lp), 
                ("MED", med), 
                ("PrList", prelist), 
                ("ComList", cle), 
                ("ASPathList", aspath)
            );
        }

        /// <summary>
        /// Converts Zen match clause to a string
        /// </summary>
        /// <returns> the string </returns>
        public override string ToString(){
            return $"Local Preference: {LP}, MED: {MED}, Prefix List: {PrList}, Community List: {ComList}, AS Path List: {ASPathList}";
        }
    }

    public static class MatchClauseExtensions{
        /// <summary>
        /// Gets the local preference value
        /// </summary>
        /// <param name="m"> match clause </param>
        /// <returns> local preference </returns>
        public static Zen<Option<uint>> GetLP(this Zen<MatchClause> m) => m.GetField<MatchClause, Option<uint>>("LP");
        
        /// <summary>
        /// Gets the metric value
        /// </summary>
        /// <param name="m"> match clause </param>
        /// <returns> metric </returns>
        public static Zen<Option<uint>> GetMED(this Zen<MatchClause> m) => m.GetField<MatchClause, Option<uint>>("MED");

        /// <summary>
        /// Gets the prefix list
        /// </summary>
        /// <param name="m"> match clause </param>
        /// <returns> prefix list </returns>
        public static Zen<Option<PrefixList>> GetPrefixList(this Zen<MatchClause> m) => m.GetField<MatchClause, Option<PrefixList>>("PrList");

        /// <summary>
        /// Gets the community list
        /// </summary>
        /// <param name="m"> match clause </param>
        /// <returns> community list </returns>
        public static Zen<Option<CommunityListEntry>> GetCommunityList(this Zen<MatchClause> m) => m.GetField<MatchClause, Option<CommunityListEntry>>("ComList");
        
        /// <summary>
        /// Gets the AS path list
        /// </summary>
        /// <param name="m"> match clause </param>
        /// <returns> AS path list </returns>
        public static Zen<Option<ASPathListEntry>> GetASPathList(this Zen<MatchClause> m) => m.GetField<MatchClause, Option<ASPathListEntry>>("ASPathList");

        
        /// <summary>
        /// conditions for valid match clause:
        /// 1. at most one entry (this prevents the number of cases from blowing up)
        /// 2. entry must be valid
        /// </summary>
        /// <param name="mc"> match clause </param>
        /// <param name="regex_com"> list of community regexes </param>
        /// <param name="pos_com"> postive regex matches for each community regex </param>
        /// <param name="regex_as"> list of AS path regexes </param>
        /// <param name="pos_as"> positive regex matches for each AS path regex </param>
        /// <returns> a boolean </returns>
        public static Zen<bool> IsValidMatchClause(this Zen<MatchClause> mc, string[] regex_com, List<Array<FSeq<uint>, _3>> pos_com,
                                                                            string[] regex_as, List<Array<FSeq<uint>, _3>> pos_as, int num_prefixes){
            
            // match clause can have at most one most matching condition
            Zen<int> num_conditions = 0;
            num_conditions = If(Option.IsSome(mc.GetLP()), num_conditions + 1, num_conditions);
            num_conditions = If(Option.IsSome(mc.GetMED()), num_conditions + 1, num_conditions);
            num_conditions = If(Option.IsSome(mc.GetPrefixList()), num_conditions + 1, num_conditions);
            num_conditions = If(Option.IsSome(mc.GetCommunityList()), num_conditions + 1, num_conditions);
            num_conditions = If(Option.IsSome(mc.GetASPathList()), num_conditions + 1, num_conditions);
            Zen<bool> single_entry = num_conditions <= 1;
            
            
            return And(
                        single_entry,
                        Implies(Option.IsSome(mc.GetLP()), And(mc.GetLP().Value() >= 100, mc.GetLP().Value() <= 900)), // limit the range of local-preference
                        Implies(Option.IsSome(mc.GetMED()), And(mc.GetMED().Value() > 0, mc.GetMED().Value() <= 800)), // limit the range of MED
                        Implies(Option.IsSome(mc.GetPrefixList()), mc.GetPrefixList().Value().IsValidPrefixList(num_prefixes)), // check whether prefix list is valid
                        Implies(Option.IsSome(mc.GetCommunityList()), mc.GetCommunityList().Value().IsValidCommunityListEntry(regex_com, pos_com)), // check whether community list is valid
                        Implies(Option.IsSome(mc.GetASPathList()), mc.GetASPathList().Value().IsValidASPathListEntry(regex_as, pos_as)) // check whether as path list is valid
                    );
        }

        /// <summary>
        /// Calculate the difference between 2 match clauses
        /// </summary>
        /// <param name="me1"> first match clause </param>
        /// <param name="me2"> second match clause </param>
        /// <returns> an integer denoting the difference between the clauses </returns>
        public static Zen<int> GetAttrDifference(this Zen<MatchClause> me1, Zen<MatchClause> me2){
            // initialize counter
            Zen<int> count = 0;

            // add difference obtained by comparing same types of attributes
            // if the attributes are not the same, then the distance is infinite
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

        /// <summary>
        ///  match an incoming route advertisement against a match clause
        /// </summary>
        /// <param name="m"> the match clause </param>
        /// <param name="ipa"> the route advertisement </param>
        /// <returns> pair containing the decision tree branch and boolean </returns>
        public static Zen<Pair<string, bool>> MatchAgainstClause(this Zen<MatchClause> m, Zen<IPAttr> ipa){
            var lp_clause = m.GetLP();
            var med_clause = m.GetMED();
            var pre_clause = m.GetPrefixList();
            var com_clause = m.GetCommunityList();
            var as_clause = m.GetASPathList();
            // if option.some then check else return true
            // none means empty clause so default pass through

            // compare local preference
            var lp_match = If<bool>(
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

            // compare metric
            var med_match = If<bool>(
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

            // compare prefix
            var prefix_match = If<bool>(
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


            // compare community values
            var community_match = If<bool>(
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

            // compare as path match
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
                    Utils.AndIf(lp_match, Utils.AndIf(med_match, Utils.AndIf(prefix_match, Utils.AndIf(community_match, expr5)))),
                    true,
                    false
                )
            );
        }
    }
}
