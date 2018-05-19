using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Globalization;

namespace BGG
{
    public class Game : IGame
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public override bool Equals(object obj)
        {
            Game other = obj as Game;
            if (other == null)
                return false;
            else
                return Id.Equals(other.Id);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
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
    public class Query
    {
        internal int Page { get; set; } = 1;
        public string Title { get; set; }
        public int? Designer { get; set; }
        public int? Publisher { get; set; }
        public int? PublishedFrom { get; set; }
        public int? PublishedTo { get; set; }
        public int? MinimumAge { get; set; }
        public double? AvgRatingFrom { get; set; }
        public double? AvgRatingTo { get; set; }
        public int? AvgRatingUsers { get; set; }
        public double? AvgWeightFrom { get; set; }
        public double? AvgWeightTo { get; set; }
        public int? AvgWeightUsers { get; set; }
        public bool NoExp { get; set; } = true;
        public Dictionary<Category, bool?> Categories { get; } = new Dictionary<Category, bool?>();
        public Query()
        {
            foreach (Category cat in Enum.GetValues(typeof(Category)))
            {
                Categories[cat] = null;
            }
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder($"https://boardgamegeek.com/search/boardgame/page/{Page}?advsearch=1");
            sb.Append($"&q={Title}");
            sb.Append(Designer != null ? $"&include[designerid]={Designer}" : string.Empty);
            sb.Append(Publisher != null ? $"&include[publisherid]={Publisher}" : string.Empty);
            sb.Append(PublishedFrom != null ? $"&range[yearpublished][min]={PublishedFrom}" : string.Empty);
            sb.Append(PublishedTo != null ? $"&range[yearpublished][max]={PublishedTo}" : string.Empty);
            sb.Append(MinimumAge != null ? $"&range[minage][max]={MinimumAge}" : string.Empty);
            sb.Append(AvgRatingFrom != null ? $"&floatrange[avgrating][min]={AvgRatingFrom?.ToString(CultureInfo.InvariantCulture)}" : string.Empty);
            sb.Append(AvgRatingTo != null ? $"&floatrange[avgrating][max]={AvgRatingTo?.ToString(CultureInfo.InvariantCulture)}" : string.Empty);
            sb.Append(AvgRatingUsers != null ? $"&range[numvoters][min]={AvgRatingUsers}" : string.Empty);
            sb.Append(AvgWeightFrom != null ? $"&floatrange[avgweight][min]={AvgWeightFrom?.ToString(CultureInfo.InvariantCulture)}" : string.Empty);
            sb.Append(AvgWeightTo != null ? $"&floatrange[avgweight][max]={AvgWeightTo?.ToString(CultureInfo.InvariantCulture)}" : string.Empty);
            sb.Append(AvgWeightUsers != null ? $"&range[numweights][min]={AvgWeightUsers}" : string.Empty);
            sb.Append(NoExp ? $"&nosubtypes[]=boardgameexpansion" : string.Empty);
            foreach (var kvp in Categories)
            {
                string line = kvp.Value != null
                    ? $"&{(kvp.Value == false ? "no" : string.Empty)}propertyids[]={(int)kvp.Key}"
                    : string.Empty;
                sb.Append(line);
            }
            return sb.ToString();
        }
    }
    internal static class URI
    {
        internal static string Hot => "https://boardgamegeek.com/xmlapi2/hot?type=boardgame";
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
                                Id = int.Parse(node.Descendants("item").First().Attribute("objectid").Value)
                            },
                            Minutes = int.Parse(node.Attribute("length").Value)
                        };
            return plays.ToList<IPlay>();
        }
        public static List<IGame> FilterHotItems(this XDocument doc)
        {
            var items = from node in doc.Root.Elements("item")
                        select new Game
                        {
                            Id = int.Parse(node.Attribute("id").Value),
                            Name = node.Element("name").Attribute("value").Value
                        };
            return items.ToList<IGame>();
        }
        public static List<IGeekItem> FilterGeekItems(this XDocument doc)
        {
            var items = from node in doc.Root.Elements("item")
                        select new GeekItem
                        {
                            Game = new Game
                            {
                               Id = int.Parse(node.Attribute("objectid").Value),
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
        public static List<IGame> FilterGames(this HtmlDocument doc)
        {
            HtmlNode table = doc.DocumentNode
                .SelectSingleNode("//table[contains(@class, 'collection_table')]");
            var links = table.SelectNodes("//td[contains(@class, 'collection_objectname')]/div/a") ?? Enumerable.Empty<HtmlNode>();
            var games = links.Select(a => new Game
            {
                Id = int.Parse(a.GetAttributeValue("href", string.Empty).Split('/')[2]),
                Name = a.InnerText
            }).ToList<IGame>();
            return games;
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
        public async Task<List<IGame>> GetHotAsync()
        {
            XDocument doc = await GetHot();
            return doc.FilterHotItems();
        }
        public async Task<List<IGame>> GetQueryAsync(Query query)
        {
            HashSet<IGame> games = new HashSet<IGame>();
            //List<IGame> games = new List<IGame>();
            // Keep looking for pages with games
            for (int page = 1; ; page++)
            {
                query.Page = page;
                int asBefore = games.Count;
                HtmlDocument doc = await DoQuery(query);
                List<IGame> gamesOnPage = doc.FilterGames();
                games.UnionWith(gamesOnPage);
                if (games.Count == asBefore) break;
                // wait before asking for result again
                await Task.Delay(config.Delay);
            }
            return games.ToList();
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
        private async Task<XDocument> GetHot()
        {
            string URI = BGG.URI.Hot;
            return await GetXMLFromAsync(URI);
        }
        private async Task<HtmlDocument> DoQuery(Query query)
        {
            using(var client = new WebClient())
            {
                // Have to use webclient for https
                Uri uri = new Uri(query.ToString());
                string page = await client.DownloadStringTaskAsync(uri);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(page);
                return doc;
            }
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
