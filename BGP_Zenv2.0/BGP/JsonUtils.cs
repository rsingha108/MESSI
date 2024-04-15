using ZenLib;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using static ZenLib.Zen;
using System.Text.Encodings.Web;

namespace BGP{

    public static class JsonUtils{

        public static Dictionary<string, string> RmeToDict(RouteMapEntry rme)
        {
            var m = rme.MC; // match clause
            var s = rme.SC; // set clause
            var rmap = new Dictionary<string, string>() { };

            if(m.PrList.HasValue){
                rmap.Add("Prefix", "Some");
                rmap.Add("Prefix0", Utils.Prefix2String(m.PrList.Value.Value.Get(0).Prefix, m.PrList.Value.Value.Get(0).Mask));
                rmap.Add("LE0",Utils.Mask2String(m.PrList.Value.Value.Get(0).LE));
                rmap.Add("GE0",Utils.Mask2String(m.PrList.Value.Value.Get(0).GE));
                rmap.Add("PrefixPD0", m.PrList.Value.Value.Get(0).Permit.ToString());

                // rmap.Add("Prefix1", Utils.Prefix2String(m.PreList.Value.Value.Get(1).Prefix, m.PreList.Value.Value.Get(1).Mask));
                // rmap.Add("LE1",Utils.Mask2String(m.PreList.Value.Value.Get(1).LE));
                // rmap.Add("GE1",Utils.Mask2String(m.PreList.Value.Value.Get(1).GE));
                // rmap.Add("PrefixPD1", m.PreList.Value.Value.Get(1).Permit.ToString());

                // rmap.Add("Prefix2", Utils.Prefix2String(m.PreList.Value.Value.Get(2).Prefix, m.PreList.Value.Value.Get(2).Mask));
                // rmap.Add("LE2",Utils.Mask2String(m.PreList.Value.Value.Get(2).LE));
                // rmap.Add("GE2",Utils.Mask2String(m.PreList.Value.Value.Get(2).GE));
                // rmap.Add("PrefixPD2", m.PreList.Value.Value.Get(2).Permit.ToString());


            }
            else{
                rmap.Add("Prefix", "None");
                rmap.Add("LE", "None");
                rmap.Add("GE", "None");
                rmap.Add("PrefixPD", "None");
            }
            
            rmap.Add("RmapPD",rme.Permit.ToString());
            rmap.Add("LP", m.LP.HasValue? m.LP.Value.ToString() : "None");
            rmap.Add("MED", m.MED.HasValue? m.MED.Value.ToString() : "None");
            rmap.Add("Community", m.ComList.HasValue? "Some" : "None");
            rmap.Add("Community-regex", m.ComList.HasValue?  m.ComList.Value.RegularExpression.ToString() : "None");
            rmap.Add("Community-permit", m.ComList.HasValue?  m.ComList.Value.Permit.ToString() : "None");
            rmap.Add("ASPath", m.ASPathList.HasValue? "Some" : "None");
            rmap.Add("ASPath-regex", m.ASPathList.HasValue? m.ASPathList.Value.RegularExpression.ToString() : "None");
            rmap.Add("ASPath-permit", m.ASPathList.HasValue? m.ASPathList.Value.Permit.ToString() : "None");
            
            if (s.HasValue){
                rmap.Add("Set", "Some");
                rmap.Add("SetLP", s.Value.LP.HasValue? s.Value.LP.Value.ToString() : "None");
                rmap.Add("SetMED", s.Value.MED.HasValue? s.Value.MED.Value.ToString() : "None");
                rmap.Add("SetCommunity", s.Value.Community.HasValue? SetClause.CommunityInt2Str(s.Value.Community.Value) : "None");
                rmap.Add("DeleteCommunity", s.Value.DeleteCommunity.HasValue? s.Value.DeleteCommunity.Value.Item1 : "None");
                rmap.Add("ASPathPrepend", s.Value.ASPathPrepend.HasValue? s.Value.ASPathPrepend.Value.ToString() : "None");
                rmap.Add("ASPathExclude", s.Value.ASPathExclude.HasValue? s.Value.ASPathExclude.Value.ToString() : "None");
                rmap.Add("NextHopIP", s.Value.NextHopIP.HasValue? SetClause.NextHopIP2Str(s.Value.NextHopIP.Value) : "None");
                rmap.Add("NextHopPeer", s.Value.NextHopPeer.ToString());
                rmap.Add("NextHopUnchanged", s.Value.NextHopUnchanged.ToString());
            }
            else{
                rmap.Add("Set", "None");
                rmap.Add("SetLP", "None");
                rmap.Add("SetMED", "None");
                rmap.Add("SetCommunity", "None");
                rmap.Add("DeleteCommunity", "None");
                rmap.Add("ASPathPrepend", "None");
                rmap.Add("ASPathExclude", "None");
                rmap.Add("NextHopIP", "None");
                rmap.Add("NextHopPeer", "None");
                rmap.Add("NextHopUnchanged", "None");
            }
            return rmap;
        }

