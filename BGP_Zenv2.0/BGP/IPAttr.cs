using System;
using System.Collections.Generic;
using System.Numerics;
using ZenLib;
using System.Linq;
using System.IO;
using static ZenLib.Zen;

namespace BGP{
    /// <summary>
    /// Class for defining Route Advertisements
    /// </summary>
    public class IPAttr{
        /// <summary>
        /// IPv4 prefix (stored in unsinged integer format)
        /// </summary>
        public uint Prefix{get; set;}

        /// <summary>
        /// Subnet mask (stored in unsigned integer format)
        /// </summary>
        public uint Mask{get; set;}

        /// <summary>
        /// Local preference
        /// </summary>
        public uint LP {get; set;}

        /// <summary>
        /// Multi-exit discriminator
        /// </summary>
        public uint MED {get; set;}

        /// <summary>
        /// Used for indexing community regexes (only used for writing constraints)
        /// </summary>
        public uint Index {get; set;}

        /// <summary>
        /// Set of communities in the route
        /// </summary>
        public FSeq<uint> CommunityAsList {get; set;}

        /// <summary>
        /// Used for indexing AS path regexes (only used for writing constraints)
        /// </summary>
        public uint IndexAS {get; set;}

        /// <summary>
        /// Sequence of AS numbers
        /// </summary>
        public FSeq<uint> ASPath {get; set;}

        /// <summary>
        /// IP next hop
        /// </summary>
        public uint NextHop;

        /// <summary>
        /// Converts community from unsigned integer to AA:NN format
        /// </summary>
        /// <param name="s">the community value</param>
        /// <returns>community value in AA:NN format</returns>
        public static string CommunityInt2Str(uint s){
            var a = s / (1 << 16);
            var b = s % (1 << 16);
            return $"{a}:{b}";
        }

        /// <summary>
        /// Converts the next-hop IP address from unsigned integer to 4 octets
        /// </summary>
        /// <param name="ip">the next-hop IP address</param>
        /// <returns>IP address in 4 octect format</returns>
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
        /// Creates a Zen Route advertisement
        /// </summary>
        /// <param name="prefix">IPv4 prefix</param>
        /// <param name="mask">subnet mask</param>
        /// <param name="lp">local preference</param>
        /// <param name="med">multi-exit discriminator</param>
        /// <param name="index">used for indexing community regexes</param>
        /// <param name="communityaslist">list of communities</param>
        /// <param name="index_as">used for indexing AS path regexes</param>
        /// <param name="aspath">AS path</param>
        /// <param name="nexthop">next-hop IP address</param>
        /// <returns>Zen Route advertisement</returns>
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

        /// <summary>
        /// Converts list of communities in unsigned integer form to AA:NN form
        /// </summary>
        /// <param name="CommunityAsList">list of community values</param>
        /// <returns>string containing all communities in AA:NN format</returns>
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
        
        /// <summary>
        /// Converts AS path to string form
        /// </summary>
        /// <param name="ASPath">AS path</param>
        /// <returns>string containing AS numbers</returns>
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


        /// <summary>
        /// Converts Zen Route advertisement to string
        /// </summary>
        /// <returns>A string</returns>
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

    /// <summary>
    /// IPAttr extensions class
    /// </summary>
    public static class IPAttrExtensions{
        /// <summary>
        /// Retrieve the prefix
        /// </summary>
        /// <param name="ipa">the input route</param>
        /// <returns>the prefix</returns>
        public static Zen<uint> GetPrefix(this Zen<IPAttr> ipa) => ipa.GetField<IPAttr, uint>("Prefix");
        
        /// <summary>
        /// Retirve the subnet mask
        /// </summary>
        /// <param name="ipa">the input route</param>
        /// <returns>the subnet mask</returns>
        public static Zen<uint> GetMask(this Zen<IPAttr> ipa) => ipa.GetField<IPAttr, uint>("Mask");
        
        /// <summary>
        /// Retrieve the local preference value
        /// </summary>
        /// <param name="ipa">the input route</param>
        /// <returns>the subnet mask</returns>
        public static Zen<uint> GetLP(this Zen<IPAttr> ipa) => ipa.GetField<IPAttr, uint>("LP");
        
        /// <summary>
        /// Retrieve the multi-exit discriminator
        /// </summary>
        /// <param name="ipa">the input route</param>
        /// <returns>the multi-exit discriminator</returns>
        public static Zen<uint> GetMED(this Zen<IPAttr> ipa) => ipa.GetField<IPAttr, uint>("MED");
        
        /// <summary>
        /// Gets the index of the community regex
        /// </summary>
        /// <param name="ipa"></param>
        /// <returns>index of the community regex</returns>
        public static Zen<uint> GetIndex(this Zen<IPAttr> ipa) => ipa.GetField<IPAttr, uint>("Index");
        
        /// <summary>
        /// Gets the community attribute
        /// </summary>
        /// <param name="ipa">the input route</param>
        /// <returns>list of community values</returns>
        public static Zen<FSeq<uint>> GetCommunityAsList(this Zen<IPAttr> ipa) => ipa.GetField<IPAttr, FSeq<uint>>("CommunityAsList");
        
