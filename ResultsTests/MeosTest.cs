using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Results.Meos;
using static Results.Meos.MeosResultSource;

namespace ResultsTests
{
    [TestClass]
    public class MeosTest
    {

        private static readonly string Message =
            new XElement(MopNs + "MOPComplete",
                new XElement(MopNs + "competition", new XAttribute("date", "2012-08-12"), new XAttribute("organizer", "OK Linné"), new XAttribute("homepage", "http://www.oklinne.nu"), "Linnéklassikern"),
                new XElement(MopNs + "ctrl", new XAttribute("id", "70"), "Radio"),
                new XElement(MopNs + "cls", new XAttribute("id", "1"), new XAttribute("ord", "10"), new XAttribute("radio", "70"), "H21E"),
                new XElement(MopNs + "cls", new XAttribute("id", "19"), new XAttribute("ord", "190"), new XAttribute("radio", ""), "H35"),
                new XElement(MopNs + "org", new XAttribute("id", "255"), new XAttribute("nat", "SWE"), "Länna IF"),
                new XElement(MopNs + "org", new XAttribute("id", "515"), new XAttribute("nat", "SWE"), "OK Österåker"),
                new XElement(MopNs + "cmp", new XAttribute("id", "5490"), new XAttribute("card", "12345"),
                    new XElement(MopNs + "base", new XAttribute("org", "515"), new XAttribute("cls", "1"), new XAttribute("stat", "1"), new XAttribute("st", "370800"), new XAttribute("rt", "71480"), new XAttribute("bib", "100"), new XAttribute("nat", "SWE"), "Jonas Svensson"),
                    new XElement(MopNs + "radio", "70,27160")),
                new XElement(MopNs + "cmp", new XAttribute("id", "23378"), new XAttribute("card", "515501"),
                    new XElement(MopNs + "base", new XAttribute("org", "255"), new XAttribute("cls", "19"), new XAttribute("stat", "1"), new XAttribute("st", "401400"), new XAttribute("rt", "50740"), new XAttribute("nat", "SWE"), "Hostas Sjögren")),
                new XElement(MopNs + "cmp", new XAttribute("id", "3718"), new XAttribute("card", "536609"), new XAttribute("competing", "true"),
                    new XElement(MopNs + "base", new XAttribute("org", "255"), new XAttribute("cls", "19"), new XAttribute("stat", "0"), new XAttribute("st", "402000"), new XAttribute("rt", "50740"), new XAttribute("nat", "GBR"), "Jill Pennyfeather"))).ToString();


        [TestMethod]
        public async Task TestMethod1()
        {
            using var meosResultSource = new MeosResultSource();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(Message));
            var newResultPostAsync = await meosResultSource.NewResultPostAsync(stream).ConfigureAwait(false);
        }
    }
}