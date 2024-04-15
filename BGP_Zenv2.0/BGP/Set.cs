using System;
using System.Collections.Generic;
using ZenLib;
using static ZenLib.Zen;

namespace BGP{
    /// <summary>
    /// Set clause class
    /// </summary>
    public class SetClause{
        /// <summary>
        /// Local-preference
        /// </summary>
        public Option<uint> LP;

        /// <summary>
        /// Multi-exit discriminator
        /// </summary>
        public Option<uint> MED;

        /// <summary>
        /// Community to be added
        /// </summary>
        public Option<uint> Community;

        /// <summary>
        /// Delete community regex and corresponding positive examples
        /// </summary>
        public Option<Pair<string, Array<uint, _3>>> DeleteCommunity;

        /// <summary>
        /// Index of delete community regex (only used for writing constraints)
        /// </summary>
        public uint IndexCom;

        /// <summary>
        /// prepend AS number 
        /// </summary>
        public Option<uint> ASPathPrepend;

        /// <summary>
        /// delete AS number
        /// </summary>
        public Option<uint> ASPathExclude;

        /// <summary>
        /// Index of AS path regex used in route-map stanza (only used for writing constraints)
        /// </summary>
        public uint IndexAs;

        /// <summary>
        /// Next-hop IP
        /// </summary>
        public Option<uint> NextHopIP;

        /// <summary>
        /// next-hop-peer flag
        /// </summary>
        public bool NextHopPeer;

        /// <summary>
        /// next-hop-unchanged flag
        /// </summary>
        public bool NextHopUnchanged;

        public static string CommunityInt2Str(uint s){
            var a = s / (1 << 16);
            var b = s % (1 << 16);
            return $"{a}:{b}";
        }

        public static string NextHopIP2Str(uint ip){
            var nh1 = ip / (1<<24);
            var rem1 = ip % (1<<24);
            var nh2 = rem1 / (1 << 16);
            var rem2 = rem1 % (1 << 16);
            var nh3 = rem2 / (1 << 8);
            var nh4 = rem2 % (1 << 8);
            return $"{nh1}.{nh2}.{nh3}.{nh4}";
        }

        /// <summary>
        /// Create a Zen Set clause
        /// </summary>
        /// <param name="lp">local preference</param>
        /// <param name="med">multi-exit discriminator</param>
        /// <param name="community">community to add to route</param>
        /// <param name="delcomm">delete community regex</param>
        /// <param name="index_com">index of delete community regex</param>
        /// <param name="aspathprepend">AS path prepend value</param>
        /// <param name="aspathexclude">AS path delete value</param>
        /// <param name="index_as">index of as path regex</param>
        /// <param name="nhip">next-hop IP</param>
        /// <param name="nhpeer">next-hop-peer flag</param>
        /// <param name="nhunchanged">next-hop-unchanged flag</param>
        /// <returns>A Zen object</returns>
        public Zen<SetClause> Create(Zen<Option<uint>> lp, Zen<Option<uint>> med, Zen<Option<uint>> community, Zen<Option<Pair<string, Array<uint,_3>>>> delcomm, Zen<uint> index_com, Zen<Option<uint>> aspathprepend, Zen<Option<uint>> aspathexclude, Zen<uint> index_as, Zen<Option<uint>> nhip, Zen<bool> nhpeer, Zen<bool> nhunchanged){
            return Zen.Create<SetClause>(
                ("LP", lp), 
                ("MED", med),
                ("Community", community),
                ("DeleteCommunity", delcomm),
                ("IndexCom", index_com),
                ("ASPathPrepend", aspathprepend),
                ("ASPathExclude", aspathexclude),
                ("IndexAs", index_as),
                ("NextHopIP", nhip),
                ("NextHopPeer", nhpeer),
                ("NextHopUnchanged", nhunchanged));
        }

        /// <summary>
        /// Convert the Set clause to a string
        /// </summary>
        /// <returns>A string</returns>
        public override string ToString(){
            var c = Community.HasValue? CommunityInt2Str(Community.Value) : "None"; 
            var d = DeleteCommunity.HasValue? DeleteCommunity.Value.Item1 : "None";
            var nh = "None";
            if(NextHopPeer) nh = "Peer";
            else if(NextHopUnchanged) nh = "Unchanged";
            else if(NextHopIP.HasValue){
                var ip = NextHopIP.Value;
                var nh1 = ip / (1<<24);
                var rem1 = ip % (1<<24);
                var nh2 = rem1 / (1 << 16);
                var rem2 = rem1 % (1 << 16);
                var nh3 = rem2 / (1 << 8);
                var nh4 = rem2 % (1 << 8);

                nh = $"{nh1}.{nh2}.{nh3}.{nh4}";

            }
            return $"Local Preference {LP}, MED: {MED}, Community: {c}, Delete Community: {d}, AS Path Prepend: {ASPathPrepend}, AS Path Exclude: {ASPathExclude}, Next Hop: {nh}";
        }
    }

