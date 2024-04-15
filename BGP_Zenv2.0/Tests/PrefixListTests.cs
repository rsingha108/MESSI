using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using ZenLib;
using static ZenLib.Zen;
using BGP;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;

namespace Tests{
    [TestClass]
    public class PrefixListTests{
        [TestMethod]
        public void PrefixListEntryValidityTests(){
            var f = new ZenFunction<PrefixListEntry, bool>(PrefixListEntryExtensions.IsValidPrefixListEntry);

            var pr1 = new PrefixListEntry{
                Prefix = 1671377731,
                Mask = 4290772992,
                LE = Option.None<uint>(),
                GE = Option.None<uint>(),
                Permit = false
            };

            var pr2 = new PrefixListEntry{
                Prefix = 1671377732,
                Mask = 4290772994,
                LE = Option.None<uint>(),
                GE = Option.None<uint>(),
                Permit = false
            };

            var pr3 = new PrefixListEntry{
                Prefix = 1671377732,
                Mask = 4290772992,
                LE = Option.Some(4290772993),
                GE = Option.None<uint>(),
                Permit = false
            };

            var pr4 = new PrefixListEntry{
                Prefix = 1671377732,
                Mask = 4290772992,
                LE = Option.Some(4290772992),
                GE = Option.None<uint>(),
                Permit = false
            };

            var pr5 = new PrefixListEntry{
                Prefix = 1671377732,
                Mask = 4290772992,
                LE = Option.Some(4286578688),
                GE = Option.None<uint>(),
                Permit = false
            };

            var pr6 = new PrefixListEntry{
                Prefix = 1671377732,
                Mask = 4290772992,
                LE = Option.Some(4286578688),
                GE = Option.Some(4290772992),
                Permit = false
            };

            var pr7 = new PrefixListEntry{
                Prefix = 1679687938,
                Mask = 4286578688,
                GE = Option.Some(4286578688),
                LE = Option.Some(4290772992),
                Permit = true
            };


            var pr8 = new PrefixListEntry{
                Prefix = 0,
                Mask = 0,
                GE = Option.Some((uint)0),
                LE = Option.Some(4294967295),
                Any = true,
                Permit = false
            };

            var pr9 = new PrefixListEntry{
                Prefix = 1671377733,
                Mask = 0,
                GE = Option.Some((uint)0),
                LE = Option.Some(4294967295),
                Any = true,
                Permit = true
            };

            Assert.IsFalse(f.Evaluate(pr1));
            Assert.IsFalse(f.Evaluate(pr2));
            Assert.IsFalse(f.Evaluate(pr3));
            Assert.IsTrue(f.Evaluate(pr4));
            Assert.IsFalse(f.Evaluate(pr5));
            Assert.IsFalse(f.Evaluate(pr6));
            Assert.IsTrue(f.Evaluate(pr7));
            Assert.IsTrue(f.Evaluate(pr8));
            Assert.IsFalse(f.Evaluate(pr9));
        }

        [TestMethod]
        public void PrefixListValidityTests(){
            var f = new ZenFunction<PrefixList, int, bool>(PrefixListExtensions.IsValidPrefixList);

            var masks = new List<uint>();
            uint n = 0;
            masks.Add(n);
            for(int i=0;i<32;i++){
                n |= ((uint)1) << (31-i);
                masks.Add(n);
            }

            var pr1 = new PrefixListEntry{
                Prefix = 1671377754,
                Mask = masks[8],
                GE = Option.Some(masks[16]),
                LE = Option.Some(masks[24]),
                Any = false,
                Permit = true
            };

            var pr2 = new PrefixListEntry{
                Prefix = 1671377756,
                Mask = masks[10],
                GE = Option.Some(masks[12]),
                LE = Option.Some(masks[17]),
                Any = false,
                Permit = false
            };

            var pr3 = new PrefixListEntry{
                Prefix = 1671377757,
                Mask = masks[2],
                GE = Option.Some(masks[10]),
                LE = Option.Some(masks[20]),
                Any = false,
                Permit = true
            };

            var pr4 = new PrefixListEntry{
                Prefix = (uint)0,
                Mask = masks[0],
                GE = Option.Some(masks[0]),
                LE = Option.Some(masks[32]),
                Any = true,
                Permit = false
            };

            var prlist = new PrefixList{
                Value = new Array<PrefixListEntry, _3>(pr1, pr2, pr3)
            };

            var prlist2 = new PrefixList{
                Value = new Array<PrefixListEntry, _3>(pr1, pr4, pr4)
            };

            var prlist3 = new PrefixList{
                Value = new Array<PrefixListEntry, _3>(pr1, pr4, pr3)
            };

            Assert.IsTrue(f.Evaluate(prlist, 3));
            Assert.IsFalse(f.Evaluate(prlist, 1));
            Assert.IsTrue(f.Evaluate(prlist2, 1));
            Assert.IsFalse(f.Evaluate(prlist2, 3));
            Assert.IsFalse(f.Evaluate(prlist3, 1));
            Assert.IsTrue(f.Evaluate(prlist3, 3));
        }

