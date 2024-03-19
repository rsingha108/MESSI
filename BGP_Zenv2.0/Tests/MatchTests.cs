using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using ZenLib;
using static ZenLib.Zen;
using BGP;

namespace Tests{
    [TestClass]
    public class MatchTests{
        /*[TestMethod]
        public void MatchAgainstClauseTests(){
            var f = new ZenFunction<MatchClause, IPAttr, bool>(MatchClauseExtensions.MatchAgainstClause);

            var r1 = f.Evaluate(new MatchClause{LP = Option.Some<uint>(120)}, new IPAttr{LP = 120});
            Assert.IsTrue(r1);

            var r2 = f.Evaluate(new MatchClause{LP = Option.Some<uint>(120)}, new IPAttr{LP = 130});
            Assert.IsFalse(r2);

        }*/
    } 
}