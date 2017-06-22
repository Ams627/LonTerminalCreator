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


                var d = document.Descendants(ns + "Stations").First().Elements(ns + "Station")
                    .GroupBy(x => x.Attribute("Crs")?.Value,
                             x => x.Elements("FareGroupLocation").Where(y => y.Attribute("Nlc")?.Value == "1072")  // take only london dests
                                    .SelectMany(z => z.Elements("FareRoute")).
                                        GroupBy(a => a.Attribute("Code"),       // group by route code 
                                                i => new
                                                {
                                                    StartDate = i.Element("PermittedStations")?.Attribute("StartDate")?.Value,
                                                    EndDate = i.Element("PermittedStations")?.Attribute("EndDate")?.Value,
                                                    StationList = i.Element("PermittedStations").Elements("Station").Select(j => j.Value).ToList()
                                                }));

                var crsDups = d.Where(x => x.Count() > 1);


                XNamespace xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
                XNamespace xsd = XNamespace.Get("http://www.w3.org/2001/XMLSchema");
                XNamespace outputns = XNamespace.Get("http://schema.test.com/test");
                var outputDoc = new XDocument(
                        new XDeclaration("1.0", "utf-8", "yes"),
                        new XElement(outputns + "LondonTerminalMappingData",
                                            new XAttribute("xmlns", outputns.NamespaceName),
                                            new XAttribute(XNamespace.Xmlns + "xsd", xsd.NamespaceName),
                                            new XAttribute(XNamespace.Xmlns + "xsi", xsi.NamespaceName),
                        new XElement(ns + "Stations",
                            from station in d select new XElement(ns + "Station",
                                new XElement(ns + "CrsCode", new XText(station.Key),
                                    new XElement(ns + "LondonTerminalsMappings",
                                        from route in station.First()
                                            select new XElement(ns + "LondonTerminalMapping",
                                                new XElement(ns + "LondonTerminals", 
                                                    from dest in route.Key
                                                    select new XElement(ns + "Terminal", new XText(dest.st                                                    
                                                    
                                                    
                                                    )))));
                outputDoc.Save("q:\\temp\\save1.xml");
                foreach (var station in d)
                {
                    Console.WriteLine($"CRS {station.Key}");
                    foreach(var stationElement in station) // should be only one station in each GroupBy grouping as we group by CRS
                    {
                        foreach (var route in stationElement)
                        {
                            Console.WriteLine($"{route.Key}");
                            foreach (var permittedStationList in route)
                            {
                                Console.WriteLine($"start date is {permittedStationList.StartDate}");
                                Console.WriteLine($"end date is {permittedStationList.EndDate}");
                                foreach (var destination in permittedStationList.StationList)
                                {
                                    Console.WriteLine($"{destination}");
                                }
                            }
                        }
                    }
                }



                //foreach (var station in doubleDict)
                //{
                //    Console.WriteLine($"{station.Key}");
                //    foreach (var routedic in station.Value)
                //    {
                //        Console.WriteLine($"    Route: {routedic.Key}");
                //        Console.WriteLine($"    StartDate: {routedic.Value.startDate}");
                //        Console.WriteLine($"    EndDate: {routedic.Value.endDate}");
                //        foreach (var destination in routedic.Value.stationList)
                //        {
                //            Console.WriteLine($"        {destination}");
                //        }
                //    }
                //}
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