        public static Dictionary<string, string> RouteToDict(IPAttr ipa)
        {
            var route = new Dictionary<string, string>() { };
            route.Add("Prefix", Utils.Prefix2String(ipa.Prefix, ipa.Mask));
            route.Add("LP", ipa.LP.ToString());
            route.Add("MED", ipa.MED.ToString());
            route.Add("Community", IPAttr.ToStringCom(ipa.CommunityAsList));
            route.Add("ASPath", IPAttr.ToStringAS(ipa.ASPath));
            route.Add("NextHop", IPAttr.NextHopIP2Str(ipa.NextHop));
            return route;
        }

        public static Dictionary<string, string> RouterToDict(Router rt)
        {             
            var router = new Dictionary<string, string>() { };
            router.Add("AggregateRoute", rt.AggregateRoute.ToString());
            router.Add("SummaryOnly", rt.SummaryOnly.ToString());
            router.Add("MatchingMEDOnly", rt.MatchingMEDOnly.ToString());
            router.Add("AS", rt.AS.ToString());
            return router;
        }

        public static List<Dictionary<string, string>> ListRoutes(FSeq<IPAttr> res)
        {
            var routes = new List<Dictionary<string, string>>();
            foreach (var r in res.ToList())
            {
                var route = RouteToDict(r);
                routes.Add(route);
            }
            return routes;
        }



        public static List<Dictionary<string, string>> CreateJson(RouteMapEntry rme, IPAttr ipa, ZenFunction<RouteMapEntry, IPAttr, Pair<string, bool, Option<IPAttr>>> f, int n_tests) 
        {

            var rmap = RmeToDict(rme);
            var route = RouteToDict(ipa);
            
            var decision = new Dictionary<string, string>() { };
            decision.Add("Tag", f.Evaluate(rme, ipa).Item1.ToString());
            decision.Add("Allowed", f.Evaluate(rme, ipa).Item2.ToString());
            // decision.Add("Rewritten_Packet", f.Evaluate(rme, ipa).Item2.HasValue? f.Evaluate(rme, ipa).Item2.Value.ToString() : "None");
            var r_pkt = f.Evaluate(rme, ipa).Item3;
            decision.Add("Prefix", r_pkt.HasValue? Utils.Prefix2String(r_pkt.Value.Prefix, r_pkt.Value.Mask) : "None");
            decision.Add("LP", r_pkt.HasValue? r_pkt.Value.LP.ToString() : "None");
            decision.Add("MED", r_pkt.HasValue? r_pkt.Value.MED.ToString() : "None");
            decision.Add("Community", r_pkt.HasValue? IPAttr.ToStringCom(r_pkt.Value.CommunityAsList) : "None");
            decision.Add("ASPath", r_pkt.HasValue? IPAttr.ToStringAS(r_pkt.Value.ASPath) : "None");
            decision.Add("NextHop", r_pkt.HasValue? IPAttr.NextHopIP2Str(r_pkt.Value.NextHop) : "None");


            var test = new List<Dictionary<string, string>>(){rmap, route, decision};
            var options = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping};
            FileInfo file = new FileInfo($"../CLI/tests/{n_tests}.json");
            //FileInfo file = new FileInfo($"tests/{n_tests}.json");
            var info = System.Text.Json.JsonSerializer.Serialize(test, options);
            File.WriteAllText(file.FullName, info);
            return test;
        }


        public static void CreateJson(RouteMapEntry rme1, RouteMapEntry rme2, IPAttr ipa, bool dec1, bool dec2, int diff, int n_tests)
        {
            FileInfo file = new FileInfo($"../CLI/tests/{n_tests}.json");
            var result = new{
                Rmap1 = RmeToDict(rme1),
                Rmap2 = RmeToDict(rme2),
                Route = RouteToDict(ipa),
                Decision1 = dec1,
                Decision2 = dec2,
                Difference = diff,
            }; 
            string jsonString = JsonConvert.SerializeObject(result, Formatting.Indented);
            File.WriteAllText(file.FullName, jsonString);
        }

