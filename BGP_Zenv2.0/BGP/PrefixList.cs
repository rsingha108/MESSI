using System;
using System.Collections.Generic;
using System.Numerics;
using ZenLib;
using System.Linq;
using System.IO;
using static ZenLib.Zen;
using System.ComponentModel;

namespace BGP{

    /// <summary>
    /// One entry in a prefix list
    /// </summary>
    public class PrefixListEntry{
        
        /// <summary>
        /// IPv4 prefix
        /// (we represent prefixes in their unsigned integer form. e.g. 100.10.1.1 is converted to 1678377217)
        /// </summary>
        public uint Prefix;

        /// <summary>
        /// Subnet mask
        /// (we represent masks in their unsigned integer form e.g, mask 24 is converted to 4294967040) 
        /// </summary>
        public uint Mask;

        /// <summary>
        /// Less than or equal to
        /// </summary>
        public Option<uint> LE;

        /// <summary>
        /// Greater than or equal to
        /// </summary>
        public Option<uint> GE;

        /// <summary>
        /// Permit or deny
        /// </summary>
        public bool Permit;

        /// <summary>
        /// Matches any prefix
        /// </summary>
        public bool Any;

        /// <summary>
        /// Create a Zen prefix list entry
        /// </summary>
        /// <param name="prefix"> IPv4 prefix </param>
        /// <param name="mask"> subnet mask </param>
        /// <param name="le"> less than or equal to </param>
        /// <param name="ge"> greater than or equal to </param>
        /// <param name="permit"> permit or deny </param>
        /// <param name="any"> matches any prefix </param>
        /// <returns> Zen prefix list entry </returns>
        public Zen<PrefixListEntry> Create(
            Zen<uint> prefix,
            Zen<uint> mask,
            Zen<Option<uint>> le,
            Zen<Option<uint>> ge,
            Zen<bool> permit,
            Zen<bool> any
        ){
            return Zen.Create<PrefixListEntry>(
                ("Prefix", prefix),
                ("Mask", mask),
                ("LE", le),
                ("GE", ge),
                ("Permit", permit),
                ("Any", any)
            );
        }

        /// <summary>
        /// Convert Zen prefix list entry to a string
        /// </summary>
        /// <returns> the string </returns>
        public override string ToString(){
            var pre1 = Prefix/(1<<24);
            var rem1 = Prefix%(1<<24);

            var pre2 = rem1 / (1 << 16);
            var rem2 = rem1 % (1 << 16);

            var pre3 = rem2 / (1 << 8);
            var pre4 = rem2 % (1 << 8);

            uint count = 0;
            var n = Mask;
            while(n > 0){
                n &= (n - 1);
                count++;
            }

            uint count2 = 0;
            n = LE.Value;
            while(n > 0){
                n &= (n - 1);
                count2++;
            }

            uint count3 = 0;
            n = GE.Value;
            while(n > 0){
                n &= (n - 1);
                count3++;
            }

            var le = LE.HasValue? "Some(" + count2.ToString() + ")": "None";
            var ge = GE.HasValue? "Some(" + count3.ToString() + ")": "None";

            return $"[Prefix: {pre1}.{pre2}.{pre3}.{pre4}/{count}, LE: {le}, GE: {ge}, Permit: {Permit}, Any: {Any}]";
        }
    }

    public static class PrefixListEntryExtensions{

        /// <summary>
        /// Get the IPv4 prefix
        /// </summary>
        /// <param name="ple"> the prefix list entry </param>
        /// <returns> the IPv4 prefix </returns>
        public static Zen<uint> GetPrefix(this Zen<PrefixListEntry> ple) => ple.GetField<PrefixListEntry, uint>("Prefix");

        /// <summary>
        /// Get the subnet mask
        /// </summary>
        /// <param name="ple"> the prefix list entry </param>
        /// <returns> the subnet mask </returns>
        public static Zen<uint> GetMask(this Zen<PrefixListEntry> ple) => ple.GetField<PrefixListEntry, uint>("Mask");

        /// <summary>
        /// Get the LE value
        /// </summary>
        /// <param name="ple"> the prefix list entry</param>
        /// <returns> the LE value </returns>
        public static Zen<Option<uint>> GetLE(this Zen<PrefixListEntry> ple) => ple.GetField<PrefixListEntry, Option<uint>>("LE");

        /// <summary>
        /// Get the GE value
        /// </summary>
        /// <param name="ple"> the prefix list entry</param>
        /// <returns> the GE value </returns>
        public static Zen<Option<uint>> GetGE(this Zen<PrefixListEntry> ple) => ple.GetField<PrefixListEntry, Option<uint>>("GE");

