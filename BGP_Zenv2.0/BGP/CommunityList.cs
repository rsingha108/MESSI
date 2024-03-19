using System;
using System.Collections.Generic;
using ZenLib;
using static ZenLib.Zen;

namespace BGP{
    public class CommunityListEntry{
        public uint Index;
        public string RegularExpression;
        public Array<FSeq<uint>, _3> PositiveExamples;
        public bool Permit;

        public static Zen<CommunityListEntry> Create(
            Zen<uint> index,
            Zen<string> regex,
            Zen<Array<FSeq<uint>, _3>> pos,
            Zen<bool> permit
        ){
            return Zen.Create<CommunityListEntry>(
                ("Index", index),
                ("RegularExpression", regex),
                ("PositiveExamples", pos),
                ("Permit", permit)
            );
        }

        public override string ToString(){
            return $"{RegularExpression}, Permit: {Permit}";
        }
    }

    public static class CommunityListEntryExtensions{
        public static Zen<uint> GetIndex(this Zen<CommunityListEntry> cle) => cle.GetField<CommunityListEntry, uint>("Index");
        public static Zen<string> GetRegex(this Zen<CommunityListEntry> cle) => cle.GetField<CommunityListEntry, string>("RegularExpression");
        public static Zen<Array<FSeq<uint>, _3>> GetPosEg(this Zen<CommunityListEntry> cle) => cle.GetField<CommunityListEntry, Array<FSeq<uint>, _3>>("PositiveExamples");
        public static Zen<bool> GetPermit(this Zen<CommunityListEntry> cle) => cle.GetField<CommunityListEntry, bool>("Permit");
        public static Zen<bool> IsValidCommunityListEntry(this Zen<CommunityListEntry> cle, string[] regex, List<Array<FSeq<uint>, _3>> pos){
            var res = False();
            for(int i=0;i<regex.Length;i++){
                res = Utils.OrIf(
                    res,
                    If<bool>(
                        cle.GetIndex() == (uint)i,
                        Utils.AndIf(
                            cle.GetRegex() == regex[i],
                            cle.GetPosEg() == pos[i]
                        ),
                        false
                    )
                );
            }

            return res;
        }

        public static Zen<int> GetDifference(this Zen<Option<CommunityListEntry>> cle1, Zen<Option<CommunityListEntry>> cle2){
            Zen<int> count;
            count = If<int>(
                cle1.IsNone(),
                If<int>(
                    cle2.IsNone(),
                    0,
                    Constants.INF
                ),
                If<int>(
                    cle2.IsNone(),
                    Constants.INF,
                    If<int>(
                        cle1.Value().GetRegex() != cle2.Value().GetRegex(),
                        If<int>(
                            cle1.Value().GetPermit() != cle2.Value().GetPermit(),
                            2,
                            1
                        ),
                        If<int>(
                            cle1.Value().GetPermit() != cle2.Value().GetPermit(),
                            1,
                            0
                        )
                    )
                )
            );
            return count;
        }
        
        public static Zen<bool> MatchAgainstCommunityListEntry(this Zen<CommunityListEntry> cle, Zen<IPAttr> ipa){
            var pos = cle.GetPosEg();
            return If<bool>(
                pos.Any(p => p == ipa.GetCommunityAsList()),
                cle.GetPermit(),
                false
            );
        }
    }
}