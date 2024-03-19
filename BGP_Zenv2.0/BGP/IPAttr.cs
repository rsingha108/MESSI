using System;
using System.Collections.Generic;
using System.Numerics;
using ZenLib;
using System.Linq;
using System.IO;
using static ZenLib.Zen;

namespace BGP{
    public class IPAttr{
        public uint Prefix{get; set;}
        public uint Mask{get; set;}
        public uint LP {get; set;}
        public uint MED {get; set;}
        public uint Index {get; set;}
        public FSeq<uint> CommunityAsList {get; set;}
        public uint IndexAS {get; set;}
        public FSeq<uint> ASPath {get; set;}
        public uint NextHop;

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

        public static Zen<IPAttr> Create(Zen<uint> prefix, Zen<uint> mask, Zen<uint> lp, Zen<uint> med, Zen<uint> index, Zen<FSeq<uint>> communityaslist, Zen<uint> index_as, Zen<FSeq<uint>> aspath, Zen<uint> nexthop){
            return Zen.Create<IPAttr>(
                ("Prefix", prefix), 
                ("Mask", mask), 
                ("LP", lp), 
                ("MED", med),
                ("Index", index),
                ("CommunityAsList", communityaslist),
                ("IndexAS", index_as),
                ("ASPath", aspath),
                ("NextHop", nexthop)
            );
        }

        public static string ToStringCom(FSeq<uint> CommunityAsList){
            var coms = CommunityAsList.ToList().Select(x => CommunityInt2Str(x)).ToArray();

            var com_arr = "";
            for(int i=0;i<coms.Length;i++){
                com_arr += coms[i];
                if(i != coms.Length - 1){
                    com_arr += " ";
                }
            }

            return $"{com_arr}";
        }

        public static string ToStringAS(FSeq<uint> ASPath){
            var aspath = ASPath.ToList().ToArray();

            var aspath_arr = "";
            for(int i=0;i<aspath.Length;i++){
                aspath_arr += aspath[i];
                if(i != aspath.Length - 1){
                    aspath_arr += " ";
                }
            }

            return $"{aspath_arr}";
        }

        public override string ToString(){

            var coms = CommunityAsList.ToList().Select(x => CommunityInt2Str(x)).ToArray();
            var aspath = ASPath.ToList().ToArray();

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

            var nh1 = NextHop / (1<<24);
            rem1 = NextHop % (1<<24);
            var nh2 = rem1 / (1 << 16);
            rem2 = rem1 % (1 << 16);
            var nh3 = rem2 / (1 << 8);
            var nh4 = rem2 % (1 << 8);

            var com_arr = "";
            for(int i=0;i<coms.Length;i++){
                com_arr += coms[i];
                if(i != coms.Length - 1){
                    com_arr += ", ";
                }
            }

            var aspath_arr = "";
            for(int i=0;i<aspath.Length;i++){
                aspath_arr += aspath[i];
                if(i != aspath.Length - 1){
                    aspath_arr += ", ";
                }
            }

            return $"Prefix: {pre1}.{pre2}.{pre3}.{pre4}/{count}, Local Preference: {LP}, MED: {MED}, Community: [{com_arr}], AS Path: [{aspath_arr}], Next Hop: {nh1}.{nh2}.{nh3}.{nh4}";
        }
    }

    public static class IPAttrExtensions{
        public static Zen<uint> GetPrefix(this Zen<IPAttr> ipa) => ipa.GetField<IPAttr, uint>("Prefix");
        public static Zen<uint> GetMask(this Zen<IPAttr> ipa) => ipa.GetField<IPAttr, uint>("Mask");
        public static Zen<uint> GetLP(this Zen<IPAttr> ipa) => ipa.GetField<IPAttr, uint>("LP");
        public static Zen<uint> GetMED(this Zen<IPAttr> ipa) => ipa.GetField<IPAttr, uint>("MED");
        public static Zen<uint> GetIndex(this Zen<IPAttr> ipa) => ipa.GetField<IPAttr, uint>("Index");
        public static Zen<FSeq<uint>> GetCommunityAsList(this Zen<IPAttr> ipa) => ipa.GetField<IPAttr, FSeq<uint>>("CommunityAsList");
        public static Zen<uint> GetIndexAS(this Zen<IPAttr> ipa) => ipa.GetField<IPAttr, uint>("IndexAS");
        public static Zen<FSeq<uint>> GetASPathAsList(this Zen<IPAttr> ipa) => ipa.GetField<IPAttr, FSeq<uint>>("ASPath");
        public static Zen<uint> GetNextHop(this Zen<IPAttr> ipa) => ipa.GetField<IPAttr, uint>("NextHop");

        public static Zen<bool> CheckCommunity(this Zen<IPAttr> ipa, Array<FSeq<uint>, _3> pos, Array<FSeq<uint>, _3> neg){
            var res = False();
            for(int i=0;i<pos.Length();i++){
                res = Utils.OrIf(
                    res,
                    ipa.GetCommunityAsList() == pos.Get(i)
                );
            }

            for(int i=0;i<neg.Length();i++){
                res = Utils.OrIf(
                    res,
                    ipa.GetCommunityAsList() == neg.Get(i)
                );
            }

            return res;
        }

        public static Zen<bool> CheckASPath(this Zen<IPAttr> ipa, Array<FSeq<uint>, _3> pos, Array<FSeq<uint>, _3> neg){
            var res = False();
            for(int i=0;i<pos.Length();i++){
                res = Utils.OrIf(
                    res,
                    ipa.GetASPathAsList() == pos.Get(i)
                );
            }

            for(int i=0;i<pos.Length();i++){
                res = Utils.OrIf(
                    res,
                    ipa.GetASPathAsList() == neg.Get(i)
                );
            }

            return res;
        }


