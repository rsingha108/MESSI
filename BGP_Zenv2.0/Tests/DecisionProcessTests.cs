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

namespace Tests
{
    [TestClass]
    public class DecisionProcessTests
    {

        [TestMethod]
        public void TestDecisionProcess()
        {
            var f = new ZenFunction<Router, RoutesForDecisionProcess, RoutesForDecisionProcess, RoutesForDecisionProcess>(RouterExtensions.PathSelection);

            var rt = new Router();
            rt.AS = 100;

            int[] ip1 = {1, 0, 0, 1};
            int[] ip2 = {2, 0, 0, 2};

            var r1 = new RoutesForDecisionProcess{
                LP = 100,
                ASPathLength = 3,
                Origin = 'i',
                MED = 5,
                ASN = 100,
                IGP = 0,
                RID = Utils.PrefixToUint(ip1),
                NeighborAddr = Utils.PrefixToUint(ip2),
                ArrivalTime = 1
            };

            var r2 = new RoutesForDecisionProcess{
                LP = 100,
                ASPathLength = 3,
                Origin = 'i',
                MED = 5,
                ASN = 200,
                IGP = 0,
                RID = Utils.PrefixToUint(ip1),
                NeighborAddr = Utils.PrefixToUint(ip2),
                ArrivalTime = 1
            };

            var r3 = new RoutesForDecisionProcess{
                LP = 100,
                ASPathLength = 3,
                Origin = 'i',
                MED = 3,
                ASN = 200,
                IGP = 0,
                RID = Utils.PrefixToUint(ip1),
                NeighborAddr = Utils.PrefixToUint(ip2),
                ArrivalTime = 1
            };

            var r4 = new RoutesForDecisionProcess{
                LP = 100,
                ASPathLength = 1,
                Origin = 'i',
                MED = 5,
                ASN = 100,
                IGP = 0,
                RID = Utils.PrefixToUint(ip1),
                NeighborAddr = Utils.PrefixToUint(ip2),
                ArrivalTime = 1
            };

            Assert.IsTrue(f.Evaluate(rt, r1, r2).ASN == r2.ASN);
            Assert.IsTrue(f.Evaluate(rt, r1, r3).MED == r3.MED);
            Assert.IsTrue(f.Evaluate(rt, r1, r4) == r4);

        }
    }
} 
