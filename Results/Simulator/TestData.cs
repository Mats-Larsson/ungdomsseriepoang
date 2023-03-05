﻿using Results.Model;

namespace Results.Simulator
{
    internal static class TestData
    {
        internal static readonly TimeSpan ZeroTime = TimeSpan.FromHours(18.5);

        static TestData()
        {
            foreach (var participantResult in TemplateParticipantResults)
            {
                participantResult.Club = Clubs[Random.Shared.Next(0, Clubs.Length)];
            }
        }

        // ReSharper disable StringLiteralTypo
        private static readonly string[] Clubs = {
            "Snättringe SK",
            "Tullinge SK",
            "Järfälla OK",
            "Tumba-Mälarhöjden OK",
            "Sundbybergs IK",
            "OK Ravinen",
            "Järla Orientering",
            "Täby OK",
            "Skogsluffarnas OK",
            "IFK Lidingö SOK",
            "Attunda OK",
            "OK Södertörn",
            "Söders-Tyresö",
            "Väsby OK",
            "Skarpnäcks OL",
            "OK Älvsjö-Örby",
            "Waxholms OK",
            "IFK Enskede",
            "Mälarö SOK",
            "Bromma-Vällingby SOK",
            "Haninge SOK",
            "Gustavsbergs OK",
            "OK Österåker",
            "Hellas Orientering",
            "Enebybergs IF",
            "Solna OK",
            "Nynäshamns OK"
        };

