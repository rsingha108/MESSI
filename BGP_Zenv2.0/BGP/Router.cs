using System;
using System.Collections.Generic;
using System.Numerics;
using ZenLib;
using static ZenLib.Zen;

namespace BGP{
    public class RoutesForDecisionProcess{
        /// <summary>
        /// Local preference
        /// </summary>
        public uint LP;

        /// <summary>
        /// Length of AS path
        /// </summary>
        public uint ASPathLength;

        /// <summary>
        /// Origin: i or e
        /// </summary>
        public char Origin;

        /// <summary>
        /// Multi-exit discriminator
        /// </summary>
        public uint MED;

        /// <summary>
        /// AS Number
        /// </summary>
        public uint ASN;
        
        /// <summary>
        /// IGP Metric
        /// </summary>
        public uint IGP;

        /// <summary>
        /// Router-ID
        /// </summary>
        public uint RID;

        /// <summary>
        /// Neighbor Address
        /// </summary>
        public uint NeighborAddr;

        /// <summary>
        /// Arrival Time
        /// </summary>
        public uint ArrivalTime;
        
        /// <summary>
        /// Create a Zen RoutesForDecisionProcess object
        /// </summary>
        /// <param name="lp">local preference</param>
        /// <param name="aspathlen">AS path length</param>
        /// <param name="origin">origin</param>
        /// <param name="med">multi-exit discriminator</param>
        /// <param name="asn">AS number</param>
        /// <param name="igp">IGP metric</param>
        /// <param name="rid">router-id</param>
        /// <param name="ngbr">neighbor address</param>
        /// <returns></returns>
        public static Zen<RoutesForDecisionProcess> Create(
            Zen<uint> lp, 
            Zen<uint> aspathlen,
            Zen<char> origin,
            Zen<uint> med,
            Zen<uint> asn,
            Zen<uint> igp,
            Zen<uint> rid,
            Zen<uint> ngbr,
            Zen<uint> arrtime
        ){
            return Zen.Create<RoutesForDecisionProcess>(
                ("LP", lp),
                ("ASPathLength", aspathlen),
                ("Origin", origin),
                ("MED", med),
                ("ASN", asn),
                ("IGP", igp),
                ("RID", rid),
                ("NeighborAddr", ngbr),
                ("ArrivalTime", arrtime)
            );
        }

        /// <summary>
        /// Convert Route object to string
        /// </summary>
        /// <returns>a string</returns>
        public override string ToString(){
            var rid1 = RID/(1<<24);
            var rem1 = RID%(1<<24);

            var rid2 = rem1 / (1 << 16);
            var rem2 = rem1 % (1 << 16);

            var rid3 = rem2 / (1 << 8);
            var rid4 = rem2 % (1 << 8);

            var ngbr1 = NeighborAddr/(1<<24);
            rem1 = NeighborAddr%(1<<24);

            var ngbr2 = rem1 / (1 << 16);
            rem2 = rem1 % (1 << 16);

            var ngbr3 = rem2 / (1 << 8);
            var ngbr4 = rem2 % (1 << 8);



            return $"LP:{LP}, AS Path Length: {ASPathLength}, Origin: {Origin}, MED: {MED}, AS Number: {ASN}, IGP Metric: {IGP}, Router ID: {rid1}.{rid2}.{rid3}.{rid4}, Neighbor Address: {ngbr1}.{ngbr2}.{ngbr3}.{ngbr4}, Arrival Time: {ArrivalTime}\n";
        }
    }

    /// <summary>
    /// Extensions class for RoutesForDecisionProcess
    /// </summary>
    public static class RoutesForDecisionProcessExtensions{
        /// <summary>
        /// Gets the local preference
        /// </summary>
        /// <param name="rdp">the input route</param>
        /// <returns>the local preference</returns>
        public static Zen<uint> GetLP(this Zen<RoutesForDecisionProcess> rdp) => rdp.GetField<RoutesForDecisionProcess, uint>("LP");
        
