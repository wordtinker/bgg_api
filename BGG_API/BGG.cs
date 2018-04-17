using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BGG
{
    public class Play : IPlay
    {
        public string GameId { get; set; }
        public int Minutes { get; set; }
    }
    public class APIConfig
    {
        // ms
        public int Delay { get; set; } = 1000;
    }
    internal static class URI
    {
        internal static string Plays => "https://www.boardgamegeek.com/xmlapi2/plays?username={0}&page={1}";
    }
    internal static class FilterExtensions
    {
        public static List<Play> FilterPlays(this XDocument doc)
        {
            var plays = from node in doc.Root.Elements("play")
                        select new Play
                        {
                            GameId = node.Descendants("item").First().Attribute("objectid").Value,
                            Minutes = int.Parse(node.Attribute("length").Value)
                        };
            return plays.ToList();
        }
    }

    public class API
    {
        private APIConfig config;

        public API(APIConfig config)
        {
            this.config = config;
        }
        public async Task<List<IPlay>> GetPlaysAsync(string user)
        {
            List<IPlay> plays = new List<IPlay>();
            // Keep looking for pages with plays
            for (int page = 1; ; page++)
            {
                XDocument doc = await GetPlaysAsync(user, page);
                List<Play> playsOnPage = doc.FilterPlays();
                if (playsOnPage.Count == 0) break;
                plays.AddRange(playsOnPage);
            }
            return plays;
        }
        private async Task<XDocument> GetPlaysAsync(string userName, int pageNumber)
        {
            string URI = string.Format(BGG.URI.Plays, userName, pageNumber);
            return await GetXMLFromAsync(URI);
        }
        private async Task<XDocument> GetXMLFromAsync(string uri)
        {
            // use separate instance every time instead one static instance
            // better spam prevention
            using (HttpClient client = new HttpClient())
            {
                do
                {
                    using (HttpResponseMessage response = await client.GetAsync(uri))
                    {
                        // wait before asking for result again
                        await Task.Delay(config.Delay);
                        // throw on errors
                        response.EnsureSuccessStatusCode();

                        if (response.StatusCode != HttpStatusCode.OK) continue;

                        // Gather results
                        using (HttpContent content = response.Content)
                        {
                            byte[] bytes = await response.Content.ReadAsByteArrayAsync();
                            XDocument doc;
                            using (MemoryStream ms = new MemoryStream(bytes))
                            {
                                doc = XDocument.Load(ms);
                            }
                            return doc;
                        }
                    }
                } while (true);
            }
        }
    }
}
