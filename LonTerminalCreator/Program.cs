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
                var doubleDict = (from origin in document.Descendants(ns + "Station")
                           let originCrs = origin.Attribute("Crs")?.Value
                           let londonList = origin.Elements(ns + "FareGroupLocation")
                                ?.Where(y => y.Attribute("Nlc")?.Value == "1072")
                           where originCrs != null && londonList != null && londonList.Count() > 0
                           select new
                           {
                               originCrs,
                               routeDic = (from routeSpec in londonList
                                           select new
                                           {
                                               routeCode = routeSpec.Element("FareRoute")?.Attribute("Code")?.Value,
                                               permittedSpec = new
                                               {
                                                   startDate = routeSpec?.Element("FareRoute")?.Element("PermittedStations")?.Attribute("StartDate"),
                                                   endDate = routeSpec?.Element("FareRoute")?.Element("PermittedStations")?.Attribute("EndDate"),
                                                   stationList = routeSpec?.Element("FareRoute")?.Element("PermittedStations")?.Elements("Station").Select(x => x?.Value)
                                               }
                                           }).ToDictionary(x => x.routeCode, x => x.permittedSpec)
                           }).ToDictionary(x => x.originCrs, x => x.routeDic);

                var res = doubleDict.TryGetValue("POO", out var pooleRoutes);
                if (res)
                {
                    var res2 = pooleRoutes.TryGetValue("00000", out var pooleStation);
                    Console.WriteLine("");
                }

                foreach (var station in doubleDict)
                {
                    Console.WriteLine($"{station.Key}");
                    foreach (var routedic in station.Value)
                    {
                        Console.WriteLine($"    Route: {routedic.Key}");
                        Console.WriteLine($"    StartDate: {routedic.Value.startDate}");
                        Console.WriteLine($"    Route: {routedic.Value.endDate}");
                        foreach (var destination in routedic.Value.stationList)
                        {
                            Console.WriteLine($"        {destination}");
                        }
                    }
                }
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