        /// <summary>
        /// Gets the length of the AS path
        /// </summary>
        /// <param name="rdp">the input route</param>
        /// <returns>the length of the AS path</returns>
        public static Zen<uint> GetASPLen(this Zen<RoutesForDecisionProcess> rdp) => rdp.GetField<RoutesForDecisionProcess, uint>("ASPathLength");
        
        /// <summary>
        /// Gets the origin
        /// </summary>
        /// <param name="rdp">the input route</param>
        /// <returns>the origin</returns>
        public static Zen<char> GetOrigin(this Zen<RoutesForDecisionProcess> rdp) => rdp.GetField<RoutesForDecisionProcess, char>("Origin");
        
        /// <summary>
        /// Gets the mult-exit discriminator
        /// </summary>
        /// <param name="rdp">the input route</param>
        /// <returns>the med value</returns>
        public static Zen<uint> GetMED(this Zen<RoutesForDecisionProcess> rdp) => rdp.GetField<RoutesForDecisionProcess, uint>("MED");
        
        /// <summary>
        /// Gets the AS number
        /// </summary>
        /// <param name="rdp">the input route</param>
        /// <returns>the AS number</returns>
        public static Zen<uint> GetASN(this Zen<RoutesForDecisionProcess> rdp) => rdp.GetField<RoutesForDecisionProcess, uint>("ASN");
        
        /// <summary>
        /// Get the IGP metric
        /// </summary>
        /// <param name="rdp">the input route</param>
        /// <returns>the igp metric</returns>
        public static Zen<uint> GetIGP(this Zen<RoutesForDecisionProcess> rdp) => rdp.GetField<RoutesForDecisionProcess, uint>("IGP");
        
        /// <summary>
        /// Gets the router-id
        /// </summary>
        /// <param name="rdp">the input route</param>
        /// <returns>the router-id</returns>
        public static Zen<uint> GetRID(this Zen<RoutesForDecisionProcess> rdp) => rdp.GetField<RoutesForDecisionProcess, uint>("RID");
        
        /// <summary>
        /// Gets the neighbor address
        /// </summary>
        /// <param name="rdp">the input route</param>
        /// <returns>the neighbor address</returns>
        public static Zen<uint> GetNgbr(this Zen<RoutesForDecisionProcess> rdp) => rdp.GetField<RoutesForDecisionProcess, uint>("NeighborAddr");
        
        /// <summary>
        /// Gets the arrival time
        /// </summary>
        /// <param name="rdp">the input route</param>
        /// <returns>the arrival time</returns>
        public static Zen<uint> GetArrTime(this Zen<RoutesForDecisionProcess> rdp) => rdp.GetField<RoutesForDecisionProcess, uint>("ArrivalTime");

        public static Zen<bool> IsValidRoute(this Zen<RoutesForDecisionProcess> rdp){
            int[] t1 = {1, 1, 1, 1};
            int[] t2 = {3, 3, 3, 1};
            return And(
                rdp.GetLP() >= 100, rdp.GetLP() <= 300,
                rdp.GetASPLen() >= 1, rdp.GetASPLen() <= 4,
                Or(rdp.GetOrigin() == 'i', rdp.GetOrigin() == 'e'),
                rdp.GetMED() >= 100, rdp.GetMED() <= 300,
                rdp.GetASN() >= 100, rdp.GetASN() <= 700,
                rdp.GetIGP() >= 300, rdp.GetIGP() <= 1000,
                rdp.GetRID() >= 1671377732, rdp.GetRID() <= 1679687938,
                Utils.OrIf(rdp.GetNgbr() == Utils.PrefixToUint(t1), rdp.GetNgbr() == Utils.PrefixToUint(t2)),
                rdp.GetArrTime() >= 0, rdp.GetArrTime() <= 1
            );
        }
    }

    /// <summary>
    /// Router class for route aggregation model
    /// </summary>
    public class Router{
        /// <summary>
        /// The aggregate prefix
        /// </summary>
        public IPAttr AggregateRoute;

        /// <summary>
        /// The summary-only flag
        /// </summary>
        public bool SummaryOnly;

        /// <summary>
        /// The matching-med-only flag
        /// </summary>
        public bool MatchingMEDOnly;

