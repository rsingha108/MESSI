using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using ZenLib;
using static ZenLib.Zen;
using BGP;

namespace Tests{
    [TestClass]
    public class MatchTests{
        [TestMethod]
        public void MatchAgainstClauseTests(){
            var f = new ZenFunction<MatchClause, IPAttr, Pair<string, bool>>(MatchClauseExtensions.MatchAgainstClause);

            var r1 = f.Evaluate(new MatchClause{LP = Option.Some<uint>(120)}, new IPAttr{LP = 120});
            Assert.IsTrue(r1.Item2);
            Assert.IsTrue(r1.Item1 == "L1");

            var r2 = f.Evaluate(new MatchClause{LP = Option.Some<uint>(120)}, new IPAttr{LP = 130});
            Assert.IsFalse(r2.Item2);
            Assert.IsTrue(r2.Item1 == "L2");

            var r3 = f.Evaluate(
                new MatchClause{
                    LP = Option.Some<uint>(120),
                    MED = Option.Some<uint>(30)
                },
                new IPAttr{
                    LP = 120,
                    MED = 30
                }
            );
            Assert.IsTrue(r3.Item2);
            Assert.IsTrue(r3.Item1 == "L1M1");


            var r4 = f.Evaluate(
                new MatchClause{
                    LP = Option.Some<uint>(120),
                    MED = Option.Some<uint>(30)
                },
                new IPAttr{
                    LP = 120,
                    MED = 40
                }
            );
            Assert.IsFalse(r4.Item2);
            Assert.IsTrue(r4.Item1 == "L1M2");
        }

        [TestMethod]
        public void MatchClauseDifferenceTests(){
            var f = new ZenFunction<MatchClause, MatchClause, int>(MatchClauseExtensions.GetAttrDifference);

            var m1 = new MatchClause{
                LP = Option.Some<uint>(120),
                MED = Option.None<uint>(),
                PrList = Option.None<PrefixList>(),
                ComList = Option.None<CommunityListEntry>(),
                ASPathList = Option.None<ASPathListEntry>()
            };
            var m2 = new MatchClause{
                LP = Option.None<uint>(),
                MED = Option.Some<uint>(30),
                PrList = Option.None<PrefixList>(),
                ComList = Option.None<CommunityListEntry>(),
                ASPathList = Option.None<ASPathListEntry>()
            };
            var m3 = new MatchClause{
                LP = Option.Some<uint>(100),
                MED = Option.None<uint>(),
                PrList = Option.None<PrefixList>(),
                ComList = Option.None<CommunityListEntry>(),
                ASPathList = Option.None<ASPathListEntry>()
            };

            var res = f.Evaluate(m1, m2);
            var res2 = f.Evaluate(m1, m3);

            Assert.IsTrue(res >= Constants.INF);
            Assert.IsTrue(res2 == 1);
        }

        [TestMethod]
        public void TestRouteFilterDynamics(){
            var f = new ZenFunction<RouteMapEntry, RouteMapEntry, IPAttr, bool>(RouteMapEntryExtensions.DecisionDiffer);

            var m1 = new MatchClause{
                LP = Option.Some<uint>(120),
                MED = Option.None<uint>(),
                PrList = Option.None<PrefixList>(),
                ComList = Option.None<CommunityListEntry>(),
                ASPathList = Option.None<ASPathListEntry>()
            };
            var m2 = new MatchClause{
                LP = Option.None<uint>(),
                MED = Option.Some<uint>(30),
                PrList = Option.None<PrefixList>(),
                ComList = Option.None<CommunityListEntry>(),
                ASPathList = Option.None<ASPathListEntry>()
            };

            var rme1 = new RouteMapEntry{
                Permit = true,
                MC = m1,
                SC = Option.None<SetClause>()
            };

            var rme2 = new RouteMapEntry{
                Permit = true,
                MC = m2,
                SC = Option.None<SetClause>()
            };

            var ipa = new IPAttr{
                LP = 120,
                MED = 30
            };

            var res = f.Evaluate(rme1, rme2, ipa);
            Assert.IsFalse(res);

            var ipa2 = new IPAttr{
                LP = 120,
                MED = 50
            };

            var res2 = f.Evaluate(rme1, rme2, ipa2);
            Assert.IsTrue(res2);
        }
    } 
}