        /// <summary>
        /// Gets the permit value
        /// </summary>
        /// <param name="ple"> the prefix list entry </param>
        /// <returns> permit value </returns>
        public static Zen<bool> GetPermit(this Zen<PrefixListEntry> ple) => ple.GetField<PrefixListEntry, bool>("Permit");

        /// <summary>
        /// Gets the 'any' value
        /// </summary>
        /// <param name="ple"> the prefix list entry </param>
        /// <returns> 'any' value </returns>
        public static Zen<bool> GetAny(this Zen<PrefixListEntry> ple) => ple.GetField<PrefixListEntry, bool>("Any");

        private static Zen<bool> IsValidMaskLEGE(this Zen<uint> num){
            uint n = 0;
            Zen<bool> constraints = (num == n);
            for(int i=0;i<32;i++){
                n |= ((uint)1) << (31-i);
                constraints = Or(constraints, num == n);
            }

            return constraints;
        }
        
        /// <summary>
        /// Specifies the conditions for a valid prefix list entry
        /// </summary>
        /// <param name="ple"> the prefix list entry </param>
        /// <returns> true or false </returns>
        public static Zen<bool> IsValidPrefixListEntry(this Zen<PrefixListEntry> ple){
            IList<Zen<bool>> predicates = new List<Zen<bool>>();

            // The subnet mask can be 0-32
            predicates.Add(ple.GetMask().IsValidMaskLEGE());


            // The LE value can be 0-32
            var le = ple.GetLE();
            predicates.Add(
                Implies(
                    Option.IsSome(le),
                    le.Value().IsValidMaskLEGE()
                )
            );

            // The GE value can be 0-32
            var ge = ple.GetGE();
            predicates.Add(
                Implies(
                    Option.IsSome(ge),
                    ge.Value().IsValidMaskLEGE()
                )
            );

            // mask <= LE
            predicates.Add(
                Implies(
                    Option.IsSome(le),
                    le.Value() >= ple.GetMask()
                )
            );

            // mask <= GE
            predicates.Add(
                Implies(
                    Option.IsSome(ge),
                    ge.Value() >= ple.GetMask()
                )
            );

            // GE <= LE
            predicates.Add(
                Implies(
                    And(Option.IsSome(le), Option.IsSome(ge)),
                    ge.Value() <= le.Value()
                )
            );

            // Limit the range of allowed prefixes
            predicates.Add(
                Or(
                    ple.GetPrefix() == 0,
                    And(
                        ple.GetPrefix() >= 1671377732,
                        ple.GetPrefix() <= 1679687938
                    )
                )
            );


            // Prefix 0 is only allowed iff Any is true
            predicates.Add(
                And(
                    Implies(ple.GetAny(), ple.GetPrefix() == 0),
                    Implies(ple.GetPrefix() == 0, ple.GetAny())
                )
            );

            // If Any is true, then GE=0, LE=32 and mask=0
            predicates.Add(
                Implies(
                    ple.GetAny(),
                    And(
                        ple.GetPrefix() == 0,
                        ple.GetMask() == 0,
                        ple.GetLE().IsSome(),
                        ple.GetLE().Value() == 4294967295,
                        ple.GetGE().IsSome(),
                        ple.GetGE().Value() == 0
                    )
                )
            );


            // preventing some boundary conditions
            predicates.Add(
                And(
                    (ple.GetPrefix() & 4278190080) != 4278190080,  // first byte cannot equal 255
                    (ple.GetPrefix() & 4261412864) != 4261412864,  // first byte cannot equal 254
                    (ple.GetPrefix() & 16711680) != 16711680, // second byte cannot equal 255
                    (ple.GetPrefix() & 16646144) != 16646144, // second byte cannot equal 254
                    (ple.GetPrefix() & 65280) != 65280, // third byte cannot equal 255
                    (ple.GetPrefix() & 65024) != 65024, // third byte cannot equal 254
                    (ple.GetPrefix() & 255) != 255, // fourth byte cannot equal 255
                    (ple.GetPrefix() & 254) != 254 // fourth byte cannot equal 254 
                )
            );

            return predicates.Aggregate((a, b) => And(a, b));
        }