        /// <summary>
        /// AS number
        /// </summary>
        public uint AS;

        /// <summary>
        /// Create a Zen Router object
        /// </summary>
        /// <param name="aggregateroute">the aggregate prefix</param>
        /// <param name="summary">summary-only flag</param>
        /// <param name="match_med">matching med only flag</param>
        /// <param name="asnum">as number (although instantiated, but not used in current model)</param>
        /// <returns></returns>
        public static Zen<Router> Create(Zen<IPAttr> aggregateroute, Zen<bool> summary, Zen<bool> match_med, Zen<uint> asnum){
            return Zen.Create<Router>(
                ("AggregateRoute", aggregateroute),
                ("SummaryOnly", summary),
                ("MatchingMEDOnly", match_med),
                ("AS", asnum)
            );
        }

        /// <summary>
        /// Converts Zen Router to a string
        /// </summary>
        /// <returns>the string</returns>
        public override string ToString(){
            var prefix = AggregateRoute.Prefix;
            var mask = AggregateRoute.Mask;

            var pre1 = prefix/(1<<24);
            var rem1 = prefix%(1<<24);

            var pre2 = rem1 / (1 << 16);
            var rem2 = rem1 % (1 << 16);

            var pre3 = rem2 / (1 << 8);
            var pre4 = rem2 % (1 << 8);

            uint count = 0;
            var n = mask;
            while(n > 0){
                n &= (n - 1);
                count++;
            }

            return $"Aggregate Route: {pre1}.{pre2}.{pre3}.{pre4}/{count}, Summary-Only: {SummaryOnly}, MatchingMEDOnly: {MatchingMEDOnly}, Router AS: {AS}";
        }    
    }

    public static class RouterExtensions{
        /// <summary>
        /// Gets the aggregate prefix
        /// </summary>
        /// <param name="rt">route aggregation configuration</param>
        /// <returns>the aggregate prefix</returns>
        public static Zen<IPAttr> GetAggRoute(this Zen<Router> rt) => rt.GetField<Router, IPAttr>("AggregateRoute");
        
        /// <summary>
        /// Gets the summary only flag
        /// </summary>
        /// <param name="rt">route aggregation configuration</param>
        /// <returns>the summary only flag (bool)</returns>
        public static Zen<bool> GetSummary(this Zen<Router> rt)=> rt.GetField<Router, bool>("SummaryOnly");
        
        /// <summary>
        /// Gets the matching-med-only flag
        /// </summary>
        /// <param name="rt">route aggregation configuration</param>
        /// <returns>the matching-med-only flag (bool)</returns>
        public static Zen<bool> GetMatchingMEDOnly(this Zen<Router> rt)=> rt.GetField<Router, bool>("MatchingMEDOnly");
        
        /// <summary>
        /// Gets the AS number of the current router
        /// </summary>
        /// <param name="rt">route aggregation configuration</param>
        /// <returns>the AS number</returns>
        public static Zen<uint> GetAS(this Zen<Router> rt)=> rt.GetField<Router, uint>("AS");

        private static Zen<bool> IsValidMask(this Zen<uint> num){
            uint n = 0;
            Zen<bool> constraints = True();
            for(int i=0;i<32;i++){
                n |= ((uint)1) << (31-i);
                constraints = Or(constraints, num == n);
            }

            return constraints;
        }

