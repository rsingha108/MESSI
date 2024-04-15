using System;
using System.Collections.Generic;
using System.Numerics;
using ZenLib;
using System.Linq;
using System.IO;
using static ZenLib.Zen;

namespace BGP{
    /// <summary>
    /// Generic utility functions
    /// </summary>
    public static class Utils{
        /// <summary>
        /// And operation
        /// </summary>
        /// <param name="a">expression 1</param>
        /// <param name="b">expression 2</param>
        /// <returns>a boolean</returns>
        public static Zen<bool> AndIf(Zen<bool> a, Zen<bool> b){
            return If<bool>(
                a,
                If<bool>(
                    b,
                    true,
                    false
                ),
                If<bool>(
                    b,
                    false,
                    false
                )
            );
        }

        /// <summary>
        /// Or operation
        /// </summary>
        /// <param name="a">expression 1</param>
        /// <param name="b">expression 2</param>
        /// <returns>a boolean</returns>
        public static Zen<bool> OrIf(Zen<bool> a, Zen<bool> b){
            return If<bool>(
                a,
                If<bool>(
                    b,
                    true,
                    true
                ),
                If<bool>(
                    b,
                    true,
                    false
                )
            );
        }

        /// <summary>
        /// Takes an IPv4 address and returns the unsigned integer representation
        /// </summary>
        /// <param name="ip">IPv4 address</param>
        /// <returns>an unsigned integer</returns>
        public static uint PrefixToUint(int[] ip){
            uint val = 0;
            int shift = 24;
            for(int i=0;i<4;i++){
                val |= (((uint)ip[i]) << shift);
                shift -= 8;
            }

            return val;
        }

        /// <summary>
        /// Converts unsigned integer to corresponding IPv4 format
        /// </summary>
        /// <param name="ip">the unsigned integer</param>
        /// <returns>IPv4 address in string format</returns>
        public static string UintToPrefix(uint ip){
            var pre1 = ip/(1<<24);
            var rem1 = ip%(1<<24);

            var pre2 = rem1 / (1 << 16);
            var rem2 = rem1 % (1 << 16);

            var pre3 = rem2 / (1 << 8);
            var pre4 = rem2 % (1 << 8);

            return $"{pre1}.{pre2}.{pre3}.{pre4}";
        }
        
        /// <summary>
        /// Checks whether the communities in the community attribute are in sorted order
        /// </summary>
        /// <param name="p">the communities attribute</param>
        /// <returns>a boolean</returns>
        public static bool IsSortedCommunity(string p){
            var coms = p.Split(" ");
            var duml = new List<Tuple<int,int>>();
            foreach (var d in coms){
                var t1 = Int32.Parse(d.Split(":")[0]);
                var t2 = Int32.Parse(d.Split(":")[1]);
                var tp = new Tuple<int,int>(t1,t2);
                duml.Add(tp);
            }
            duml = duml.Distinct().ToList();
            duml.Sort(Comparer<Tuple<int, int>>.Default);
            var dums1 = new List<string>();
            foreach (var t in duml){
                dums1.Add(t.Item1.ToString()+":"+t.Item2.ToString());
            }
            var p1 = string.Join(" ",dums1);
            if (p==p1){
                return true;
            }
            else{
                return false;
            }
        }

        public static uint CommunityStr2Int(string s){
            var s1 = s.Split(':');
            uint a = uint.Parse(s1[0]);
            uint b = uint.Parse(s1[1]);
            return a*65536 + b;
        }

        public static string CommunityInt2Str(uint i){
            uint a = i/65536;
            uint b = i%65536;
            return a.ToString() + ":" + b.ToString();
        }

        public static string Prefix2String(uint prefix, uint mask){
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

            return $"{pre1}.{pre2}.{pre3}.{pre4}/{count}";
        }

        public static string Mask2String(Option<uint> x){
                
            if(x.HasValue == false){
                return "None";
            }
            else{
                uint count = 0;
                var n = x.Value;
                while(n > 0){
                    n &= (n - 1);
                    count++;
                }
                return $"{count}";
            }
        }

        public static uint FindSmallestMissingNumber(List<uint> list)
        {
            uint smallestNumber = 1;

            // Sort the list in ascending order
            list.Sort();

            foreach (uint number in list)
            {
                // Check if the number is equal to the expected smallest number
                if (number == smallestNumber)
                {
                    smallestNumber++;
                }
                // If the number is greater than the expected smallest number, we found the missing number
                else if (number > smallestNumber)
                {
                    break;
                }
            }

            return smallestNumber;
        }

        public static List<string> ComputeCrossProduct(List<string> list1, List<string> list2)
        {
            List<string> crossProduct = new List<string>();

            foreach (string item1 in list1)
            {
                foreach (string item2 in list2)
                {
                    string result = item1 + " " + item2;
                    crossProduct.Add(result);
                }
            }

            return crossProduct;
        }

        public static void ShuffleList<T>(List<T> list)
        {
            Random random = new Random();

            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        public static List<uint> GenerateRandomUniqueNumbers(int count, uint minValue, uint maxValue)
        {
            if (count > maxValue - minValue + 1)
            {
                throw new ArgumentException("Cannot generate more unique numbers than the range allows.");
            }

            List<uint> numbers = new List<uint>();
            Random random = new Random();

            while (numbers.Count < count)
            {
                uint randomNumber = (uint)random.Next((int)minValue, (int)maxValue + 1);

                if (!numbers.Contains(randomNumber))
                {
                    numbers.Add(randomNumber);
                }
            }

            return numbers;
        }

        public static void PrintList(List<uint> list)
        {
            foreach (uint value in list)
            {
                Console.Write(value + " ");
            }
            Console.WriteLine();
        }

        public static List<uint> FlattenList(List<List<uint>> nestedList)
        {
            List<uint> flattenedList = nestedList.SelectMany(x => x).ToList();
            return flattenedList;
        }
    }
}