        /// <summary>
        /// Match the incoming route advertisement against the prefix list entry
        /// </summary>
        /// <param name="ple"> the prefix list entry </param>
        /// <param name="ipa"> the route advertisement </param>
        /// <returns> true or false </returns>
        public static Zen<bool> MatchAgainstPrefix(this Zen<PrefixListEntry> ple, Zen<IPAttr> ipa){
            // This is the actual logic
            // if ple.GetPrefix() and ple.GetMask() == ipa.GetPrefix() and ple.GetMask()
            //      if ge <= ipa.mask <= le
            //          return true
            //      else if no le and no ge
            //          if ipa.mask == ple.mask
            //              return true
            //      else return false
            // else return false

            
            var expr1 = If<bool>(
                    // checking for a prefix match
                    (ple.GetPrefix() & ple.GetMask()) == (ipa.GetPrefix() & ple.GetMask()),
                    true,
                    false
                );

            var expr4 = If<bool>(
                ple.GetMask() == ipa.GetMask(),
                expr1,
                false
            );

            // if issome(le)
            //     if issome(ge)
            //         if ge <= ip.mask <= le
            //             expr1
            //         else
            //             false    
            //     else
            //         if ple.mask <= ip.mask <= le
            //             expr1
            //         else
            //             false
            // else
            //     if issome(ge)
            //         if ip.mask >= ge
            //             expr1
            //         else
            //             false
            //     else
            //         expr4

            return If<bool>(
                // if any flag is set, then all prefixes match regardless
                ple.GetAny(),
                true,
                // otherwise, apply above logic
                If<bool>(
                    Option.IsSome(ple.GetLE()),
                    If<bool>(
                        Option.IsSome(ple.GetGE()),
                        If<bool>(
                            // if LE and GE are defined, check whether input route's subnet mask lies within that range
                            Utils.AndIf(
                                ple.GetGE().Value() <= ipa.GetMask(), 
                                ple.GetLE().Value() >= ipa.GetMask()
                            ),
                            expr1,
                            false
                        ),
                        If<bool>(
                            // if LE is defined, but GE is not defined, then check whether Prefix Length <= Route prefix length <= LE
                            Utils.AndIf(
                                ipa.GetMask() <= ple.GetLE().Value(),
                                ipa.GetMask() >= ple.GetMask()
                            ),
                            expr1,
                            false
                        )
                    ),
                    If<bool>(
                        // if GE is defined, but LE is not defined, then input route's subnet mask must be greater than GE
                        Option.IsSome(ple.GetGE()),
                        If<bool>(ipa.GetMask() >= ple.GetGE().Value(),
                            expr1,
                            false
                        ),
                        // otherwise prefix mask and input route-mask must be equal
                        expr4
                    )
                )
            );
        }
    }

    /// <summary>
    /// Prefix list class
    /// </summary>
    public class PrefixList{
        /// <summary>
        /// Array of prefix list entries
        /// </summary>
        public Array<PrefixListEntry, _3> Value {get; set;}

        /// <summary>
        /// Create a Zen prefix list
        /// </summary>
        /// <param name="value"> the array of prefix list entries </param>
        /// <returns> Zen prefix list </returns>
        public Zen<PrefixList> Create(Zen<Array<PrefixListEntry, _3>> value){
            return Zen.Create<PrefixList>(("Value", value));
        }

        /// <summary>
        /// Convert a Zen prefix list to string
        /// </summary>
        /// <returns> the string </returns>
        public override string ToString(){
            return string.Join("\n", Value);
        }
    }

    /// <summary>
    /// Prefix list extensions class
    /// </summary>
    public static class PrefixListExtensions{

        /// <summary>
        /// Gets all the prefix list entries
        /// </summary>
        /// <param name="plist"> the prefix list </param>
        /// <returns> the prefix list entries </returns>
        public static Zen<Array<PrefixListEntry, _3>> GetValue(this Zen<PrefixList> plist) => plist.GetField<PrefixList, Array<PrefixListEntry, _3>>("Value");

        /// <summary>
        /// Checks whether all prefix list entries are unique
        /// </summary>
        /// <param name="plist"> the prefix list </param>
        /// <returns> true or false </returns>
        public static Zen<bool> UniquePrefixes(this Zen<PrefixList> plist){
            Zen<bool> unique = true;
            var pr_arr = plist.GetValue().ToArray();
            var len = plist.GetValue().Length();

            for(int i=0;i<len;i++){
                for(int j=i+1;j<len;j++){
                    unique = And(
                        unique,
                        pr_arr[i] != pr_arr[j]
                    );
                }
            }

            return unique;
        }