        /// <summary>
        /// Checks whether the router aggregation configuration is valid
        /// </summary>
        /// <param name="rt">route aggregation configuration</param>
        /// <returns>true or false</returns>
        public static Zen<bool> IsValidRouterConfig(this Zen<Router> rt){
            var aggroute = rt.GetAggRoute();
            return And(
                // allowed set of prefixes
                aggroute.GetPrefix() >= 1671377732,
                aggroute.GetPrefix() <= 1679687938,

                // valid masks
                aggroute.GetMask().IsValidMask(),

                And(
                    (aggroute.GetPrefix() & 4278190080) != 4278190080,  // first byte cannot equal 255
                    (aggroute.GetPrefix() & 4261412864) != 4261412864,  // first byte cannot equal 254
                    (aggroute.GetPrefix() & 16711680) != 16711680, // second byte cannot equal 255
                    (aggroute.GetPrefix() & 16646144) != 16646144, // second byte cannot equal 254
                    (aggroute.GetPrefix() & 65280) != 65280, // third byte cannot equal 255
                    (aggroute.GetPrefix() & 65024) != 65024, // third byte cannot equal 254
                    (aggroute.GetPrefix() & 255) != 255, // fourth byte cannot equal 255
                    (aggroute.GetPrefix() & 254) != 254 // fourth byte cannot equal 254 
                ),

                // These attributes are not needed for aggregation modeling
                aggroute.GetLP() == 0,
                aggroute.GetMED() == 0,
                aggroute.GetCommunityAsList().Length() == (BigInteger)0,
                aggroute.GetASPathAsList().Length() == (BigInteger)0,
                aggroute.GetNextHop() == 0,

                rt.GetAS() == 100
            );
        }

        /// <summary>
        /// Computes the route aggregation results for two input route advertisements (optimized version)
        /// </summary>
        /// <param name="rt">route aggregation configuration</param>
        /// <param name="ipa1">first route advertisement</param>
        /// <param name="ipa2">second route advertisement</param>
        /// <returns>array of routes obtained after aggregation</returns>
        public static Zen<FSeq<IPAttr>> GetRouteAdvertisements(this Zen<Router> rt, Zen<IPAttr> ipa1, Zen<IPAttr> ipa2){
            var aggrt = rt.GetAggRoute();

            Zen<bool> r1 = Utils.AndIf(
                aggrt.GetMask() <= ipa1.GetMask(),
                (aggrt.GetPrefix() & aggrt.GetMask()) == (ipa1.GetPrefix() & aggrt.GetMask())
            );

            Zen<bool> r2 = Utils.AndIf(
                aggrt.GetMask() <= ipa2.GetMask(),
                (aggrt.GetPrefix() & aggrt.GetMask()) == (ipa2.GetPrefix() & aggrt.GetMask())
            );

            Zen<FSeq<IPAttr>> res = new FSeq<IPAttr>();

            var match_med = If<bool>(Utils.AndIf(Utils.AndIf(r1,r2), Utils.AndIf(rt.GetMatchingMEDOnly(), ipa1.GetMED() != ipa2.GetMED())), false, true);

            res = If(Not(r1), res.AddBack(ipa1), res);
            res = If(Not(r2), res.AddBack(ipa2), res);
            res = If(Utils.AndIf(Utils.OrIf(r1,r2), match_med), res.AddBack(aggrt), res);
            res = If(Utils.AndIf(Not(rt.GetSummary()), r1), res.AddBack(ipa1), res);
            res = If(Utils.AndIf(Not(rt.GetSummary()), r2), res.AddBack(ipa2), res);

            res = If(Utils.AndIf(rt.GetSummary(), Not(match_med)), res.AddBack(ipa1), res);
            res = If(Utils.AndIf(rt.GetSummary(), Not(match_med)), res.AddBack(ipa2), res);

            return res;
        }

        /// <summary>
        /// Compute the distance between 2 route aggregation configurations
        /// </summary>
        /// <param name="rt1">first aggregation configuration</param>
        /// <param name="rt2">second aggregation configuration</param>
        /// <returns>the distance between the 2 configurations</returns>
        public static Zen<int> RouterDifferences(Zen<Router> rt1, Zen<Router> rt2){
            Zen<int>count = 0;
            var aggrt1 = rt1.GetAggRoute();
            var aggrt2 = rt2.GetAggRoute();


            count = If(
                Utils.OrIf(
                    aggrt1.GetPrefix() != aggrt2.GetPrefix(),
                    aggrt1.GetMask() != aggrt2.GetMask()
                ),
                count + 1,
                count
            );

            count = If(
                rt1.GetSummary() != rt2.GetSummary(),
                count + 1,
                count
            );

            count = If(
                rt1.GetMatchingMEDOnly() != rt2.GetMatchingMEDOnly(),
                count + 1,
                count
            );


            return count;
        }