        /// <summary>
        /// Gets the index of the AS path regex
        /// </summary>
        /// <param name="ipa">the input route</param>
        /// <returns>the index</returns>
        public static Zen<uint> GetIndexAS(this Zen<IPAttr> ipa) => ipa.GetField<IPAttr, uint>("IndexAS");
        
        /// <summary>
        /// Gets the AS path attribute
        /// </summary>
        /// <param name="ipa">the input route</param>
        /// <returns>the AS path</returns>
        public static Zen<FSeq<uint>> GetASPathAsList(this Zen<IPAttr> ipa) => ipa.GetField<IPAttr, FSeq<uint>>("ASPath");
        
        /// <summary>
        /// Retrieve the next-hop IP address
        /// </summary>
        /// <param name="ipa">the input route</param>
        /// <returns>the next-hop IP address</returns>
        public static Zen<uint> GetNextHop(this Zen<IPAttr> ipa) => ipa.GetField<IPAttr, uint>("NextHop");

        /// <summary>
        /// Check whether the community value is among the allowed ones
        /// </summary>
        /// <param name="ipa">the input route</param>
        /// <param name="pos">the set of allowed positive examples</param>
        /// <param name="neg">the set of allowed negative examples</param>
        /// <returns>a boolean</returns>
        private static Zen<bool> CheckCommunity(this Zen<IPAttr> ipa, Array<FSeq<uint>, _3> pos, Array<FSeq<uint>, _3> neg){
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

        /// <summary>
        /// Checks whether the AS path is among the set of allowed ones
        /// </summary>
        /// <param name="ipa">the input route</param>
        /// <param name="pos">the set of positive examples</param>
        /// <param name="neg">the set of negative examples</param>
        /// <returns>a boolean</returns>
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

        /// <summary>
        /// Gets the difference between 2 input routes
        /// </summary>
        /// <param name="ipa1">the first input route</param>
        /// <param name="ipa2">the second input route</param>
        /// <returns>an integer</returns>
        public static Zen<int> GetDifference(this Zen<IPAttr> ipa1, Zen<IPAttr> ipa2){
            Zen<int> count = 0;
            count = If(
                Utils.OrIf(
                    ipa1.GetPrefix() != ipa2.GetPrefix(),
                    ipa1.GetMask() != ipa2.GetMask()
                ),
                count + 1,
                count
            );

            count = If(
                ipa1.GetLP() != ipa2.GetLP(),
                count + 1,
                count
            );

            count = If(
                ipa1.GetMED() != ipa2.GetMED(),
                count + 1,
                count
            );

            count = If(
                ipa1.GetCommunityAsList() != ipa2.GetCommunityAsList(),
                count + 1,
                count
            );

            count = If(
                ipa1.GetASPathAsList() != ipa2.GetASPathAsList(),
                count + 1,
                count
            );

            return count;
        }

        /// <summary>
        /// Checks whether a route map gives different decisions for two input routes
        /// </summary>
        /// <param name="rme">the route-map stanza</param>
        /// <param name="ipa1">the first input route</param>
        /// <param name="ipa2">the second input route</param>
        /// <returns>a boolean</returns>
        public static Zen<bool> DecisionDiffer(Zen<RouteMapEntry> rme, Zen<IPAttr> ipa1, Zen<IPAttr> ipa2){
            var dec1 = rme.MatchAgainstEntry(ipa1);
            var dec2 = rme.MatchAgainstEntry(ipa2);

            return If<bool>(
                dec1.Item2() == dec2.Item2(),
                false,
                true
            );
        }

        private static Zen<bool> IsValidMask(this Zen<uint> num){
            uint n = 0;
            Zen<bool> constraints = (num == n);
            for(int i=0;i<32;i++){
                n |= ((uint)1) << (31-i);
                constraints = Or(constraints, num == n);
            }

            return constraints;
        }
        
        /// <summary>
        /// Checks whether the input route is valid
        /// </summary>
        /// <param name="ipa">the input route</param>
        /// <param name="pos_com">the set of positive community examples</param>
        /// <param name="neg_com">the set of negative community examples</param>
        /// <param name="pos_as">the set of positive AS path examples</param>
        /// <param name="neg_as">the set of negative AS path examples</param>
        /// <returns>a boolean</returns>
        public static Zen<bool> IsValidIPAttr(this Zen<IPAttr> ipa, List<Array<FSeq<uint>, _3>> pos_com, List<Array<FSeq<uint>, _3>> neg_com, List<Array<FSeq<uint>, _3>> pos_as, List<Array<FSeq<uint>, _3>> neg_as){
            var lp = ipa.GetLP();
            var med = ipa.GetMED();

            Zen<uint> x = ipa.GetIndex();
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
                ipa.GetMask().IsValidMask()
            );

            predicates.Add(
                (ipa.GetPrefix() & ipa.GetMask()) > 0
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