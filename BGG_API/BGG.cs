using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BGG
{
    public class Game : IGame
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class GeekItem : IGeekItem
    {
        public IGame Game { get; set; }
    }
    public class Play : IPlay
    {
        public IGame Game { get; set; }
        public int Minutes { get; set; }
    }
    public class User : IUser
    {
        [JsonProperty("username")]
        public string UserName { get; set; }
    }
    public class Rating : IRating
    {
        [JsonIgnore]
        public IUser User { get; set; }
        [JsonProperty("rating")]
        public double Value { get; set; }
        [JsonProperty("rating_tstamp")]
        public DateTime TimeStamp { get; set; }
    }
    public class APIConfig
    {
        // ms
        public int Delay { get; set; } = 1000;
    }
    internal static class URI
    {
        internal static string GeekList => "https://www.boardgamegeek.com/xmlapi/geeklist/{0}";
        internal static string Plays => "https://www.boardgamegeek.com/xmlapi2/plays?username={0}&page={1}";
        internal static string Ratings => "https://api.geekdo.com/api/collections?objectid={0}&objecttype=thing&oneperuser=1&pageid={1}&rated=1&require_review=true&showcount=50&sort=review_tstamp";
    }
    internal static class FilterExtensions
    {
        public static List<IPlay> FilterPlays(this XDocument doc)
        {
            var plays = from node in doc.Root.Elements("play")
                        select new Play
                        {
                            Game = new Game
                            {
                                Id = node.Descendants("item").First().Attribute("objectid").Value
                            },
                            Minutes = int.Parse(node.Attribute("length").Value)
                        };
            return plays.ToList<IPlay>();
        }
        public static List<IGeekItem> FilterGeekItems(this XDocument doc)
        {
            var items = from node in doc.Root.Elements("item")
                        select new GeekItem
                        {
                            Game = new Game
                            {
                               Id = node.Attribute("objectid").Value,
                               Name = node.Attribute("objectname").Value
                            }
                        };
            return items.ToList<IGeekItem>();
        }
        public static List<IRating> FilterRatings(this string json)
        {
            var ratings = new List<IRating>();
            JObject jData = JObject.Parse(json);
            foreach (var item in jData["items"])
            {
                Rating rating = JsonConvert.DeserializeObject<Rating>(item.ToString());
                rating.User = JsonConvert.DeserializeObject<User>(item["user"].ToString());
                ratings.Add(rating);
            }
            return ratings;
        }
    }

    public class API
    {
        private APIConfig config;

        public API(APIConfig config)
        {
            this.config = config;
        }
        public async Task<List<IRating>> GetRatingsAsync(int gameId)
        {
            List<IRating> ratings = new List<IRating>();
            // Keep looking for pages with ratings
            for (int page = 1; ; page++)
            {
                string json = await GetRatingsAsync(gameId, page);
                List<IRating> ratingsOnPage = json.FilterRatings();
                if (ratingsOnPage.Count == 0) break;
                // wait before asking for result again
                await Task.Delay(config.Delay);
                ratings.AddRange(ratingsOnPage);
            }
            return ratings;
        }
        public async Task<List<IPlay>> GetPlaysAsync(string user)
        {
            List<IPlay> plays = new List<IPlay>();
            // Keep looking for pages with plays
            for (int page = 1; ; page++)
            {
                XDocument doc = await GetPlaysAsync(user, page);
                List<IPlay> playsOnPage = doc.FilterPlays();
                if (playsOnPage.Count == 0) break;
                // wait before asking for result again
                await Task.Delay(config.Delay);
                plays.AddRange(playsOnPage);
            }
            return plays;
        }
        public async Task<List<IGeekItem>> GetGeekListAsync(int listId)
        {
            XDocument doc = await GetGeekList(listId);
            return doc.FilterGeekItems();
        }
        private async Task<XDocument> GetPlaysAsync(string userName, int pageNumber)
        {
            string URI = string.Format(BGG.URI.Plays, userName, pageNumber);
            return await GetXMLFromAsync(URI);
        }
        private async Task<XDocument> GetGeekList(int listId)
        {
            string URI = string.Format(BGG.URI.GeekList, listId);
            return await GetXMLFromAsync(URI);
        }
        private async Task<string> GetRatingsAsync(int gameId, int pageNumber)
        {
            string URI = string.Format(BGG.URI.Ratings, gameId, pageNumber);
            using (HttpClient httpClient = new HttpClient())
            {
                return await httpClient.GetStringAsync(URI);
            }
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
