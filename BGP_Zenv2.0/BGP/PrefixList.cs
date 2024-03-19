using System;
using System.Collections.Generic;
using System.Numerics;
using ZenLib;
using System.Linq;
using System.IO;
using static ZenLib.Zen;

namespace BGP{
    public class PrefixListEntry{
        public uint Prefix;
        public uint Mask;
        public Option<uint> LE;
        public Option<uint> GE;
        public bool Permit;
        public bool Any;

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
        public static Zen<uint> GetPrefix(this Zen<PrefixListEntry> ple) => ple.GetField<PrefixListEntry, uint>("Prefix");
        public static Zen<uint> GetMask(this Zen<PrefixListEntry> ple) => ple.GetField<PrefixListEntry, uint>("Mask");
        public static Zen<Option<uint>> GetLE(this Zen<PrefixListEntry> ple) => ple.GetField<PrefixListEntry, Option<uint>>("LE");
        public static Zen<Option<uint>> GetGE(this Zen<PrefixListEntry> ple) => ple.GetField<PrefixListEntry, Option<uint>>("GE");
        public static Zen<bool> GetPermit(this Zen<PrefixListEntry> ple) => ple.GetField<PrefixListEntry, bool>("Permit");
        public static Zen<bool> GetAny(this Zen<PrefixListEntry> ple) => ple.GetField<PrefixListEntry, bool>("Any");
        public static Zen<bool> IsValidPrefixListEntry(this Zen<PrefixListEntry> ple){
            IList<Zen<bool>> predicates = new List<Zen<bool>>();
            predicates.Add(
                Or(
                    ple.GetMask() == 0,            // 0
                    ple.GetMask() == 2147483648,   // 1
                    ple.GetMask() == 3221225472,   // 2
                    ple.GetMask() == 3758096384,   // 3
                    ple.GetMask() == 4026531840,   // 4
                    ple.GetMask() == 4160749568,   // 5
                    ple.GetMask() == 4227858432,   // 6
                    ple.GetMask() == 4261412864,   // 7
                    ple.GetMask() == 4278190080,   // 8
                    ple.GetMask() == 4286578688,   // 9
                    ple.GetMask() == 4290772992,   // 10
                    ple.GetMask() == 4292870144,   // 11
                    ple.GetMask() == 4293918720,   // 12
                    ple.GetMask() == 4294443008,   // 13
                    ple.GetMask() == 4294705152,   // 14
                    ple.GetMask() == 4294836224,   // 15
                    ple.GetMask() == 4294901760,   // 16
                    ple.GetMask() == 4294934528,   // 17
                    ple.GetMask() == 4294950912,   // 18
                    ple.GetMask() == 4294959104,   // 19
                    ple.GetMask() == 4294963200,   // 20
                    ple.GetMask() == 4294965248,   // 21
                    ple.GetMask() == 4294966272,   // 22
                    ple.GetMask() == 4294966784,   // 23
                    ple.GetMask() == 4294967040,   // 24
                    ple.GetMask() == 4294967168,   // 25
                    ple.GetMask() == 4294967232,   // 26
                    ple.GetMask() == 4294967264,   // 27
                    ple.GetMask() == 4294967280,   // 28
                    ple.GetMask() == 4294967288,   // 29
                    ple.GetMask() == 4294967292,   // 30
                    ple.GetMask() == 4294967294,   // 31
                    ple.GetMask() == 4294967295    // 32
                )
            );

            var le = ple.GetLE();

            predicates.Add(
                Implies(
                    Option.IsSome(le),
                    Or(
                        le.Value() == 0,            // 0
                        le.Value() == 2147483648,   // 1
                        le.Value() == 3221225472,   // 2
                        le.Value() == 3758096384,   // 3
                        le.Value() == 4026531840,   // 4
                        le.Value() == 4160749568,   // 5
                        le.Value() == 4227858432,   // 6
                        le.Value() == 4261412864,   // 7
                        le.Value() == 4278190080,   // 8
                        le.Value() == 4286578688,   // 9
                        le.Value() == 4290772992,   // 10
                        le.Value() == 4292870144,   // 11
                        le.Value() == 4293918720,   // 12
                        le.Value() == 4294443008,   // 13
                        le.Value() == 4294705152,   // 14
                        le.Value() == 4294836224,   // 15
                        le.Value() == 4294901760,   // 16
                        le.Value() == 4294934528,   // 17
                        le.Value() == 4294950912,   // 18                
                        le.Value() == 4294959104,   // 19
                        le.Value() == 4294963200,   // 20
                        le.Value() == 4294965248,   // 21
                        le.Value() == 4294966272,   // 22
                        le.Value() == 4294966784,   // 23
                        le.Value() == 4294967040,   // 24
                        le.Value() == 4294967168,   // 25
                        le.Value() == 4294967232,   // 26
                        le.Value() == 4294967264,   // 27
                        le.Value() == 4294967280,   // 28
                        le.Value() == 4294967288,   // 29
                        le.Value() == 4294967292,   // 30
                        le.Value() == 4294967294,   // 31
                        le.Value() == 4294967295    // 32
                    )
                )
            );

            var ge = ple.GetGE();

            predicates.Add(
                Implies(
                    Option.IsSome(ge),
                    Or(
                        ge.Value() == 0,            // 0
                        ge.Value() == 2147483648,   // 1
                        ge.Value() == 3221225472,   // 2
                        ge.Value() == 3758096384,   // 3
                        ge.Value() == 4026531840,   // 4
                        ge.Value() == 4160749568,   // 5
                        ge.Value() == 4227858432,   // 6
                        ge.Value() == 4261412864,   // 7
                        ge.Value() == 4278190080,   // 8
                        ge.Value() == 4286578688,   // 9
                        ge.Value() == 4290772992,   // 10
                        ge.Value() == 4292870144,   // 11
                        ge.Value() == 4293918720,   // 12
                        ge.Value() == 4294443008,   // 13
                        ge.Value() == 4294705152,   // 14
                        ge.Value() == 4294836224,   // 15
                        ge.Value() == 4294901760,   // 16
                        ge.Value() == 4294934528,   // 17
                        ge.Value() == 4294950912,   // 18
                        ge.Value() == 4294959104,   // 19
                        ge.Value() == 4294963200,   // 20
                        ge.Value() == 4294965248,   // 21
                        ge.Value() == 4294966272,   // 22
                        ge.Value() == 4294966784,   // 23
                        ge.Value() == 4294967040,   // 24
                        ge.Value() == 4294967168,   // 25
                        ge.Value() == 4294967232,   // 26
                        ge.Value() == 4294967264,   // 27
                        ge.Value() == 4294967280,   // 28
                        ge.Value() == 4294967288,   // 29
                        ge.Value() == 4294967292,   // 30
                        ge.Value() == 4294967294,   // 31
                        ge.Value() == 4294967295    // 32
                    )
                )
            );

            predicates.Add(
                Implies(
                    Option.IsSome(le),
                    le.Value() >= ple.GetMask()
                )
            );

            predicates.Add(
                Implies(
                    Option.IsSome(ge),
                    ge.Value() >= ple.GetMask()
                )
            );

            predicates.Add(
                Implies(
                    And(Option.IsSome(le), Option.IsSome(ge)),
                    ge.Value() <= le.Value()
                )
            );

            predicates.Add(
                Or(
                    ple.GetPrefix() == 0,
                    And(
                        ple.GetPrefix() >= 1671377732,
                        ple.GetPrefix() <= 1679687938
                    )
                )
            );

            // predicates.Add(ple.GetPermit() == true);

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

            return predicates.Aggregate((a, b) => And(a, b));
        }

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
                    (ple.GetPrefix() & ple.GetMask()) == (ipa.GetPrefix() & ple.GetMask()),
                    //ple.GetPermit(),
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
                Option.IsSome(ple.GetLE()),
                If<bool>(
                    Option.IsSome(ple.GetGE()),
                    If<bool>(
                        Utils.AndIf(
                            ple.GetGE().Value() <= ipa.GetMask(), 
                            ple.GetLE().Value() >= ipa.GetMask()
                        ),
                        expr1,
                        false
                    ),
                    If<bool>(
                        Utils.AndIf(
                            ipa.GetMask() <= ple.GetLE().Value(),   // Prefix Length <= Route prefix length <= LE  
                            ipa.GetMask() >= ple.GetMask()
                        ),
                        expr1,
                        false
                    )
                ),
                If<bool>(
                    Option.IsSome(ple.GetGE()),
                    If<bool>(ipa.GetMask() >= ple.GetGE().Value(),
                        expr1,
                        false
                    ),
                    expr4
                )
            );
        }
    }

    public class PrefixList{
        public Array<PrefixListEntry, _3> Value {get; set;}

        public Zen<PrefixList> Create(Zen<FSeq<PrefixListEntry>> value){
            return Zen.Create<PrefixList>(("Value", value));
        }

        public override string ToString(){
            return string.Join("\n", Value);
        }
    }

    public static class PrefixListExtensions{
        public static Zen<Array<PrefixListEntry, _3>> GetValue(this Zen<PrefixList> plist) => plist.GetField<PrefixList, Array<PrefixListEntry, _3>>("Value");
        public static Zen<bool> UniquePrefixes(this Zen<PrefixList> plist){
            return And(
                plist.GetValue().Get(0) != plist.GetValue().Get(1),
                plist.GetValue().Get(0) != plist.GetValue().Get(2),
                plist.GetValue().Get(1) != plist.GetValue().Get(2)
            );
        }
        public static Zen<bool> IsValidPrefixList(this Zen<PrefixList> plist){
            return And(
                plist.UniquePrefixes(), // All prefix list entries should be unique
                plist.GetValue().Get(0).IsValidPrefixListEntry(),
                plist.GetValue().Get(1).IsValidPrefixListEntry(),
                plist.GetValue().Get(2).IsValidPrefixListEntry()
            );
        }

        public static Zen<int> GetRowwiseDifference(this Zen<PrefixList> prl1, Zen<PrefixList> prl2){
            var pre_arr1 = prl1.GetValue().ToArray();
            var pre_arr2 = prl2.GetValue().ToArray();

            Zen<int> count = 0;
            for(int i=0;i<3;i++){
                count = If<int>(
                    Utils.AndIf(
                        pre_arr1[i].GetAny() == true,
                        pre_arr2[i].GetAny() == true
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

        public static Zen<bool> MatchAgainstPrefixList(this Zen<PrefixList> plist, Zen<IPAttr> ipa){
            /*var lambda = Zen.Lambda<FSeq<PrefixListEntry>, bool>();
            lambda.Initialize(arg =>
            {
                return arg.CaseStrict(
                    empty: false,
                    cons: Zen.Lambda<Pair<PrefixListEntry, FSeq<PrefixListEntry>>, bool>(x =>
                    {
                        var hd = x.Item1();
                        var tl = x.Item2();

                        return If<bool>(
                            hd.MatchAgainstPrefix(ipa),
                            hd.GetPermit(),
                            lambda.Apply(tl)
                        );
                    })
                );
            });

            return lambda.Apply(plist.GetValue());*/

            var arr = plist.GetValue().ToArray();
            /*for(int i=0;i<3;i++){
                If<int>(arr[i].MatchAgainstPrefix(ipa)) return arr[i].GetPermit();
            }
            return false;*/
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