        /// <summary>
        /// Checks whether the prefix list is valid
        /// </summary>
        /// <param name="plist"> the prefix list </param>
        /// <param name="num_prefixes"> the number of prefixes in the prefix list (1 or 3) </param>
        /// <returns> true or false </returns>
        public static Zen<bool> IsValidPrefixList(this Zen<PrefixList> plist, Zen<int> num_prefixes){
            var pr_arr = plist.GetValue().ToArray();
            var len = plist.GetValue().Length();

            Zen<bool> check_valid_prefixes = true;
            
            // all prefix list entries should be valid
            for(int i=0;i<len;i++){
                check_valid_prefixes = And(
                    check_valid_prefixes,
                    pr_arr[i].IsValidPrefixListEntry()
                );
            }

            Zen<bool> deny_any = true;
            for(int i=1;i<len;i++){
                deny_any = And(
                    deny_any,
                    Not(pr_arr[i].GetPermit()), pr_arr[i].GetAny()
                );
            }

            check_valid_prefixes = If(
                num_prefixes > 1,
                // if the number of prefix list entries is more than 1, then all of them should  be unique
                And(
                    check_valid_prefixes, 
                    plist.UniquePrefixes()
                ),
                // otherwise all entries (except the first one) in the prefix list must be deny any
                And(
                    check_valid_prefixes,
                    deny_any
                )
            );

            return check_valid_prefixes;
        }

        /// <summary>
        /// Gets the row-wise difference between 2 prefix lists 
        /// </summary>
        /// <param name="prl1"> first prefix list </param>
        /// <param name="prl2"> second prefix list </param>
        /// <returns> the difference </returns>
        public static Zen<int> GetRowwiseDifference(this Zen<PrefixList> prl1, Zen<PrefixList> prl2){
            var pre_arr1 = prl1.GetValue().ToArray();
            var pre_arr2 = prl2.GetValue().ToArray();

            var len = prl1.GetValue().Length();

            Zen<int> count = 0;
            for(int i=0;i<len;i++){
                count = If<int>(
                    Utils.AndIf(
                        pre_arr1[i].GetAny(),
                        pre_arr2[i].GetAny()
                    ),
                    If<int>(
                        pre_arr1[i].GetPermit() != pre_arr2[i].GetPermit(),
                        count + 1,
                        count
                    ),
                    If<int>(
                        Or(
                            pre_arr1[i].GetPrefix() != pre_arr2[i].GetPrefix(),
                            pre_arr1[i].GetMask() != pre_arr2[i].GetMask(),
                            pre_arr1[i].GetLE() != pre_arr2[i].GetLE(),
                            pre_arr1[i].GetGE() != pre_arr2[i].GetGE()
                        ),
                        If<int>(
                            pre_arr1[i].GetPermit() != pre_arr2[i].GetPermit(),
                            count + 2,
                            count + 1
                        ),
                        If<int>(
                            pre_arr1[i].GetPermit() != pre_arr2[i].GetPermit(),
                            count + 1,
                            count
                        )
                    )
                );
            }

            return count;
        }

        /// <summary>
        /// Gets the difference between 2 prefix lists
        /// </summary>
        /// <param name="prl1"> first prefix list </param>
        /// <param name="prl2"> second prefix list </param>
        /// <returns> the difference </returns>
        public static Zen<int> GetDifference(this Zen<Option<PrefixList>> prl1, Zen<Option<PrefixList>> prl2){
            Zen<int> count = 0;
            count = If<int>(
                prl1.IsNone(),
                If<int>(
                    prl2.IsNone(),
                    0,
                    Constants.INF
                ),
                If<int>(
                    prl2.IsNone(),
                    Constants.INF,
                    GetRowwiseDifference(prl1.Value(), prl2.Value())
                )
            );

            return count;
        }

        /// <summary>
        ///  Match the incoming route advertisement against the prefix list
        /// </summary>
        /// <param name="plist"> the prefix list </param>
        /// <param name="ipa"> the route advertisement </param>
        /// <returns> true or false </returns>
        public static Zen<bool> MatchAgainstPrefixList(this Zen<PrefixList> plist, Zen<IPAttr> ipa){          
            return If<bool>(
                plist.GetValue().Get(0).MatchAgainstPrefix(ipa),
                plist.GetValue().Get(0).GetPermit(),
                If<bool>(
                    plist.GetValue().Get(1).MatchAgainstPrefix(ipa),
                    plist.GetValue().Get(1).GetPermit(),
                    If<bool>(
                        plist.GetValue().Get(2).MatchAgainstPrefix(ipa),
                        plist.GetValue().Get(2).GetPermit(),
                        false
                    )
                )
            );
        }
    }
}