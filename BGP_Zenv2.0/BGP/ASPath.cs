using System;
using System.Collections.Generic;
using ZenLib;
using static ZenLib.Zen;

namespace BGP{
    public class ASPathListEntry{
        public uint Index;
        public string RegularExpression;
        public Array<FSeq<uint>, _3> PositiveExamples;
        public bool Permit;

        public static Zen<ASPathListEntry> Create(
            Zen<uint> index,
            Zen<string> regex,
            Zen<Array<FSeq<uint>, _3>> pos,
            Zen<bool> permit
        ){
            return Zen.Create<ASPathListEntry>(
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

    public static class ASPathListEntryExtensions{
        public static Zen<uint> GetIndex(this Zen<ASPathListEntry> cle) => cle.GetField<ASPathListEntry, uint>("Index");
        public static Zen<string> GetRegex(this Zen<ASPathListEntry> cle) => cle.GetField<ASPathListEntry, string>("RegularExpression");
        public static Zen<Array<FSeq<uint>, _3>> GetPosEg(this Zen<ASPathListEntry> cle) => cle.GetField<ASPathListEntry, Array<FSeq<uint>, _3>>("PositiveExamples");
        public static Zen<bool> GetPermit(this Zen<ASPathListEntry> cle) => cle.GetField<ASPathListEntry, bool>("Permit");
        public static Zen<bool> IsValidASPathListEntry(this Zen<ASPathListEntry> cle, string[] regex, List<Array<FSeq<uint>, _3>> pos){
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

        public static Zen<int> GetDifference(Zen<Option<ASPathListEntry>> asp1, Zen<Option<ASPathListEntry>> asp2){
            Zen<int> count;
            count = If<int>(
                asp1.IsNone(),
                If<int>(
                    asp2.IsNone(),
                    0,
                    Constants.INF
                ),
                If<int>(
                    asp2.IsNone(),
                    Constants.INF,
                    If<int>(
                        asp1.Value().GetRegex() != asp2.Value().GetRegex(),
                        If<int>(
                            asp1.Value().GetPermit() != asp2.Value().GetPermit(),
                            2,
                            1
                        ),
                        If<int>(
                            asp1.Value().GetPermit() != asp2.Value().GetPermit(),
                            1,
                            0
                        )
                    )
                )
            );
            
            return count;
        }

        public static Zen<bool> MatchAgainstASPathListEntry(this Zen<ASPathListEntry> cle, Zen<IPAttr> ipa){
            var pos = cle.GetPosEg();
            return If<bool>(
                pos.Any(p => p == ipa.GetASPathAsList()),
                cle.GetPermit(),
                false
            );
        }
    }
}