        /// <summary>
        /// Checks whether two aggregation configurations return different results for the same two input route advertisements
        /// </summary>
        /// <param name="rt1">first route aggregation configuration</param>
        /// <param name="rt2">second route aggregation configuration</param>
        /// <param name="ipa1">first route advertisement</param>
        /// <param name="ipa2">second route advertisement</param>
        /// <returns>true or false</returns>
        public static Zen<bool> DecisionDiffer(Zen<Router> rt1, Zen<Router> rt2, Zen<IPAttr> ipa1, Zen<IPAttr> ipa2){
            Zen<FSeq<IPAttr>> res1 = rt1.GetRouteAdvertisements(ipa1, ipa2);
            Zen<FSeq<IPAttr>> res2 = rt2.GetRouteAdvertisements(ipa1, ipa2);

            return If<bool>(
                res1.Length() != res2.Length(),
                true,
                If<bool>(
                    res1.Length() >= (BigInteger)1,
                    If<bool>(
                        Utils.OrIf(res1.At(0).Value().GetPrefix() != res2.At(0).Value().GetPrefix(), Utils.OrIf(res1.At(0).Value().GetMask() != res2.At(0).Value().GetMask(), res1.At(0).Value().GetMED() != res2.At(0).Value().GetMED())),
                        true,
                        If<bool>(
                            res1.Length() >= (BigInteger)2,
                            If<bool>(
                                Utils.OrIf(res1.At(1).Value().GetPrefix() != res2.At(1).Value().GetPrefix(), Utils.OrIf(res1.At(1).Value().GetMask() != res2.At(1).Value().GetMask(), res1.At(1).Value().GetMED() != res2.At(1).Value().GetMED())),
                                true,
                                If<bool>(
                                    res1.Length() >= (BigInteger)3,
                                    If<bool>(
                                        Utils.OrIf(res1.At(2).Value().GetPrefix() != res2.At(2).Value().GetPrefix(), Utils.OrIf(res1.At(2).Value().GetMask() != res2.At(2).Value().GetMask(), res1.At(2).Value().GetMED() != res2.At(2).Value().GetMED())),
                                        true,
                                        false
                                    ),
                                    false
                                )
                            ),
                            false
                        )
                    ),
                    false
                )
            );
        }



        /// <summary>
        /// Checks whether aggregation results different for two sets of route advertisements with one route in common
        /// </summary>
        /// <param name="rt">route aggregation configuration</param>
        /// <param name="ipa1">first route advertisement, this is common in both sets</param>
        /// <param name="ipa2">second route advertisement</param>
        /// <param name="ipa3">third route advertisement</param>
        /// <returns>true or false</returns>
        public static Zen<bool> DecisionDiffer2(Zen<Router> rt, Zen<IPAttr> ipa1, Zen<IPAttr> ipa2, Zen<IPAttr> ipa3){
            Zen<FSeq<IPAttr>> res1 = rt.GetRouteAdvertisements(ipa1, ipa2);
            Zen<FSeq<IPAttr>> res2 = rt.GetRouteAdvertisements(ipa1, ipa3);

            return If<bool>(
                res1.Length() != res2.Length(),
                true,
                If<bool>(
                    res1.Length() >= (BigInteger)1,
                    If<bool>(
                        Utils.OrIf(res1.At(0).Value().GetPrefix() != res2.At(0).Value().GetPrefix(), Utils.OrIf(res1.At(0).Value().GetMask() != res2.At(0).Value().GetMask(), res1.At(0).Value().GetMED() != res2.At(0).Value().GetMED())),
                        true,
                        If<bool>(
                            res1.Length() >= (BigInteger)2,
                            If<bool>(
                                Utils.OrIf(res1.At(1).Value().GetPrefix() != res2.At(1).Value().GetPrefix(), Utils.OrIf(res1.At(1).Value().GetMask() != res2.At(1).Value().GetMask(), res1.At(1).Value().GetMED() != res2.At(1).Value().GetMED())),
                                true,
                                If<bool>(
                                    res1.Length() >= (BigInteger)3,
                                    If<bool>(
                                        Utils.OrIf(res1.At(2).Value().GetPrefix() != res2.At(2).Value().GetPrefix(), Utils.OrIf(res1.At(2).Value().GetMask() != res2.At(2).Value().GetMask(), res1.At(2).Value().GetMED() != res2.At(2).Value().GetMED())),
                                        true,
                                        false
                                    ),
                                    false
                                )
                            ),
                            false
                        )
                    ),
                    false
                )
            );
        }