        internal static readonly SimulatorParticipantResult[] TemplateParticipantResults =
        {
            new("D16", "Sofia Lindhagen", "IFK Enskede", T("18:34:00.000"), T("00:00:00"), "notStarted"),
            new("D16K", "Lilly Christie", "Snättringe SK", T("18:32:00.000"), T("00:00:00"), "notValid"),
            new("H14", "Frans Ekberg", "Snättringe SK", T("18:31:00.000"), T("00:00:00"), "notStarted"),
            new("H14K", "Edvin Flodell Johansson", "Haninge SOK", T("18:32:00.000"), T("00:00:00"), "notStarted"),
            new("H12", "Joel Holmgren", "IFK Enskede", T("18:31:00.000"), T("00:00:00"), "notStarted"),
            new("H12K", "Love Norell", "Snättringe SK", T("18:45:00.000"), T("00:00:00"), "notStarted"),
            new("U1", "Daniel Söderberg", "Haninge SOK", T("00:00:00"), T("00:00:00"), "notStarted"),
            new("U1", "Cecilia Silver", "Haninge SOK", T("00:00:00"), T("00:00:00"), "notStarted"),
            new("U1", "Vilmer Gelius", "Snättringe SK", T("00:00:00"), T("00:00:00"), "notStarted"),
            new("U2", "Elin Storell", "Haninge SOK", T("00:00:00"), T("00:00:00"), "notStarted"),
            new("U2", "Ines Thepper", "Snättringe SK", T("00:00:00"), T("00:00:00"), "notStarted"),
            new("U2", "Ester Höglind", "IFK Enskede", T("00:00:00"), T("00:00:00"), "notStarted"),
            new("U2", "Alma Höglind", "IFK Enskede", T("00:00:00"), T("00:00:00"), "notStarted"),
            new("U2", "Lovisa Gelius", "Snättringe SK", T("00:00:00"), T("00:00:00"), "notStarted"),
            new("U3", "Edvin Ekberg", "Snättringe SK", T("00:00:00"), T("00:00:00"), "notStarted"),
            new("U3", "Ella Wasberg", "Haninge SOK", T("00:00:00"), T("00:00:00"), "notStarted"),
            new("Insk", "Mats Larsson", "IFK Enskede", T("04:37:44.000"), T("-10:46:39.0000"), "notParticipating"),
            new("Insk", "Elin Ahlström", "IFK Enskede", T("00:00:00"), T("00:00:00.0000"), "notActivated"),
            new("U1", "Tira Hultin", "Snättringe SK", T("00:00:00"), T("00:00:00.0000"), "notStarted"),
            new("U2", "Sonja Lundberg", "IFK Enskede", T("00:00:00"), T("00:00:00.0000"), "notParticipating"),
            new("U2", "Rickard Melchiorre", "Haninge SOK", T("00:00:00"), T("00:00:00.0000"), "notStarted"),
            new("H10", "Gustav Eggöy Markhester", "Snättringe SK", T("18:32:00.000"), T("00:12:15.0000"), "passed"),
            new("U1", "Reidmond Leidner", "Snättringe SK", T("18:40:27.000"), T("00:13:47.0000"), "passed"),
            new("U1", "Douglas Hanikat", "Haninge SOK", T("18:42:17.000"), T("00:13:56.0000"), "passed"),
            new("U1", "Nike Vejedal", "Snättringe SK", T("18:34:03.000"), T("00:14:26.0000"), "passed"),
            new("Insk", "Elsa Strindelius Gaunitz", "Snättringe SK", T("18:40:14.000"), T("00:15:13.0000"), "passed"),
            new("U2", "Oliver Nordmyr Magnusson", "Snättringe SK", T("18:42:43.000"), T("00:15:20.0000"), "passed"),
            new("D10", "Nora Ahlström", "IFK Enskede", T("18:41:00.000"), T("00:15:39.0000"), "passed"),
            new("U1", "Titti Sjöstrand", "Snättringe SK", T("18:40:50.000"), T("00:15:46.0000"), "passed"),
            new("Insk", "Alexander Kaznov", "Snättringe SK", T("19:10:51.000"), T("00:15:47.0000"), "passed"),
            new("U2", "Noel Hedman", "Snättringe SK", T("18:44:54.000"), T("00:15:48.0000"), "passed"),
            new("Insk", "Alice Eggöy Markhester", "Snättringe SK", T("18:36:39.000"), T("00:16:06.0000"), "passed"),
            new("Insk", "Ivar Boström", "Snättringe SK", T("18:34:56.000"), T("00:16:28.0000"), "passed"),
            new("U1", "Ludvig Lind", "Snättringe SK", T("18:39:42.000"), T("00:16:32.0000"), "passed"),
            new("Insk", "Agnes Melin", "Snättringe SK", T("18:34:42.000"), T("00:16:44.0000"), "passed"),
            new("U2", "Simon Lundberg", "Haninge SOK", T("18:44:31.000"), T("00:17:26.0000"), "passed"),
            new("H10", "Harry Norell", "Snättringe SK", T("18:30:00.000"), T("00:17:34.0000"), "passed"),
            new("Insk", "Edla Leidner", "Snättringe SK", T("18:37:03.000"), T("00:17:36.0000"), "passed"),
            new("U2", "Malte Holmquist", "IFK Enskede", T("18:36:55.000"), T("00:17:41.0000"), "passed"),
            new("U2", "Peter Kaznov", "Snättringe SK", T("19:08:48.000"), T("00:17:41.0000"), "notValid"),
            new("H10", "Jon Forsberg Rundström", "IFK Enskede", T("18:36:00.000"), T("00:17:48.0000"), "passed"),
            new("U1", "Kasper Florczyk", "Haninge SOK", T("18:28:01.000"), T("00:18:35.0000"), "passed"),
            new("U2", "Ida Lundberg", "Haninge SOK", T("18:43:39.000"), T("00:18:37.0000"), "passed"),
            new("U2", "Sofia Kaznova", "Snättringe SK", T("19:10:17.000"), T("00:18:55.0000"), "passed"),
            new("H10", "Holger Wiktor Jarefors", "Snättringe SK", T("18:46:00.000"), T("00:18:58.0000"), "passed"),
            new("H10", "Hugo Abel", "Snättringe SK", T("18:34:00.000"), T("00:19:12.0000"), "passed"),
            new("Insk", "Hedda Sjöstrand", "Snättringe SK", T("18:40:44.000"), T("00:19:20.0000"), "passed"),
            new("D10", "Vanja Remén", "Snättringe SK", T("18:33:00.000"), T("00:19:24.0000"), "passed"),
            new("H12K", "Hilmar Hedin", "IFK Enskede", T("18:43:00.000"), T("00:19:41.0000"), "passed"),
            new("U2", "Linnea Dahlgren", "Snättringe SK", T("18:29:01.000"), T("00:19:48.0000"), "passed"),
            new("U2", "Malte Berg", "Haninge SOK", T("18:46:25.000"), T("00:19:52.0000"), "passed"),
            new("H10", "August Blomkvist", "Snättringe SK", T("18:38:00.000"), T("00:20:06.0000"), "passed"),
            new("U2", "Erik Hagel Von Heijne", "IFK Enskede", T("18:40:30.000"), T("00:20:19.0000"), "passed"),
            new("U1", "Elton Edding", "Haninge SOK", T("18:28:26.000"), T("00:20:27.0000"), "passed"),
            new("U1", "Lina Pelkonen", "Haninge SOK", T("19:06:02.000"), T("00:20:48.0000"), "passed"),
            new("U2", "Simon Carlqvist", "Snättringe SK", T("18:40:04.000"), T("00:20:52.0000"), "passed"),
            new("U1", "Elsa Hall", "Haninge SOK", T("18:46:13.000"), T("00:21:08.0000"), "passed"),
            new("Insk", "Olof Blomkvist", "Snättringe SK", T("18:32:07.000"), T("00:21:10.0000"), "passed"),
            new("U1", "Elva Reimeringer", "Haninge SOK", T("18:31:09.000"), T("00:21:11.0000"), "passed"),
            new("U1", "Ellinor Starenius", "Snättringe SK", T("18:34:31.000"), T("00:21:21.0000"), "passed"),
            new("U1", "Max Spång", "Klubblös", T("18:37:07.000"), T("00:21:30.0000"), "passed"),
            new("U2", "Emil Libérth", "Haninge SOK", T("18:42:21.000"), T("00:21:34.0000"), "passed"),
            new("D10", "Anja Hartley", "Haninge SOK", T("18:39:00.000"), T("00:21:45.0000"), "passed"),
            new("D10", "Emmy Theander", "Haninge SOK", T("18:35:00.000"), T("00:22:08.0000"), "passed"),
            new("U1", "Vera Holmberg", "Haninge SOK", T("18:29:21.000"), T("00:22:29.0000"), "passed"),
            new("U1", "Felicia Svensson", "Haninge SOK", T("18:29:24.000"), T("00:22:30.0000"), "passed"),
            new("U1", "Felicia Hafström", "Haninge SOK", T("18:29:16.000"), T("00:22:34.0000"), "passed"),
            new("H10", "Algot Sjöstrand", "Snättringe SK", T("18:42:00.000"), T("00:22:49.0000"), "passed"),
            new("H10", "Simon Ahlström", "IFK Enskede", T("18:40:00.000"), T("00:23:20.0000"), "passed"),
            new("U2", "Klara Trygg", "Haninge SOK", T("19:08:01.000"), T("00:23:29.0000"), "passed"),
            new("U2", "Anton Trygg", "Haninge SOK", T("19:07:29.000"), T("00:23:33.0000"), "passed"),
            new("Insk", "Line Vejedal", "Snättringe SK", T("18:34:07.000"), T("00:24:31.0000"), "passed"),
            new("U1", "Einar Frid", "IFK Enskede", T("18:35:11.000"), T("00:24:40.0000"), "passed"),
            new("U1", "Lovisa Starenius", "Snättringe SK", T("18:35:09.000"), T("00:24:57.0000"), "passed"),
            new("H10", "Felix Ahlström", "IFK Enskede", T("18:44:00.000"), T("00:25:00.0000"), "passed"),
            new("U2", "Liam de Boussard", "IFK Enskede", T("18:36:16.000"), T("00:25:14.0000"), "passed"),
            new("U2", "Amelie Boström Hagel", "IFK Enskede", T("18:45:17.000"), T("00:25:19.0000"), "passed"),
            new("D10", "Linnéa Spång", "Snättringe SK", T("18:37:00.000"), T("00:25:26.0000"), "passed"),
            new("U1", "Ella Norrman", "Haninge SOK", T("18:31:50.000"), T("00:25:47.0000"), "passed"),
            new("Insk", "Viggo Lidmar", "Snättringe SK", T("18:32:35.000"), T("00:25:48.0000"), "passed"),
            new("U2", "Lea Wasberg", "Haninge SOK", T("18:35:15.000"), T("00:26:09.0000"), "passed"),
            new("U2", "Manfred Holmgren", "Haninge SOK", T("18:30:27.000"), T("00:26:21.0000"), "passed"),
            new("Insk", "August Emanuelsson", "Snättringe SK", T("18:31:00.000"), T("00:26:25.0000"), "passed"),
            new("U2", "Alma Karlsson", "Snättringe SK", T("18:31:34.000"), T("00:26:33.0000"), "passed"),
            new("Insk", "Dag Björkmarker", "Snättringe SK", T("18:31:02.000"), T("00:26:34.0000"), "passed"),
            new("Insk", "Elvin Hedman", "Snättringe SK", T("18:47:57.000"), T("00:26:37.0000"), "passed"),
            new("U1", "Edward Melin", "Snättringe SK", T("18:38:20.000"), T("00:26:38.0000"), "passed"),
            new("H12", "Ludvig Eggöy Markhester", "Snättringe SK", T("18:33:00.000"), T("00:26:52.0000"), "passed"),
            new("U2", "Nadia Florczyk", "Haninge SOK", T("18:55:02.000"), T("00:27:01.0000"), "passed"),
            new("U2", "Erika Remén", "Snättringe SK", T("18:38:51.000"), T("00:27:11.0000"), "passed"),
            new("Insk", "Sixten Flordal", "Snättringe SK", T("18:36:51.000"), T("00:27:31.0000"), "passed"),
            new("Insk", "Alexander Sundblad", "Snättringe SK", T("18:33:57.000"), T("00:28:07.0000"), "passed"),
            new("U1", "Vilma Dyrelius", "Snättringe SK", T("18:31:28.000"), T("00:28:45.0000"), "passed"),
            new("U2", "Emil Sundblad", "Snättringe SK", T("18:37:17.000"), T("00:28:46.0000"), "passed"),
            new("U2", "Ludvig Kuusimurto", "Snättringe SK", T("19:02:47.000"), T("00:28:58.0000"), "passed"),
            new("U2", "Ulf Wester", "Snättringe SK", T("19:02:48.000"), T("00:29:01.0000"), "passed"),
            new("U1", "Axel Thepper", "Snättringe SK", T("18:42:54.000"), T("00:29:04.0000"), "passed"),
            new("U2", "Sigge Fröberg Wilhelm", "IFK Enskede", T("18:47:47.000"), T("00:30:58.0000"), "passed"),
            new("U2", "Axel Holmgren", "Haninge SOK", T("18:34:00.000"), T("00:31:52.0000"), "passed"),
            new("H14", "Hannes Lindqvist", "IFK Enskede", T("18:49:00.000"), T("00:31:53.0000"), "passed"),
            new("Insk", "Svante Flordal", "Snättringe SK", T("18:36:57.000"), T("00:32:24.0000"), "passed"),
            new("U1", "Victor Hallendal", "IFK Enskede", T("18:38:58.000"), T("00:32:27.0000"), "passed"),
            new("U2", "Robin Nilevi", "Haninge SOK", T("18:45:06.000"), T("00:32:45.0000"), "passed"),
            new("U2", "Rebecka Pannagel", "Snättringe SK", T("18:32:03.000"), T("00:32:48.0000"), "passed"),
            new("U2", "Livia Hartley", "Haninge SOK", T("18:40:10.000"), T("00:32:50.0000"), "passed"),
            new("U2", "Viktor Pannagel", "Snättringe SK", T("18:31:59.000"), T("00:33:32.0000"), "passed"),
            new("Insk", "Viggo Abel", "Snättringe SK", T("18:30:30.000"), T("00:35:30.0000"), "passed"),
            new("H12", "Lucas De Boussard", "IFK Enskede", T("18:39:00.000"), T("00:35:34.0000"), "passed"),
            new("H16", "Isak Gelius", "Snättringe SK", T("18:41:00.000"), T("00:35:39.0000"), "passed"),
            new("D12", "Kerstin Christie", "Snättringe SK", T("18:40:00.000"), T("00:36:03.0000"), "passed"),
            new("U2", "Erik Theander", "Haninge SOK", T("18:36:40.000"), T("00:37:14.0000"), "passed"),
            new("D12", "Thea Gelius", "Snättringe SK", T("18:44:00.000"), T("00:37:57.0000"), "passed"),
            new("U2", "Anton Sedvall", "Haninge SOK", T("18:42:29.000"), T("00:39:00.0000"), "passed"),
            new("D14", "Alva Lidström Lauri", "Snättringe SK", T("18:39:00.000"), T("00:39:14.0000"), "passed"),
            new("H16", "Hampus Hatlen", "Snättringe SK", T("18:47:00.000"), T("00:39:32.0000"), "passed"),
            new("U3", "Gabriel Carlqvist", "Snättringe SK", T("18:35:49.000"), T("00:40:05.0000"), "passed"),
            new("U2", "Wilmer Rosendahl", "Snättringe SK", T("18:40:26.000"), T("00:40:22.0000"), "passed"),
            new("D12", "Ylva Remén", "Snättringe SK", T("18:42:00.000"), T("00:40:38.0000"), "passed"),
            new("H14", "Filip Hatlen", "Snättringe SK", T("18:37:00.000"), T("00:41:10.0000"), "passed"),
            new("U2", "Matilda Flordal", "Snättringe SK", T("18:38:19.000"), T("00:41:46.0000"), "passed"),
            new("U2", "Edith Atterlöf", "IFK Enskede", T("18:42:50.000"), T("00:42:44.0000"), "passed"),
            new("H14", "Malte Thelander", "Haninge SOK", T("18:35:00.000"), T("00:43:04.0000"), "passed"),
            new("D16", "Cecilia Hector", "Snättringe SK", T("18:36:00.000"), T("00:44:48.0000"), "passed"),
            new("H12", "Ludvig Rosendahl", "Snättringe SK", T("18:37:00.000"), T("00:44:57.0000"), "passed"),
            new("D14", "Lovisa Lindqvist", "IFK Enskede", T("18:33:00.000"), T("00:45:40.0000"), "passed"),
            new("H16", "Elis Forsberg Rundström", "IFK Enskede", T("18:45:00.000"), T("00:47:27.0000"), "passed"),
            new("D14", "Felicia Strindér", "IFK Enskede", T("18:37:00.000"), T("00:48:11.0000"), "notValid"),
            new("D14", "Molly Gelius", "Snättringe SK", T("18:45:00.000"), T("00:48:20.0000"), "passed"),
            new("H14", "Hugo Ziegler", "Snättringe SK", T("18:47:00.000"), T("00:49:48.0000"), "passed"),
            new("D16", "Elin Wolgast", "Snättringe SK", T("18:32:00.000"), T("00:51:13.0000"), "passed"),
            new("H16", "Jakob Tedhamre", "Snättringe SK", T("18:43:00.000"), T("00:51:13.0000"), "passed"),
            new("D14", "Elin Abel", "Snättringe SK", T("18:35:00.000"), T("00:51:15.0000"), "passed"),
            new("H16", "Filip Storell", "Haninge SOK", T("18:35:00.000"), T("00:51:42.0000"), "passed"),
            new("H16", "Henrik Lundfors", "IFK Enskede", T("18:39:00.000"), T("00:51:51.0000"), "passed"),
            new("H14", "Linus Nilevi", "Haninge SOK", T("18:41:00.000"), T("00:51:59.0000"), "passed"),
            new("H14K", "Elion Reimeringer", "Haninge SOK", T("18:36:00.000"), T("00:52:18.0000"), "passed"),
            new("D14", "Emma Hallendal", "IFK Enskede", T("18:43:00.000"), T("00:52:54.0000"), "notValid"),
            new("H14", "Ludvig Nord", "Snättringe SK", T("18:43:00.000"), T("00:52:56.0000"), "passed"),
            new("H14", "Henning Blomkvist", "Snättringe SK", T("18:33:00.000"), T("00:53:10.0000"), "passed"),
            new("H16", "Oskar Ekner", "Snättringe SK", T("18:37:00.000"), T("00:55:13.0000"), "passed"),
            new("D14", "Linn De Boussard", "IFK Enskede", T("18:47:00.000"), T("00:55:44.0000"), "passed"),
            new("U3", "Linnea Ekstrand", "Haninge SOK", T("18:35:18.000"), T("00:55:49.0000"), "passed"),
            new("H12", "Emil Norlin", "IFK Enskede", T("18:35:00.000"), T("00:57:34.0000"), "passed"),
            new("H14", "David Nyberg", "Snättringe SK", T("18:39:00.000"), T("00:57:53.0000"), "passed"),
            new("U3", "Ludvig Nirvin", "Haninge SOK", T("18:48:47.000"), T("00:58:00.0000"), "passed"),
            new("H16", "Johan Fredholm", "Snättringe SK", T("18:33:00.000"), T("00:59:12.0000"), "passed"),
            new("U3", "Heidi Lindquist", "Haninge SOK", T("18:33:03.000"), T("00:59:44.0000"), "passed"),
            new("H14K", "Anton Dyrelius", "Snättringe SK", T("18:38:00.000"), T("01:01:57.0000"), "passed"),
            new("D14K", "Tuva Wiktor Jarefors", "Snättringe SK", T("18:30:00.000"), T("01:02:32.0000"), "passed"),
            new("H14K", "Oscar Fredholm", "Snättringe SK", T("18:34:00.000"), T("01:03:50.0000"), "notValid"),
            new("U3", "Martin Lindvall", "Haninge SOK", T("18:35:56.000"), T("01:04:11.0000"), "passed"),
            new("U3", "Martin Nyberg", "Snättringe SK", T("18:49:28.000"), T("01:06:10.0000"), "passed"),
            new("H14", "Peter Lindvall", "Haninge SOK", T("18:45:00.000"), T("01:08:20.0000"), "passed"),
            new("H14K", "Alexander Spärlin", "Snättringe SK", T("18:30:00.000"), T("01:09:12.0000"), "passed"),
            new("D14", "Elin Theander", "Haninge SOK", T("18:41:00.000"), T("01:13:24.0000"), "passed"),
            new("D14", "Moa Glanshed", "Haninge SOK", T("18:51:00.000"), T("01:13:31.0000"), "passed"),
            new("D14", "Linn Nordmyr Magnusson", "Snättringe SK", T("18:49:00.000"), T("01:15:36.0000"), "passed"),
            new("U4", "Hampus Glanshed", "Haninge SOK", T("18:28:41.000"), T("01:16:25.0000"), "notValid"),
            new("H14", "Alvin Forslin", "Snättringe SK", T("18:51:00.000"), T("01:18:08.0000"), "passed"),
            new("H16", "Albert Nord", "Snättringe SK", T("18:31:00.000"), T("01:23:57.0000"), "passed"),
            new("D16", "Clara Höglind", "IFK Enskede", T("18:30:00.000"), T("01:26:05.0000"), "passed"),
        };

