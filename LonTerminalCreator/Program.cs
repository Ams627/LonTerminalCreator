using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LonTerminalCreator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                if (args.Count() != 1)
                {
                    throw new Exception("You must supply an IDMS group station filename.");
                }

                var document = XDocument.Load(args[0]);
                var ns = document.Root.GetDefaultNamespace();

                var res = (from origin in document.Descendants(ns + "Station")
                           let londonList = origin.Elements(ns + "FareGroupLocation")
                                ?.Where(y => y.Attribute("Nlc")?.Value == "1072")
                                ?.FirstOrDefault()?.Descendants(ns + "Station").Select(d=>d.Value)?.ToList()
                           let originCrs = origin.Attribute("Crs")?.Value
                           where originCrs != null && londonList != null
                           select new {originCrs, londonList}
                           ).ToDictionary(x => x.originCrs, x => x.londonList);
                          
                foreach (var station in res)
                {
                    Console.WriteLine($"{station.Key}");
                    foreach (var destination in station.Value)
                    {
                        Console.WriteLine($"    {destination}");
                    }
                }

                Console.WriteLine("");
            }
            catch (Exception ex)
            {
                var codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                var progname = Path.GetFileNameWithoutExtension(codeBase);
                Console.Error.WriteLine(progname + ": Error: " + ex.Message);
            }

        }
    }
}
