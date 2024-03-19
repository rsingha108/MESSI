using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using ZenLib;
using static ZenLib.Zen;
using BGP;

namespace Tests{
    [TestClass]
    public class OrAndTests{
        [TestMethod]
        public void AndIfTests(){
            var f = new ZenFunction<bool, bool, bool>(Utils.AndIf);

            Assert.IsFalse(f.Evaluate(false, false));
            Assert.IsTrue(f.Evaluate(true, true));
            Assert.IsFalse(f.Evaluate(false, true));
            Assert.IsFalse(f.Evaluate(true, false));
        }

        [TestMethod]
        public void OrIfTests(){
            var f = new ZenFunction<bool, bool, bool>(Utils.OrIf);

            Assert.IsTrue(f.Evaluate(true, true));
            Assert.IsTrue(f.Evaluate(true, false));
            Assert.IsTrue(f.Evaluate(false, true));
            Assert.IsFalse(f.Evaluate(false, false));
        }
    }
}