        [TestMethod]
        public void MatchPrefixListEntryTests(){
            var f = new ZenFunction<PrefixListEntry, IPAttr, bool>(PrefixListEntryExtensions.MatchAgainstPrefix);

            var masks = new List<uint>();
            uint n = 0;
            masks.Add(n);
            for(int i=0;i<32;i++){
                n |= ((uint)1) << (31-i);
                masks.Add(n);
            }

            var pr1 = new PrefixListEntry{
                Prefix = 1671377754,
                Mask = masks[8],
                GE = Option.Some(masks[16]),
                LE = Option.Some(masks[24]),
                Any = false,
                Permit = true
            };

            var pr2 = new PrefixListEntry{
                Prefix = 1671377754,
                Mask = masks[8],
                GE = Option.None<uint>(),
                LE = Option.None<uint>(),
                Any = false,
                Permit = true
            };

            var pr3 = new PrefixListEntry{
                Prefix = 1671377754,
                Mask = masks[8],
                GE = Option.None<uint>(),
                LE = Option.Some(masks[24]),
                Any = false,
                Permit = true
            };

            var pr4 = new PrefixListEntry{
                Prefix = 1671377754,
                Mask = masks[8],
                GE = Option.Some(masks[16]),
                LE = Option.None<uint>(),
                Any = false,
                Permit = true
            };

            var ipa1 = new IPAttr{
                Prefix = 1671377775,
                Mask = masks[10]
            };

            var ipa2 = new IPAttr{
                Prefix = 1671377775,
                Mask = masks[18]
            };

            Assert.IsFalse(f.Evaluate(pr1, ipa1));
            Assert.IsTrue(f.Evaluate(pr1, ipa2));

            Assert.IsFalse(f.Evaluate(pr2, ipa1));
            Assert.IsFalse(f.Evaluate(pr2, ipa2));

            Assert.IsTrue(f.Evaluate(pr3, ipa1));
            Assert.IsTrue(f.Evaluate(pr3, ipa2));

            Assert.IsFalse(f.Evaluate(pr4, ipa1));
            Assert.IsTrue(f.Evaluate(pr4, ipa2));
        }

