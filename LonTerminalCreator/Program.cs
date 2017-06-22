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
                                        GroupBy(a => a.Attribute("Code").Value,       // group by route code 
                                                i => new
                                                {
                                                    StartDate = i.Element("PermittedStations")?.Attribute("StartDate")?.Value,
                                                    EndDate = i.Element("PermittedStations")?.Attribute("EndDate")?.Value,
                                                    StationList = i.Element("PermittedStations").Elements("Station").Select(j => j.Value).ToList()
                                                })).OrderBy(x=>x.Key);

                var crsDups = d.Where(x => x.Count() > 1);

                XNamespace xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
                XNamespace xsd = XNamespace.Get("http://www.w3.org/2001/XMLSchema");
                XNamespace outputns = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
                var outputDoc = new XDocument(
                        new XDeclaration("1.0", "utf-8", "no"),
                        new XElement(outputns + "LondonTerminalMappingData",
                                            new XAttribute("xmlns", outputns.NamespaceName),
                                            new XAttribute(XNamespace.Xmlns + "xsd", xsd.NamespaceName),
                                            new XAttribute(XNamespace.Xmlns + "xsi", xsi.NamespaceName),
                                            new XAttribute("version", "1.3"),
                        new XElement(outputns + "Stations",
                            from station in d select new XElement(outputns + "Station",
                                new XElement(outputns + "CrsCode", new XText(station.Key)),
                                    new XElement(outputns + "LondonTerminalsMappings",
                                        from route in station.First()
                                        from permittedStations in route
                                        select new XElement(outputns + "LondonTerminalMapping",
                                            new XElement(outputns + "LondonTerminals",
                                                from dest in permittedStations.StationList
                                                select new XElement(outputns + "Terminal", new XText(dest))),
                                                    new XElement(outputns + "StartDate", new XText(permittedStations.StartDate)),
                                                    new XElement(outputns + "EndDate", new XText(permittedStations.EndDate)),
                                                    new XElement(outputns + "FareRoute", new XText(route.Key))))))));

                outputDoc.Save("q:\\temp\\save1.xml");
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