        public static Zen<bool> IsValidIPAttr(this Zen<IPAttr> ipa, List<Array<FSeq<uint>, _3>> pos_com, List<Array<FSeq<uint>, _3>> neg_com, List<Array<FSeq<uint>, _3>> pos_as, List<Array<FSeq<uint>, _3>> neg_as){
            var lp = ipa.GetLP();
            var med = ipa.GetMED();

            IList<Zen<bool>> predicates = new List<Zen<bool>>();
            predicates.Add(
                And(lp >= 100, lp <= 900,
                    med >= 0, med <= 800
                )
            );

            predicates.Add(
                And(
                    ipa.GetPrefix() >= 1671377732,
                    ipa.GetPrefix() <= 1679687938,
                    ipa.GetNextHop() >= 1671377732,
                    ipa.GetNextHop() <= 1679687938
                )
            );

            predicates.Add(
                 Or(
                    ipa.GetMask() == 0,            // 0
                    ipa.GetMask() == 2147483648,   // 1
                    ipa.GetMask() == 3221225472,   // 2
                    ipa.GetMask() == 3758096384,   // 3
                    ipa.GetMask() == 4026531840,   // 4
                    ipa.GetMask() == 4160749568,   // 5
                    ipa.GetMask() == 4227858432,   // 6
                    ipa.GetMask() == 4261412864,   // 7
                    ipa.GetMask() == 4278190080,   // 8
                    ipa.GetMask() == 4286578688,   // 9
                    ipa.GetMask() == 4290772992,   // 10
                    ipa.GetMask() == 4292870144,   // 11
                    ipa.GetMask() == 4293918720,   // 12
                    ipa.GetMask() == 4294443008,   // 13
                    ipa.GetMask() == 4294705152,   // 14
                    ipa.GetMask() == 4294836224,   // 15
                    ipa.GetMask() == 4294901760,   // 16
                    ipa.GetMask() == 4294934528,   // 17
                    ipa.GetMask() == 4294950912,   // 18
                    ipa.GetMask() == 4294959104,   // 19
                    ipa.GetMask() == 4294963200,   // 20
                    ipa.GetMask() == 4294965248,   // 21
                    ipa.GetMask() == 4294966272,   // 22
                    ipa.GetMask() == 4294966784,   // 23
                    ipa.GetMask() == 4294967040,   // 24
                    ipa.GetMask() == 4294967168,   // 25
                    ipa.GetMask() == 4294967232,   // 26
                    ipa.GetMask() == 4294967264,   // 27
                    ipa.GetMask() == 4294967280,   // 28
                    ipa.GetMask() == 4294967288,   // 29
                    ipa.GetMask() == 4294967292,   // 30
                    ipa.GetMask() == 4294967294,   // 31
                    ipa.GetMask() == 4294967295    // 32
                )
            );

            predicates.Add(
                And(
                    (ipa.GetPrefix() & 4278190080) != 4278190080,  // first byte cannot equal 255
                    (ipa.GetPrefix() & 4261412864) != 4261412864,  // first byte cannot equal 254
                    (ipa.GetPrefix() & 16711680) != 16711680, // second byte cannot equal 255
                    (ipa.GetPrefix() & 16646144) != 16646144, // second byte cannot equal 254
                    (ipa.GetPrefix() & 65280) != 65280, // third byte cannot equal 255
                    (ipa.GetPrefix() & 65024) != 65024, // third byte cannot equal 254
                    (ipa.GetPrefix() & 255) != 255, // fourth byte cannot equal 255
                    (ipa.GetPrefix() & 254) != 254, // fourth byte cannot equal 254,
                    (ipa.GetNextHop() & 4278190080) != 4278190080,  // first byte cannot equal 255
                    (ipa.GetNextHop() & 4261412864) != 4261412864,  // first byte cannot equal 254
                    (ipa.GetNextHop() & 16711680) != 16711680, // second byte cannot equal 255
                    (ipa.GetNextHop() & 16646144) != 16646144, // second byte cannot equal 254
                    (ipa.GetNextHop() & 65280) != 65280, // third byte cannot equal 255
                    (ipa.GetNextHop() & 65024) != 65024, // third byte cannot equal 254
                    (ipa.GetNextHop() & 255) != 255, // fourth byte cannot equal 255
                    (ipa.GetNextHop() & 254) != 254 // fourth byte cannot equal 254  
                )
            );

            /*string regex = "(0|[1-9][0-9]{0,3}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])"; // all numbers from 0-65535 accecpted
            regex = "(" + regex + ":" + regex + ")"; // single communities of the form AA:NN
            regex = "^(" + regex + " ){0,1}" + regex + "$";
            Regex<char> r1 = Regex.Parse(regex);

            predicates.Add(
                ipa.GetCommunity().MatchesRegex(r1)
            );*/
            
            var res = False();
            for(int i=0;i<pos_com.Count;i++){
                res = Utils.OrIf(
                    res,
                    If<bool>(
                        ipa.GetIndex() == (uint)i,
                        ipa.CheckCommunity(pos_com[i], neg_com[i]),
                        false
                    )
                );
            }

            predicates.Add(res);

            var res2 = False();
            for(int i=0;i<pos_as.Count;i++){
                res2 = Utils.OrIf(
                    res2,
                    If<bool>(
                        ipa.GetIndexAS() == (uint)i,
                        ipa.CheckASPath(pos_as[i], neg_as[i]),
                        false
                    )
                );
            }

            predicates.Add(res2);

            
            return predicates.Aggregate((a, b) => And(a, b));
        }
    }
}