    /// <summary>
    /// Set clause extensions class
    /// </summary>
    public static class SetClauseExtensions{
        /// <summary>
        /// Retrieve the Local Preference value
        /// </summary>
        /// <param name="stc">the set clause</param>
        /// <returns>the local preference value</returns>
        public static Zen<Option<uint>> GetLP(this Zen<SetClause> stc) => stc.GetField<SetClause, Option<uint>>("LP");
        
        /// <summary>
        /// retrieve the multi-exit discriminator 
        /// </summary>
        /// <param name="stc">the set clause</param>
        /// <returns>the med value</returns>
        public static Zen<Option<uint>> GetMED(this Zen<SetClause> stc) => stc.GetField<SetClause, Option<uint>>("MED");
        
        /// <summary>
        /// get the community value to be added
        /// </summary>
        /// <param name="stc">the set clause</param>
        /// <returns>the community value</returns>
        public static Zen<Option<uint>> GetCommunity(this Zen<SetClause> stc) => stc.GetField<SetClause, Option<uint>>("Community");
        
        /// <summary>
        /// get the delete community regex and the set of corresponding positive examples
        /// </summary>
        /// <param name="stc">the set clause</param>
        /// <returns>the regex and examples</returns>
        public static Zen<Option<Pair<string, Array<uint, _3>>>> GetDeleteCommunity(this Zen<SetClause> stc) => stc.GetField<SetClause, Option<Pair<string, Array<uint, _3>>>>("DeleteCommunity");
        
        /// <summary>
        /// Get the index of the delete community regex (only used for writing constraints)
        /// </summary>
        /// <param name="stc">the set clause</param>
        /// <returns>the index</returns>
        public static Zen<uint> GetIndexCom(this Zen<SetClause> stc) => stc.GetField<SetClause, uint>("IndexCom");
        
        /// <summary>
        /// Get the AS path prepend value
        /// </summary>
        /// <param name="stc">the set clause</param>
        /// <returns>the AS path prepend value</returns>
        public static Zen<Option<uint>> GetASPathPrepend(this Zen<SetClause> stc) => stc.GetField<SetClause, Option<uint>>("ASPathPrepend");
        
        /// <summary>
        /// Get the AS path value to exclude
        /// </summary>
        /// <param name="stc">the set clause</param>
        /// <returns>the AS number to be deleted</returns>
        public static Zen<Option<uint>> GetASPathExclude(this Zen<SetClause> stc) => stc.GetField<SetClause, Option<uint>>("ASPathExclude");
        
        /// <summary>
        /// Get the index of the AS path regex (only used for writing constraints)
        /// </summary>
        /// <param name="stc">the set clause</param>
        /// <returns>the index</returns>
        public static Zen<uint> GetIndexAs(this Zen<SetClause> stc) => stc.GetField<SetClause, uint>("IndexAs");
        
        /// <summary>
        /// Get the next-hop IP address
        /// </summary>
        /// <param name="stc">the set clause</param>
        /// <returns>the next-hop IP address</returns>
        public static Zen<Option<uint>> GetNextHopIP(this Zen<SetClause> stc) => stc.GetField<SetClause, Option<uint>>("NextHopIP");
        
        /// <summary>
        /// Get the next-hop-peer flag
        /// </summary>
        /// <param name="stc">the set clause</param>
        /// <returns>a boolean</returns>
        public static Zen<bool> GetNextHopPeer(this Zen<SetClause> stc) => stc.GetField<SetClause, bool>("NextHopPeer");
        
        /// <summary>
        /// Get the next-hop-unchanged flag
        /// </summary>
        /// <param name="stc">the set clause</param>
        /// <returns>a boolean</returns>
        public static Zen<bool> GetNextHopUnchanged(this Zen<SetClause> stc) => stc.GetField<SetClause, bool>("NextHopUnchanged");

