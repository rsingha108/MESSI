using System;
using System.Collections.Generic;
using ZenLib;
using static ZenLib.Zen;

namespace BGP{
    /// <summary>
    /// one entry in a community list
    /// </summary>
    public class CommunityListEntry{

        /// <summary>
        /// index of the community regex (only used for writing constraints)
        /// </summary>
        public uint Index;

        /// <summary>
        /// the community regular expression
        /// </summary>
        public string RegularExpression;

        /// <summary>
        /// the positive examples corresponding to the regular expression
        /// </summary>
        public Array<FSeq<uint>, _3> PositiveExamples;

        /// <summary>
        /// permit/deny flag
        /// </summary>
        public bool Permit;

        /// <summary>
        /// Create a Zen community list entry
        /// </summary>
        /// <param name="index">index of the community regex</param>
        /// <param name="regex">the community regular expression</param>
        /// <param name="pos">set of positive examples</param>
        /// <param name="permit">permit/deny flag</param>
        /// <returns>a Zen object</returns>
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

        /// <summary>
        /// Convert community list entry to string
        /// </summary>
        /// <returns>a string</returns>
        public override string ToString(){
            return $"{RegularExpression}, Permit: {Permit}";
        }
    }
    
    /// <summary>
    /// Community list entry extensions class
    /// </summary>
    public static class CommunityListEntryExtensions{
        /// <summary>
        /// Get the index of the community regex
        /// </summary>
        /// <param name="cle">the community list entry</param>
        /// <returns>the index</returns>
        public static Zen<uint> GetIndex(this Zen<CommunityListEntry> cle) => cle.GetField<CommunityListEntry, uint>("Index");

        /// <summary>
        /// Get the community regex string
        /// </summary>
        /// <param name="cle">the community list entry</param>
        /// <returns>the community regex string</returns>
        public static Zen<string> GetRegex(this Zen<CommunityListEntry> cle) => cle.GetField<CommunityListEntry, string>("RegularExpression");
        
        /// <summary>
        /// Get the set of positive examples corresponding to the community regex
        /// </summary>
        /// <param name="cle">the community list entry</param>
        /// <returns>the list of positive examples</returns>
        public static Zen<Array<FSeq<uint>, _3>> GetPosEg(this Zen<CommunityListEntry> cle) => cle.GetField<CommunityListEntry, Array<FSeq<uint>, _3>>("PositiveExamples");
        
        /// <summary>
        /// Get the permit/deny flag
        /// </summary>
        /// <param name="cle">the community list entry</param>
        /// <returns>a boolean</returns>
        public static Zen<bool> GetPermit(this Zen<CommunityListEntry> cle) => cle.GetField<CommunityListEntry, bool>("Permit");
        
        /// <summary>
        /// Check whether the community list entry is valid
        /// </summary>
        /// <param name="cle">the community list entry</param>
        /// <param name="regex">the set of allowed regexes</param>
        /// <param name="pos">the list of allowed positive examples corresponding to each regex</param>
        /// <returns>A boolean</returns>
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
        
        /// <summary>
        /// Get the distance between two community list entries
        /// </summary>
        /// <param name="cle1">the first community list entry</param>
        /// <param name="cle2">the second community list entry</param>
        /// <returns>an integer</returns>
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
        
        /// <summary>
        /// Checks whether the input route matches the community regex
        /// </summary>
        /// <param name="cle">the community list entry</param>
        /// <param name="ipa">the input route</param>
        /// <returns>a boolean</returns>
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