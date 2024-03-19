using System;
using System.Collections.Generic;
using System.Numerics;
using ZenLib;
using static ZenLib.Zen;

namespace BGP{
    public class Router{
        public IPAttr AggregateRoute;
        public bool SummaryOnly;
        public bool MatchingMEDOnly;
        public uint AS;

        public static Zen<Router> Create(Zen<IPAttr> aggregateroute, Zen<bool> summary, Zen<bool> match_med, Zen<uint> asnum){
            return Zen.Create<Router>(
                ("AggregateRoute", aggregateroute),
                ("SummaryOnly", summary),
                ("MatchingMEDOnly", match_med),
                ("AS", asnum)
            );
        }

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
        public static Zen<IPAttr> GetAggRoute(this Zen<Router> rt) => rt.GetField<Router, IPAttr>("AggregateRoute");
        public static Zen<bool> GetSummary(this Zen<Router> rt)=> rt.GetField<Router, bool>("SummaryOnly");
        public static Zen<bool> GetMatchingMEDOnly(this Zen<Router> rt)=> rt.GetField<Router, bool>("MatchingMEDOnly");
        public static Zen<bool> GetAS(this Zen<Router> rt)=> rt.GetField<Router, bool>("AS");

        public static Zen<bool> IsValidRouterConfig(this Zen<Router> rt){
            var aggroute = rt.GetAggRoute();
            return And(
                aggroute.GetPrefix() >= 1671377732,
                aggroute.GetPrefix() <= 1679687938,

                Or(
                    //aggroute.GetMask() == 0,            // 0
                    aggroute.GetMask() == 2147483648,   // 1
                    aggroute.GetMask() == 3221225472,   // 2
                    aggroute.GetMask() == 3758096384,   // 3
                    aggroute.GetMask() == 4026531840,   // 4
                    aggroute.GetMask() == 4160749568,   // 5
                    aggroute.GetMask() == 4227858432,   // 6
                    aggroute.GetMask() == 4261412864,   // 7
                    aggroute.GetMask() == 4278190080,   // 8
                    aggroute.GetMask() == 4286578688,   // 9
                    aggroute.GetMask() == 4290772992,   // 10
                    aggroute.GetMask() == 4292870144,   // 11
                    aggroute.GetMask() == 4293918720,   // 12
                    aggroute.GetMask() == 4294443008,   // 13
                    aggroute.GetMask() == 4294705152,   // 14
                    aggroute.GetMask() == 4294836224,   // 15
                    aggroute.GetMask() == 4294901760,   // 16
                    aggroute.GetMask() == 4294934528,   // 17
                    aggroute.GetMask() == 4294950912,   // 18
                    aggroute.GetMask() == 4294959104,   // 19
                    aggroute.GetMask() == 4294963200,   // 20
                    aggroute.GetMask() == 4294965248,   // 21
                    aggroute.GetMask() == 4294966272,   // 22
                    aggroute.GetMask() == 4294966784,   // 23
                    aggroute.GetMask() == 4294967040,   // 24
                    aggroute.GetMask() == 4294967168,   // 25
                    aggroute.GetMask() == 4294967232,   // 26
                    aggroute.GetMask() == 4294967264,   // 27
                    aggroute.GetMask() == 4294967280,   // 28
                    aggroute.GetMask() == 4294967288,   // 29
                    aggroute.GetMask() == 4294967292,   // 30
                    aggroute.GetMask() == 4294967294,   // 31
                    aggroute.GetMask() == 4294967295    // 32
                ),

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

                aggroute.GetLP() == 0,
                aggroute.GetMED() == 0,
                aggroute.GetCommunityAsList().Length() == (BigInteger)0,
                aggroute.GetASPathAsList().Length() == (BigInteger)0
            );
        }

        public static Zen<FSeq<IPAttr>> GetRouteAdvertisements1(this Zen<Router> rt, Zen<IPAttr> ipa1, Zen<IPAttr> ipa2){
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


            var match_med = If<bool>(
                Utils.AndIf(r1, r2),
                If(
                    rt.GetMatchingMEDOnly(),
                    If<bool>(
                        ipa1.GetMED() == ipa2.GetMED(),
                        true,
                        false
                    ),
                    true
                ),
                true
            );

            res = If(
                r1,
                If(
                    rt.GetSummary(),
                    res,
                    res.AddBack(ipa1)
                ),
                res.AddBack(ipa1)
            );


            res = If(
                r2,
                If(
                    rt.GetSummary(),
                    res,
                    res.AddBack(ipa2)
                ),
                res.AddBack(ipa2)
            );

            res = If(
                Utils.AndIf(
                    Utils.OrIf(r1, r2),
                    match_med
                ),
                res.AddBack(aggrt),
                res
            );

            res = If(
                Utils.AndIf(
                    r1,
                    Utils.AndIf(
                        r2,
                        Utils.AndIf(
                            rt.GetSummary(),
                            Utils.AndIf(
                                rt.GetMatchingMEDOnly(),
                                ipa1.GetMED() == ipa2.GetMED()
                            )
                        )
                    )
                ),
                res.AddBack(ipa1),
                res
            );

            res = If(
                Utils.AndIf(
                    r1,
                    Utils.AndIf(
                        r2,
                        Utils.AndIf(
                            Utils.AndIf(
                                rt.GetMatchingMEDOnly(),
                                ipa1.GetMED() == ipa2.GetMED()
                            ),
                            rt.GetSummary()
                        )
                    )
                ),
                res.AddBack(ipa2),
                res
            );

            return res;
        }

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
    }
}