        /// <summary>
        /// Compares two routes and returns the best path
        /// </summary>
        /// <param name="rt">the router configuration</param>
        /// <param name="r1">the first input route</param>
        /// <param name="r2">the second input route</param>
        /// <returns>the best path</returns>
        public static Zen<RoutesForDecisionProcess> PathSelection(Zen<Router> rt, Zen<RoutesForDecisionProcess> r1, Zen<RoutesForDecisionProcess> r2){
            var arrtime1 = r1.GetArrTime();
            var arrtime2 = r2.GetArrTime();
            
            var ngbr1 = r1.GetNgbr();
            var ngbr2 = r2.GetNgbr();

            var rid1 = r1.GetRID();
            var rid2 = r2.GetRID();

            var igp1 = r1.GetIGP();
            var igp2 = r2.GetIGP();

            var asn1 = r1.GetASN();
            var asn2 = r2.GetASN();
            var rt_asn = rt.GetAS();

            var med1 = r1.GetMED();
            var med2 = r2.GetMED();

            var org1 = r1.GetOrigin();
            var org2 = r2.GetOrigin();

            var asp1 = r1.GetASPLen();
            var asp2 = r2.GetASPLen();

            var lp1 = r1.GetLP();
            var lp2 = r2.GetLP();
            // JIndegi to snati ble kchu nai

            r1 = If(
                asn1 != rt_asn,
                RoutesForDecisionProcess.Create(100, asp1, org1, med1, asn1, igp1, rid1, ngbr1, arrtime1),
                r1
            );

            r2 = If(
                asn2 != rt_asn,
                RoutesForDecisionProcess.Create(100, asp2, org2, med2, asn2, igp2, rid2, ngbr2, arrtime2),
                r2
            );

            arrtime1 = r1.GetArrTime();
            arrtime2 = r2.GetArrTime();
            
            ngbr1 = r1.GetNgbr();
            ngbr2 = r2.GetNgbr();

            rid1 = r1.GetRID();
            rid2 = r2.GetRID();

            igp1 = r1.GetIGP();
            igp2 = r2.GetIGP();

            asn1 = r1.GetASN();
            asn2 = r2.GetASN();
            rt_asn = rt.GetAS();

            med1 = r1.GetMED();
            med2 = r2.GetMED();

            org1 = r1.GetOrigin();
            org2 = r2.GetOrigin();

            asp1 = r1.GetASPLen();
            asp2 = r2.GetASPLen();

            lp1 = r1.GetLP();
            lp2 = r2.GetLP();
            
            var expr1 = If(arrtime1 <= arrtime2, r1, r2);
            var expr2 = If(ngbr1 < ngbr2, r1, If(ngbr1 > ngbr2, r2, expr1));
            var expr3 = If(rid1 < rid2, r1, If(rid1 > rid2,  r2, expr2));
            var expr4 = If(igp1 < igp2, r1, If(igp1 > igp2, r1, expr3));
            var expr5 = If(And(asn1!=asn2, asn1==rt_asn), r2, If(And(asn1!=asn2, asn2==rt_asn),  r1, expr4));
            var expr6 = If(med1 < med2, r1, If(med1 > med2, r2, expr5));
            var expr7 = If(And(org1!=org2, org1=='i'), r1, If(And(org1!=org2, org2=='i'), r2, expr6));
            var expr8 = If(asp1 < asp2, r1, If(asp1 > asp2, r2, expr7));
            var expr9 = If(lp1 > lp2, r1, If(lp1 < lp2, r2, expr8));

            return expr9;
        }
    }
}