        /// <summary>
        /// Remove a community value from the input route
        /// </summary>
        /// <param name="stc">the set clause</param>
        /// <param name="ipComList">the set of communities present in the iput route</param>
        /// <returns>the new set of communities after deletion</returns>
        private static Zen<FSeq<uint>> CheckDeleteCommunity(this Zen<SetClause> stc, Zen<FSeq<uint>> ipComList){
            var delcom = stc.GetDeleteCommunity().Value().Item2();
            var arrayExpr = delcom.ToArray();

            for(int i=0;i<delcom.Length();i++){
                ipComList = If<FSeq<uint>>(
                    ipComList.Contains(arrayExpr[i]),
                    ipComList.RemoveAll(arrayExpr[i]),
                    ipComList
                );
            }

            return ipComList;
        }

        /// <summary>
        /// Check whether the set clause is well-formed
        /// </summary>
        /// <param name="stc">the set clause</param>
        /// <param name="delcom_regex">the list of allowed delete community regexes</param>
        /// <param name="delete_com_list">the list of positive examples corresponding to each delete community regex</param>
        /// <param name="aspathprepend">allowed AS numbers that can be prepended to the AS path of the input route</param>
        /// <param name="aspathexclude">allowed AS numbers that can be deleted from the input route</param>
        /// <returns>a boolean</returns>
        public static Zen<bool> IsValidSetClause(this Zen<SetClause> stc, string[] delcom_regex, List<Array<uint, _3>> delete_com_list, List<Array<uint, _3>> aspathprepend, List<Array<uint, _3>> aspathexclude){
            // check whether the delete community value is among the ones that are allowed
            var res = False();
            for(int i=0;i<delete_com_list.Count;i++){
                res = Utils.OrIf(
                    res,
                    Implies(
                        Option.IsSome(stc.GetDeleteCommunity()),
                        Utils.AndIf(
                            stc.GetIndexCom() == (uint)i,
                            Utils.AndIf(
                                stc.GetDeleteCommunity().Value().Item1() == delcom_regex[i],
                                stc.GetDeleteCommunity().Value().Item2() == delete_com_list[i]
                            )
                        )
                    )
                );
            }

            // check whether the AS path prepend values is among the ones that are allowed
            var res2 = False();
            for(int i=0;i<aspathprepend.Count;i++){
                res2 = Utils.OrIf(
                    res2,
                    Implies(
                        Option.IsSome(stc.GetASPathPrepend()),
                        Utils.AndIf(
                            stc.GetIndexAs() == (uint)i,
                            Utils.OrIf(
                                stc.GetASPathPrepend().Value() == aspathprepend[i].Get(0),
                                Utils.OrIf(
                                    stc.GetASPathPrepend().Value() == aspathprepend[i].Get(1),
                                    stc.GetASPathPrepend().Value() == aspathprepend[i].Get(2)
                                )
                            )
                        )
                    )
                );
            }
            
            // check whether the AS path exclude values is among the ones that are provided
            var res3 = False();
            for(int i=0;i<aspathexclude.Count;i++){
                res3 = Utils.OrIf(
                    res3,
                    Implies(
                        Option.IsSome(stc.GetASPathExclude()),
                        Utils.AndIf(
                            stc.GetIndexAs() == (uint)i,
                            Utils.OrIf(
                                stc.GetASPathExclude().Value() == aspathexclude[i].Get(0),
                                Utils.OrIf(
                                    stc.GetASPathExclude().Value() == aspathexclude[i].Get(1),
                                    stc.GetASPathExclude().Value() == aspathexclude[i].Get(2)
                                )
                            )
                        )
                    )
                );
            }
            

            return And(
                Implies(Option.IsSome(stc.GetLP()), And(stc.GetLP().Value() >= 100, stc.GetLP().Value() <= 900)),
                Implies(Option.IsSome(stc.GetMED()), And(stc.GetMED().Value() >= 0, stc.GetLP().Value() <= 800)),
                Not(And(                            // all set actions cannot be None at the same time
                        Option.IsNone(stc.GetLP()),
                        Option.IsNone(stc.GetMED()),
                        Option.IsNone(stc.GetCommunity()),
                        Option.IsNone(stc.GetDeleteCommunity())
                    )
                ),
                Implies(Option.IsSome(stc.GetCommunity()), Option.IsNone(stc.GetDeleteCommunity())), // cannot have both delete community and add community
                Implies(Option.IsSome(stc.GetDeleteCommunity()), Option.IsNone(stc.GetCommunity())), // cannot have both delete community and add community
                Implies(Option.IsSome(stc.GetCommunity()), stc.GetCommunity().Value() > 0),
                Implies(Option.IsSome(stc.GetASPathPrepend()), Option.IsNone(stc.GetASPathExclude())), // cannot have both AS path prepend and AS path exclude
                Implies(Option.IsSome(stc.GetASPathExclude()), Option.IsNone(stc.GetASPathPrepend())), // cannot have both AS path prepend and AS path exclude
                Implies(Option.IsSome(stc.GetASPathPrepend()), stc.GetASPathPrepend().Value() > 0),
                Implies(Option.IsSome(stc.GetASPathExclude()), stc.GetASPathExclude().Value() > 0),
                Implies(Option.IsSome(stc.GetNextHopIP()), And(Not(stc.GetNextHopUnchanged()), Not(stc.GetNextHopPeer()))), // only one of these set actions is allowed at a time
                Implies(stc.GetNextHopUnchanged(), And(Option.IsNone(stc.GetNextHopIP()), Not(stc.GetNextHopPeer()))),
                Implies(stc.GetNextHopPeer(), And(Not(stc.GetNextHopUnchanged()), Not(stc.GetNextHopPeer()))),
                Implies(Option.IsSome(stc.GetNextHopIP()), And(stc.GetNextHopIP().Value() >= 1631377732, stc.GetNextHopIP().Value() <= 1639687938)), // constrain the set of next-hop IP addresses
                res,
                res2,
                res3
            );
        }

