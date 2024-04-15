using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZenLib;
using static ZenLib.Zen;
using BGP;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;


namespace Tests{
    [TestClass]
    public class AggregationTests{

        [TestMethod]
        public void TestRouteAggregation(){
            var f = new ZenFunction<Router, IPAttr, IPAttr, FSeq<IPAttr>>(RouterExtensions.GetRouteAdvertisements);

            var masks = new List<uint>();
            uint n = 0;
            masks.Add(n);
            for(int i=0;i<32;i++){
                n |= ((uint)1) << (31-i);
                masks.Add(n);
            }

            int[] t1 = {240, 0, 0, 0};
            var agg_route = new IPAttr{
                Prefix = Utils.PrefixToUint(t1),
                Mask = masks[8]
            };

            var rt = new Router{
                AggregateRoute = agg_route,
                SummaryOnly = false,
                MatchingMEDOnly = true
            };

            int[] t2 = {240, 10, 0, 0};
            int[] t3 = {240, 20, 20, 0};
            int[] t4 = {240, 30, 192, 0};

            var ip1 = new IPAttr{
                Prefix = Utils.PrefixToUint(t2),
                Mask = masks[16],
                MED = 5
            };

            var ip2 = new IPAttr{
                Prefix = Utils.PrefixToUint(t3),
                Mask = masks[24],
                MED = 3
            };

            var ip3 = new IPAttr{
                Prefix = Utils.PrefixToUint(t4),
                Mask = masks[18],
                MED = 5
            };

            var res = f.Evaluate(rt, ip1, ip2).ToList();

            Assert.IsTrue(res.Count() == 2);
            Assert.IsTrue(res.Contains(ip1));
            Assert.IsTrue(res.Contains(ip2));

            var res2 = f.Evaluate(rt, ip1, ip3).ToList();

            Assert.IsTrue(res2.Count() == 3);
            Assert.IsTrue(res2.Contains(agg_route));
            Assert.IsTrue(res2.Contains(ip1));
            Assert.IsTrue(res2.Contains(ip3));

            var rt2 = new Router{
                AggregateRoute = agg_route,
                SummaryOnly = true,
                MatchingMEDOnly = true
            };

            var res3 = f.Evaluate(rt2, ip1, ip3).ToList();

            Assert.IsTrue(res3.Count() == 1);
            Assert.IsTrue(res3.Contains(agg_route));
        }

        [TestMethod]
        public void TestRouterDifference(){
            var f = new ZenFunction<Router, Router, int>(RouterExtensions.RouterDifferences);

            var masks = new List<uint>();
            uint n = 0;
            masks.Add(n);
            for(int i=0;i<32;i++){
                n |= ((uint)1) << (31-i);
                masks.Add(n);
            }

            int[] t1 = {240, 0, 0, 0};
            var agg_rt1 = new IPAttr{
                Prefix = Utils.PrefixToUint(t1),
                Mask = masks[8]
            };
            var rt1 = new Router{
                AggregateRoute = agg_rt1,
                SummaryOnly = true,
                MatchingMEDOnly = false
            };

            int[] t2 = {100, 10, 0, 0};
            var agg_rt2 = new IPAttr{
                Prefix = Utils.PrefixToUint(t2),
                Mask = masks[16]
            };
            var rt2 = new Router{
                AggregateRoute =  agg_rt2,
                SummaryOnly = false,
                MatchingMEDOnly = true
            };

            var res = f.Evaluate(rt1, rt2);

            Assert.IsTrue(res == 3);
        }

        [TestMethod]
        public void TestDecisionDiffer(){
            var f = new ZenFunction<Router, Router, IPAttr, IPAttr, bool>(RouterExtensions.DecisionDiffer);
            
            var masks = new List<uint>();
            uint n = 0;
            masks.Add(n);
            for(int i=0;i<32;i++){
                n |= ((uint)1) << (31-i);
                masks.Add(n);
            }

            int[] t1 = {240, 0, 0, 0};
            var agg_rt1 = new IPAttr{
                Prefix = Utils.PrefixToUint(t1),
                Mask = masks[8]
            };

            // Router 1
            var rt1 = new Router{
                AggregateRoute = agg_rt1,
                SummaryOnly = true,
                MatchingMEDOnly = false
            };

            // Router 2
            var rt2 = new Router{
                AggregateRoute =  agg_rt1,
                SummaryOnly = false,
                MatchingMEDOnly = false
            };

            // IP 1
            int[] t3 = {240, 20, 0, 0};
            var ipa1 = new IPAttr{
                Prefix = Utils.PrefixToUint(t3),
                Mask = masks[16],
                MED = 4
            };

            // IP 2
            int[] t4 = {240,  20, 8, 0};
            var ipa2 = new IPAttr{
                Prefix = Utils.PrefixToUint(t4),
                Mask = masks[24],
                MED = 5
            };

            var res = f.Evaluate(rt1, rt2, ipa1, ipa2);
            Assert.IsTrue(res);

            // Router 3
            var rt3 = new Router{
                AggregateRoute = agg_rt1,
                SummaryOnly = true,
                MatchingMEDOnly = true
            };

            // Router 4
            var rt4 = new Router{
                AggregateRoute = agg_rt1,
                SummaryOnly = false,
                MatchingMEDOnly = true
            };

            var res2 = f.Evaluate(rt3, rt4, ipa1, ipa2);
            Assert.IsFalse(res2);
        }

        [TestMethod]
        public void TestDecisionDiffer2(){
            var f = new ZenFunction<Router, IPAttr, IPAttr, IPAttr, bool>(RouterExtensions.DecisionDiffer2);

             var masks = new List<uint>();
            uint n = 0;
            masks.Add(n);
            for(int i=0;i<32;i++){
                n |= ((uint)1) << (31-i);
                masks.Add(n);
            }

            int[] t1 = {240, 0, 0, 0};
            var agg_rt1 = new IPAttr{
                Prefix = Utils.PrefixToUint(t1),
                Mask = masks[8]
            };

            // Router 1
            var rt1 = new Router{
                AggregateRoute = agg_rt1,
                SummaryOnly = true,
                MatchingMEDOnly = true
            };

            // IP 1
            int[] t3 = {240, 20, 0, 0};
            var ipa1 = new IPAttr{
                Prefix = Utils.PrefixToUint(t3),
                Mask = masks[16],
                MED = 4
            };

            // IP 2
            int[] t4 = {240, 20, 8, 0};
            var ipa2 = new IPAttr{
                Prefix = Utils.PrefixToUint(t4),
                Mask = masks[24],
                MED = 5
            };

            // IP 3
            int[] t5 = {240, 20, 8, 0};
            var ipa3 = new IPAttr{
                Prefix = Utils.PrefixToUint(t4),
                Mask = masks[24],
                MED = 4
            };

            var res = f.Evaluate(rt1, ipa1, ipa2, ipa3);

            Assert.IsTrue(res);
        }
    }
}