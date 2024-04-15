using System;
using System.Collections.Generic;
using ZenLib;
using static ZenLib.Zen;

namespace BGP{
    /// <summary>
    /// one entry in AS path list
    /// </summary>
    public class ASPathListEntry{
        /// <summary>
        /// the index of the AS path regex (only used for writing constraints)
        /// </summary>
        public uint Index;

        /// <summary>
        /// the AS path regular expression
        /// </summary>
        public string RegularExpression;

        /// <summary>
        /// the set of positive examples corresponding to the regex
        /// </summary>
        public Array<FSeq<uint>, _3> PositiveExamples;

        /// <summary>
        /// permit/deny flag
        /// </summary>
        public bool Permit;

        /// <summary>
        /// Create a Zen AS path list entry
        /// </summary>
        /// <param name="index">the index of the AS path regular expression</param>
        /// <param name="regex">the AS path regular expression</param>
        /// <param name="pos">the set of positive examples corresponding to the regex</param>
        /// <param name="permit">the permit/deny flag</param>
        /// <returns>a Zen oject</returns>
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

        /// <summary>
        /// Convert the as path list entry to a string
        /// </summary>
        /// <returns>a string</returns>
        public override string ToString(){
            return $"{RegularExpression}, Permit: {Permit}";
        }
    }

    public static class ASPathListEntryExtensions{
        /// <summary>
        /// Get the index of the AS path regex
        /// </summary>
        /// <param name="ase">the AS path list entry</param>
        /// <returns><the index/returns>
        public static Zen<uint> GetIndex(this Zen<ASPathListEntry> ase) => ase.GetField<ASPathListEntry, uint>("Index");
        
        /// <summary>
        /// Get the AS path regex string
        /// </summary>
        /// <param name="ase">the AS path list entry</param>
        /// <returns>the regex string</returns>
        public static Zen<string> GetRegex(this Zen<ASPathListEntry> ase) => ase.GetField<ASPathListEntry, string>("RegularExpression");
        
        /// <summary>
        /// Retrieve the set of positive examples corresponding to the AS path regex
        /// </summary>
        /// <param name="ase">the AS path list entry</param>
        /// <returns>the list of positive examples</returns>
        public static Zen<Array<FSeq<uint>, _3>> GetPosEg(this Zen<ASPathListEntry> ase) => ase.GetField<ASPathListEntry, Array<FSeq<uint>, _3>>("PositiveExamples");
        
        /// <summary>
        /// Get the permit/deny flag
        /// </summary>
        /// <param name="ase"><the AS path list entry/param>
        /// <returns>a boolean</returns>
        public static Zen<bool> GetPermit(this Zen<ASPathListEntry> ase) => ase.GetField<ASPathListEntry, bool>("Permit");
        
        /// <summary>
        /// Check whether the AS path list entry is valid
        /// </summary>
        /// <param name="ase">the AS path list entry</param>
        /// <param name="regex">the set of allowed regexes</param>
        /// <param name="pos">the set of positive examples corresponding to each regex</param>
        /// <returns>a boolean</returns>
        public static Zen<bool> IsValidASPathListEntry(this Zen<ASPathListEntry> ase, string[] regex, List<Array<FSeq<uint>, _3>> pos){
            var res = False();
            for(int i=0;i<regex.Length;i++){
                res = Utils.OrIf(
                    res,
                    If<bool>(
                        ase.GetIndex() == (uint)i,
                        Utils.AndIf(
                            ase.GetRegex() == regex[i],
                            ase.GetPosEg() == pos[i]
                        ),
                        false
                    )
                );
            }

            return res;
        }


        /// <summary>
        /// Get the distance between two AS path list entries
        /// </summary>
        /// <param name="asp1">the first AS path list entry</param>
        /// <param name="asp2">the second AS path list entry</param>
        /// <returns>an integer</returns>
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

        /// <summary>
        /// Checks whether the iput route matches the AS path regex
        /// </summary>
        /// <param name="ase">the AS path list entry</param>
        /// <param name="ipa">the input route</param>
        /// <returns>a boolean</returns>
        public static Zen<bool> MatchAgainstASPathListEntry(this Zen<ASPathListEntry> ase, Zen<IPAttr> ipa){
            var pos = ase.GetPosEg();
            return If<bool>(
                pos.Any(p => p == ipa.GetASPathAsList()),
                ase.GetPermit(),
                false
            );
        }
    }
}