        /// <summary>
        /// sets the attributes of an input route
        /// </summary>
        /// <param name="stc">the set clause</param>
        /// <param name="ipa">the input route</param>
        /// <returns>the transformed route</returns>
        public static Zen<Pair<string, Option<IPAttr>>> SetAttr(this Zen<SetClause> stc, Zen<IPAttr> ipa){
            // set the local preference value
            var new_lp =  If(
                    Option.IsSome(stc.GetLP()),
                    stc.GetLP().Value(),
                    ipa.GetLP()
                );

            var s = If<string>(
                Option.IsSome(stc.GetLP()),
                "L3",
                ""
            );

            // set the multi-exit discriminator
            var new_med = If(
                    Option.IsSome(stc.GetMED()),
                    stc.GetMED().Value(),
                    ipa.GetMED()
                );

            s = If(
                Option.IsSome(stc.GetMED()),
                s + "M3",
                s + ""
            );

            // set the community attribute in the input route
            var new_com_list = If(
                Option.IsSome(stc.GetCommunity()),
                ipa.GetCommunityAsList().AddBack(stc.GetCommunity().Value()),
                If(
                    Option.IsSome(stc.GetDeleteCommunity()),
                    stc.CheckDeleteCommunity(ipa.GetCommunityAsList()),
                    ipa.GetCommunityAsList()
                )
            );

            s = If(
                Option.IsSome(stc.GetCommunity()),
                s + "C3",
                If(
                    Option.IsSome(stc.GetDeleteCommunity()),
                    s + "C4",
                    s
                )
            );

            // set the AS path in the input route
            var new_as_path_list = If(
                Option.IsSome(stc.GetASPathPrepend()),
                ipa.GetASPathAsList().AddFront(stc.GetASPathPrepend().Value()),
                If(
                    Utils.AndIf(Option.IsSome(stc.GetASPathExclude()),ipa.GetASPathAsList().Contains(stc.GetASPathExclude().Value())),
                    ipa.GetASPathAsList().RemoveAll(stc.GetASPathExclude().Value()),
                    ipa.GetASPathAsList()
                )
            );

            s = If(
                Option.IsSome(stc.GetASPathPrepend()),
                s + "A3",
                If(
                    Option.IsSome(stc.GetASPathExclude()),
                    s + "A4",
                    s
                )
            );


            // set the next-hop IP in the input route
            var new_nh = If<uint>(
                Option.IsSome(stc.GetNextHopIP()),
                stc.GetNextHopIP().Value(),
                If<uint>(
                    stc.GetNextHopPeer(),
                    0, // this value is set to 0, but the testing setup changes it to the IP addres of the peer
                    If<uint>(
                        stc.GetNextHopUnchanged(),
                        ipa.GetNextHop(),
                        ipa.GetNextHop()
                    )
                )
            );

            s = If(
                Option.IsSome(stc.GetNextHopIP()),
                s + "H3",
                If(
                    stc.GetNextHopPeer(),
                    s + "H4",
                    If(
                        stc.GetNextHopUnchanged(),
                        s + "H5",
                        s + ""
                    )
                )
            );

            return Pair.Create<string, Option<IPAttr>>(
                s,
                Option.Create(IPAttr.Create(
                ipa.GetPrefix(),
                ipa.GetMask(),
                new_lp,
                new_med,
                ipa.GetIndex(),
                new_com_list, 
                ipa.GetIndexAS(),
                new_as_path_list,
                new_nh
            )));
          }
    }
}