        [TestMethod]
        public void PrefixListDifferenceTests(){
            var f = new ZenFunction<Option<PrefixList>, Option<PrefixList>, int>(PrefixListExtensions.GetDifference);

            var masks = new List<uint>();
            uint n = 0;
            masks.Add(n);
            for(int i=0;i<32;i++){
                n |= ((uint)1) << (31-i);
                masks.Add(n);
            }
            
            var pr1 = new PrefixListEntry{
                Prefix = 1671377754,
                Mask = masks[8],
                GE = Option.Some(masks[16]),
                LE = Option.Some(masks[24]),
                Any = false,
                Permit = true
            };

            var pr1a = new PrefixListEntry{
                Prefix = 1671377754,
                Mask = masks[8],
                GE = Option.Some(masks[16]),
                LE = Option.Some(masks[24]),
                Any = false,
                Permit = false
            };

            var pr1b = new PrefixListEntry{
                Prefix = 1671377754,
                Mask = masks[8],
                GE = Option.Some(masks[16]),
                LE = Option.Some(masks[25]),
                Any = false,
                Permit = false
            };

            var pr2 = new PrefixListEntry{
                Prefix = 1671377754,
                Mask = masks[8],
                GE = Option.None<uint>(),
                LE = Option.None<uint>(),
                Any = false,
                Permit = true
            };

            var pr3 = new PrefixListEntry{
                Prefix = 1671377754,
                Mask = masks[8],
                GE = Option.None<uint>(),
                LE = Option.Some(masks[24]),
                Any = false,
                Permit = true
            };

            var pr4 = new PrefixListEntry{
                Prefix = 0,
                Mask = masks[0],
                GE = Option.Some(masks[0]),
                LE = Option.Some(masks[0]),
                Any = true,
                Permit = true
            };


            var pr4b = new PrefixListEntry{
                Prefix = 0,
                Mask = masks[0],
                GE = Option.Some(masks[0]),
                LE = Option.Some(masks[0]),
                Any = true,
                Permit = false
            };

            var prl1 = new PrefixList{
                Value = new Array<PrefixListEntry, _3>(pr1, pr2, pr3)
            };

            var prl1a = new PrefixList{
                Value = new Array<PrefixListEntry, _3>(pr1a, pr2, pr3)
            };

            var prl1b = new PrefixList{
                Value = new Array<PrefixListEntry, _3>(pr1b, pr2, pr3)
            };

            var prl2 = new PrefixList{
                Value = new Array<PrefixListEntry, _3>(pr4, pr2, pr3)
            };

            var prl2b = new PrefixList{
                Value = new Array<PrefixListEntry, _3>(pr4b, pr2, pr3)
            };

            var prl3 = new PrefixList{
                Value = new Array<PrefixListEntry, _3>(pr1b, pr1, pr3)
            };

            Assert.AreEqual(f.Evaluate(Option.None<PrefixList>(), Option.None<PrefixList>()), 0);
            Assert.AreEqual(f.Evaluate(Option.Some(prl1), Option.None<PrefixList>()), Constants.INF);
            Assert.AreEqual(f.Evaluate(Option.None<PrefixList>(), Option.Some(prl1)), Constants.INF);
            Assert.AreEqual(f.Evaluate(Option.Some(prl1), Option.Some(prl1)), 0);
            Assert.AreEqual(f.Evaluate(Option.Some(prl1a), Option.Some(prl1)), 1);
            Assert.AreEqual(f.Evaluate(Option.Some(prl1), Option.Some(prl1b)), 2);
            Assert.AreEqual(f.Evaluate(Option.Some(prl1), Option.Some(prl2)), 1);
            Assert.AreEqual(f.Evaluate(Option.Some(prl2), Option.Some(prl2b)), 1);
            Assert.AreEqual(f.Evaluate(Option.Some(prl1), Option.Some(prl3)), 3);
        }

        [TestMethod]
        public void PrefixListMatchTests(){
            var f = new ZenFunction<PrefixList, IPAttr, bool>(PrefixListExtensions.MatchAgainstPrefixList);

            var masks = new List<uint>();
            uint n = 0;
            masks.Add(n);
            for(int i=0;i<32;i++){
                n |= ((uint)1) << (31-i);
                masks.Add(n);
            }

            int[] t1 = {240, 0, 0, 0};
            var pr1 = new PrefixListEntry{
                Prefix = Utils.PrefixToUint(t1),
                Mask = masks[8],
                GE = Option.None<uint>(),
                LE = Option.Some<uint>(masks[32]),
                Permit = false,
            };

            int[] t2 = {172, 16, 0, 0};
            var pr2 = new PrefixListEntry{
                Prefix = Utils.PrefixToUint(t2),
                Mask = masks[16],
                GE = Option.Some<uint>(masks[20]),
                LE = Option.Some<uint>(masks[28]),
                Permit = false
            };

            var pr3 = new PrefixListEntry{
                Prefix = 0,
                Mask = masks[0],
                GE = Option.None<uint>(),
                LE = Option.Some<uint>(masks[32]),
                Permit = true
            };
            

            var pr_list = new PrefixList{
                Value = new Array<PrefixListEntry, _3>(pr1, pr2, pr3)
            };


            int[] t = {240, 20, 0, 0};
            var ip1 = new IPAttr{
                Prefix = Utils.PrefixToUint(t),
                Mask = masks[16]
            };

            int[] t3 = {172, 16, 2, 0};
            var ip2 = new IPAttr{
                Prefix = Utils.PrefixToUint(t3),
                Mask = masks[20]
            };

            int[] t4 = {100, 0, 0, 0};
            var ip3 = new IPAttr{
                Prefix = Utils.PrefixToUint(t4),
                Mask = masks[20]
            };

            Assert.IsFalse(f.Evaluate(pr_list, ip1));
            Assert.IsFalse(f.Evaluate(pr_list, ip2));
            Assert.IsTrue(f.Evaluate(pr_list, ip3));
        }
    }
}