        private static TimeSpan T(string time) => TimeSpan.Parse(time);
    }
}

internal class SimulatorParticipantResult : ParticipantResult
{
    public SimulatorParticipantResult(string @class, string name, string club, TimeSpan? startTime, TimeSpan? time, string olaStatus) 
        : base(@class, name, club, startTime, time, TranslateStatus(olaStatus, time))
    {
    }

    private static ParticipantStatus TranslateStatus(string olaStatus, TimeSpan? time)
    {
        return olaStatus switch
        {
            "walkOver" => ParticipantStatus.Ignored,
            "movedUp" => ParticipantStatus.Ignored,
            "notParticipating" => ParticipantStatus.Ignored,

            "notActivated" => ParticipantStatus.NotActivated,

            "started" => ParticipantStatus.Started,

            "finished" => time.HasValue ? ParticipantStatus.Preliminary : ParticipantStatus.Started,
            "finishedTimeOk" => time.HasValue ? ParticipantStatus.Preliminary : ParticipantStatus.Started,
            "finishedPunchOk" => time.HasValue ? ParticipantStatus.Preliminary : ParticipantStatus.Started,

            "passed" => time.HasValue ? ParticipantStatus.Passed : ParticipantStatus.Started,
            "disqualified" => ParticipantStatus.NotValid,
            "notValid" => ParticipantStatus.NotValid,
            "notStarted" => ParticipantStatus.NotStarted,

            _ => throw new InvalidOperationException($"Unexpected status: {olaStatus}")
        };
    }
}