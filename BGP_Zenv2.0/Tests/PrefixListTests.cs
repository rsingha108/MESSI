using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using ZenLib;
using static ZenLib.Zen;
using BGP;

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

            Assert.IsFalse(f.Evaluate(pr1));
            Assert.IsFalse(f.Evaluate(pr2));
            Assert.IsFalse(f.Evaluate(pr3));
            Assert.IsTrue(f.Evaluate(pr4));
            Assert.IsFalse(f.Evaluate(pr5));
            Assert.IsFalse(f.Evaluate(pr6));
            Assert.IsTrue(f.Evaluate(pr7));
        }

        [TestMethod]
        public void PrefixListEntryMatchTests(){
            var f = new ZenFunction<PrefixListEntry, IPAttr, bool>(PrefixListEntryExtensions.MatchAgainstPrefix);

            var pr1 = new PrefixListEntry{
                Prefix = 1679687938,
                Mask = 4294901760, // 16
                GE = Option.Some(4294963200), // 20
                LE = Option.Some(4294967280), // 28
                Permit = true
            };

            var pr2 = new PrefixListEntry{
                Prefix = 1679687938,
                Mask = 4294966272, // 22
                GE = Option.None<uint>(),
                LE = Option.None<uint>(),
                Permit = true
            };

            var pr3 = new PrefixListEntry{
                Prefix = 352605117,
                Mask = 2155986972,
                LE = Option.None<uint>(),
                GE = Option.Some(3758096377),
                Permit = true
            };

            var ipa1 = new IPAttr{
                Prefix = 1679687938,
                Mask = 4294966272, // 22
                LP = 0,
                MED = 0
            };

            var ipa2 = new IPAttr{
                Prefix = 1679647938,
                Mask = 4294966272, // 22
                LP = 0,
                MED = 0
            };

            var ipa3 = new IPAttr{
                Prefix = 1679687938,
                Mask = 4294901760,
                LP = 0,
                MED = 0
            };

            var ipa4 = new IPAttr{
                Prefix = 1679584254,
                Mask = 4294967280,
                LP = 132,
                MED = 4
            };

            Assert.IsTrue(f.Evaluate(pr1, ipa1));
            Assert.IsFalse(f.Evaluate(pr1, ipa2));
            Assert.IsTrue(f.Evaluate(pr2, ipa1));
            Assert.IsFalse(f.Evaluate(pr2, ipa2));
            Assert.IsFalse(f.Evaluate(pr2, ipa3));
        }
    }
}