        public static void CreateJson(RouteMapEntry rme1, IPAttr ipa1, IPAttr ipa2, bool dec1, bool dec2, int diff, int n_tests){
            FileInfo file = new FileInfo($"../CLI/tests/{n_tests}.json");
            var result = new{
                Rmap1 = RmeToDict(rme1),
                Route1 = RouteToDict(ipa1),
                Route2 = RouteToDict(ipa2),
                Decision1 = dec1,
                Decision2 = dec2,
                Difference = diff,
            }; 
            string jsonString = JsonConvert.SerializeObject(result, Formatting.Indented);
            File.WriteAllText(file.FullName, jsonString);
        }

        public static void CreateJson(Router rt, RoutesForDecisionProcess route1, RoutesForDecisionProcess route2, RoutesForDecisionProcess dec, int n_tests){
            FileInfo file = new FileInfo($"../CLI/tests/{n_tests}.json");
            var result = new{
                RouterAS = rt.AS,
                Route1 = new{
                    LP = route1.LP,
                    ASPathLength = route1.ASPathLength,
                    Origin = route1.Origin,
                    MED = route1.MED,
                    ASN = route1.ASN,
                    IGP = route1.IGP,
                    RID = Utils.UintToPrefix(route1.RID),
                    NgbrAddr = Utils.UintToPrefix(route1.NeighborAddr),
                    ArrivalTime = route1.ArrivalTime
                },
                Route2 = new{
                    LP = route2.LP,
                    ASPathLength = route2.ASPathLength,
                    Origin = route2.Origin,
                    MED = route2.MED,
                    ASN = route2.ASN,
                    IGP = route2.IGP,
                    RID = Utils.UintToPrefix(route2.RID),
                    NgbrAddr = Utils.UintToPrefix(route2.NeighborAddr),
                    ArrivalTime = route2.ArrivalTime
                },
                Decision = new{
                    LP = dec.LP,
                    ASPathLength = dec.ASPathLength,
                    Origin = dec.Origin,
                    MED = dec.MED,
                    ASN = dec.ASN,
                    IGP = dec.IGP,
                    RID = Utils.UintToPrefix(dec.RID),
                    NgbrAddr = Utils.UintToPrefix(dec.NeighborAddr),
                    ArrivalTime = dec.ArrivalTime
                }
            };

            string jsonString = JsonConvert.SerializeObject(result, Formatting.Indented);
            File.WriteAllText(file.FullName, jsonString);
        }

        /// <summary>
        /// For Aggregation tests
        /// </summary>
        public static void CreateJson(Router rt, IPAttr route1, IPAttr route2, FSeq<IPAttr> res, int n_tests)
        {
            FileInfo file = new FileInfo($"../CLI/tests/{n_tests}.json");
            var result = new
            {
                Router = RouterToDict(rt),
                Route1 = RouteToDict(route1),
                Route2 = RouteToDict(route2),
                output = ListRoutes(res)
            };
            string jsonString = JsonConvert.SerializeObject(result, Formatting.Indented);
            File.WriteAllText(file.FullName, jsonString);
        }

        /// <summary>
        /// For Aggregation tests, config change
        /// </summary>
        public static void CreateJson(Router rt1, Router rt2, IPAttr route1, IPAttr route2, FSeq<IPAttr> dec1, FSeq<IPAttr> dec2, int n_tests)
        {
            FileInfo file = new FileInfo($"../CLI/tests/{n_tests}.json");
            var result = new
            {
                Router1 = RouterToDict(rt1),
                Router2 = RouterToDict(rt2),
                Route1 = RouteToDict(route1),
                Route2 = RouteToDict(route2),
                Decision1 = ListRoutes(dec1),
                Decision2 = ListRoutes(dec2)
            };
            string jsonString = JsonConvert.SerializeObject(result, Formatting.Indented);
            File.WriteAllText(file.FullName, jsonString);
        }

        /// <summary>
        /// For Aggregation tests, route change (route2 --> route3)
        /// </summary>
        public static void CreateJson(Router rt, IPAttr route1, IPAttr route2, IPAttr route3, FSeq<IPAttr> dec1, FSeq<IPAttr> dec2, int n_tests)
        {
            FileInfo file = new FileInfo($"../CLI/tests/{n_tests}.json");
            var result = new
            {
                Router = RouterToDict(rt),
                Route1 = RouteToDict(route1),
                Route2 = RouteToDict(route2),
                Route3 = RouteToDict(route3),
                Decision1 = ListRoutes(dec1),
                Decision2 = ListRoutes(dec2)
            };
            string jsonString = JsonConvert.SerializeObject(result, Formatting.Indented);
            File.WriteAllText(file.FullName, jsonString);
        }


    }

}