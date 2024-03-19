using System;
using System.Numerics;
using System.Collections.Generic;
using ZenLib;
using System.IO;
using System.Linq;
// using System.Text.RegularExpressions;
using static ZenLib.Zen;
using regex;


// Regex vr = new Regex("^[0-9]+:[0-9]+$");

Console.WriteLine("Starting...");
var reg_gen = new RegexGenerator();

// Regex<char> r2 = Regex.Parse("^[1-2]:(([2-3])|([2-3]*))$"); 
// Console.WriteLine("parsed...");
// Console.WriteLine(r2.IsMatch("2:233"));

// var posneg_exmpls = reg_gen.GeneratePosNegExamples("^([190]+)|([7-9]|[8-9])$");
// var posneg_exmpls = reg_gen.GeneratePosNegExamples("^(([7-9]):([6-8]+))$|^(([1-2]):([3-4]+))$");
var posneg_exmpls = reg_gen.GeneratePosNegExamples("^[1-2]:([2-3])|([2-3]*)$");
var multi_pos = posneg_exmpls.Item1;
var all_neg = posneg_exmpls.Item2;
var com = posneg_exmpls.Item3;


string line;
StreamReader sr = new StreamReader("regexes.txt");
StreamWriter sw = new StreamWriter("regex-ex.txt");
//Read the first line of text

//Continue to read until you reach end of file
var count = 0;

while (true)
{
    count++;
    Console.WriteLine("\ncount: " + count);
    // Console.WriteLine("\nReading line...");
    line = sr.ReadLine();
    if (line == null)
    {
        break;
    }
    //write the line to console window
    // Console.WriteLine("line: " + line + "!");
    // Console.WriteLine("Calling regex generator...");
    posneg_exmpls = reg_gen.GeneratePosNegExamples("^"+line.Replace("\n","")+"$");
    // Console.WriteLine("Done!");
    multi_pos = posneg_exmpls.Item1;
    all_neg = posneg_exmpls.Item2;
    com = posneg_exmpls.Item3;

    foreach (var item in multi_pos)
    {
        // if (vr.IsMatch(item)) {
        //     // Console.WriteLine(item);
        //     sw.Write(com + "," + item + ", P" + "\n"); 
        // }
        sw.Write(com + "," + item + ",P" + "\n");
        
    }

    foreach (var item in all_neg)
    {
        // if (vr.IsMatch(item)) {
        //     // Console.WriteLine(item);
        //     sw.Write(com + "," + item + ", N" +"\n"); 
        // }
        sw.Write(com + "," + item + ",N" +"\n");
        
    }    
}

//close the file
sr.Close();
sw.Close();

// Console.WriteLine("com : "+string.Join("|",com));
// Console.WriteLine("multi_pos : "+string.Join("|",multi_pos));
// Console.WriteLine("all_neg : "+string.Join("|",all_neg));


