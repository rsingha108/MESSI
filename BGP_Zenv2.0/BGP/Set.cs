using System;
using System.Collections.Generic;
using ZenLib;
using static ZenLib.Zen;

namespace BGP{
    public class SetClause{
        public Option<uint> LP;
        public Option<uint> MED;
        public Option<uint> Community; // Community to add
        public Option<Pair<string, Array<uint, _3>>> DeleteCommunity;
        public uint Index;
        public Option<uint> ASPathPrepend;
        public Option<uint> ASPathExclude;
        public uint IndexAs;
        public Option<uint> NextHopIP;
        public bool NextHopPeer;
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

        public Zen<SetClause> Create(Zen<Option<uint>> lp, Zen<Option<uint>> med, Zen<Option<uint>> community, Zen<Option<Pair<string, Array<uint,_3>>>> delcomm, Zen<uint> index, Zen<Option<uint>> aspathprepend, Zen<Option<uint>> aspathexclude, Zen<uint> index_as, Zen<Option<uint>> nhip, Zen<bool> nhpeer, Zen<bool> nhunchanged){
            return Zen.Create<SetClause>(
                ("LP", lp), 
                ("MED", med),
                ("Community", community),
                ("DeleteCommunity", delcomm),
                ("Index", index),
                ("ASPathPrepend", aspathprepend),
                ("ASPathExclude", aspathexclude),
                ("IndexAs", index_as),
                ("NextHopIP", nhip),
                ("NextHopPeer", nhpeer),
                ("NextHopUnchanged", nhunchanged));
        }

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

    public static class SetClauseExtensions{
        public static Zen<Option<uint>> GetLP(this Zen<SetClause> stc) => stc.GetField<SetClause, Option<uint>>("LP");
        public static Zen<Option<uint>> GetMED(this Zen<SetClause> stc) => stc.GetField<SetClause, Option<uint>>("MED");
        public static Zen<Option<uint>> GetCommunity(this Zen<SetClause> stc) => stc.GetField<SetClause, Option<uint>>("Community");
        public static Zen<Option<Pair<string, Array<uint, _3>>>> GetDeleteCommunity(this Zen<SetClause> stc) => stc.GetField<SetClause, Option<Pair<string, Array<uint, _3>>>>("DeleteCommunity");
        public static Zen<uint> GetIndex(this Zen<SetClause> stc) => stc.GetField<SetClause, uint>("Index");
        public static Zen<Option<uint>> GetASPathPrepend(this Zen<SetClause> stc) => stc.GetField<SetClause, Option<uint>>("ASPathPrepend");
        public static Zen<Option<uint>> GetASPathExclude(this Zen<SetClause> stc) => stc.GetField<SetClause, Option<uint>>("ASPathExclude");
        public static Zen<uint> GetIndexAs(this Zen<SetClause> stc) => stc.GetField<SetClause, uint>("IndexAs");
        public static Zen<Option<uint>> GetNextHopIP(this Zen<SetClause> stc) => stc.GetField<SetClause, Option<uint>>("NextHopIP");
        public static Zen<bool> GetNextHopPeer(this Zen<SetClause> stc) => stc.GetField<SetClause, bool>("NextHopPeer");
        public static Zen<bool> GetNextHopUnchanged(this Zen<SetClause> stc) => stc.GetField<SetClause, bool>("NextHopUnchanged");

        public static Zen<FSeq<uint>> CheckDeleteCommunity(this Zen<SetClause> stc, Zen<FSeq<uint>> ipComList){
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

        public static Zen<bool> IsValidSetClause(this Zen<SetClause> stc, string[] delcom_regex, List<Array<uint, _3>> delete_com_list, List<Array<uint, _3>> aspathprepend, List<Array<uint, _3>> aspathexclude){
            // string regex = "(0|[1-9][0-9]{0,3}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])"; // all numbers from 0-65535 accecpted
            // regex = "(" + regex + ":" + regex + ")"; // single communities of the form AA:NN
            // regex = "^(" + regex + " ){0,1}" + regex + "$";
            // Regex<char> r1 = Regex.Parse(regex);

            var res = False();
            for(int i=0;i<delete_com_list.Count;i++){
                res = Utils.OrIf(
                    res,
                    Implies(
                        Option.IsSome(stc.GetDeleteCommunity()),
                        Utils.AndIf(
                            stc.GetIndex() == (uint)i,
                            Utils.AndIf(
                                stc.GetDeleteCommunity().Value().Item1() == delcom_regex[i],
                                stc.GetDeleteCommunity().Value().Item2() == delete_com_list[i]
                            )
                        )
                    )
                );
            }

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
                //Implies(Option.IsSome(stc.GetCommunity()), stc.GetCommunity().Value().MatchesRegex(r1)),
                Not(And(
                        Option.IsNone(stc.GetLP()),
                        Option.IsNone(stc.GetMED()),
                        Option.IsNone(stc.GetCommunity()),
                        Option.IsNone(stc.GetDeleteCommunity())
                    )
                ),
                Implies(Option.IsSome(stc.GetCommunity()), Option.IsNone(stc.GetDeleteCommunity())),
                Implies(Option.IsSome(stc.GetCommunity()), stc.GetCommunity().Value() > 0),
                Implies(Option.IsSome(stc.GetDeleteCommunity()), Option.IsNone(stc.GetCommunity())),
                Implies(Option.IsSome(stc.GetASPathPrepend()), Option.IsNone(stc.GetASPathExclude())),
                Implies(Option.IsSome(stc.GetASPathExclude()), Option.IsNone(stc.GetASPathPrepend())),
                Implies(Option.IsSome(stc.GetASPathPrepend()), stc.GetASPathPrepend().Value() > 0),
                Implies(Option.IsSome(stc.GetASPathExclude()), stc.GetASPathExclude().Value() > 0),
                Implies(Option.IsSome(stc.GetNextHopIP()), And(Not(stc.GetNextHopUnchanged()), Not(stc.GetNextHopPeer()))),
                Implies(stc.GetNextHopUnchanged(), And(Option.IsNone(stc.GetNextHopIP()), Not(stc.GetNextHopPeer()))),
                Implies(stc.GetNextHopPeer(), And(Not(stc.GetNextHopUnchanged()), Not(stc.GetNextHopPeer()))),
                Implies(Option.IsSome(stc.GetNextHopIP()), And(stc.GetNextHopIP().Value() >= 1631377732, stc.GetNextHopIP().Value() <= 1639687938)),
                res,
                res2,
                res3
            );
        }
        public static Zen<Pair<string, Option<IPAttr>>> SetLP(this Zen<SetClause> stc, Zen<IPAttr> ipa){
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

            var new_nh = If<uint>(
                Option.IsSome(stc.GetNextHopIP()),
                stc.GetNextHopIP().Value(),
                If<uint>(
                    stc.GetNextHopPeer(),
                    0,
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