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
            if (obj is Game other)
                return Id.Equals(other.Id);
            else
                return false;
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public override string ToString()
        {
            return $"https://boardgamegeek.com/boardgame/{Id}";
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
    [Serializable]
    public class CategoryDescriptor
    {
        public int Id { get; internal set; }
        public bool? On { get; set; } = null;
    }
    [Serializable]
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
        public Dictionary<string, CategoryDescriptor> Categories { get; } = new Dictionary<string, CategoryDescriptor>()
        {
            {"AbstractStrategy", new CategoryDescriptor{ Id = 1009 } },
            {"ActionDexterity", new CategoryDescriptor{ Id = 1032} },
            {"Adventure", new CategoryDescriptor{ Id = 1022 } },
            {"AgeOfReason", new CategoryDescriptor { Id = 2726 } },
            {"AmericanCivilWar", new CategoryDescriptor{ Id = 1048} },
            {"AmericanIndianWars", new CategoryDescriptor{ Id = 1108} },
            {"AmericanRevolutionaryWar", new CategoryDescriptor{ Id = 1075 } },
            {"AmericanWest", new CategoryDescriptor{ Id = 1055 } },
            {"Ancient", new CategoryDescriptor{ Id = 1050} },
            {"Animals", new CategoryDescriptor{ Id = 1089 } },
            {"Arabian", new CategoryDescriptor{ Id = 1052} },
            {"AviationFight", new CategoryDescriptor{ Id = 2650} },
            {"Bluffing", new CategoryDescriptor{Id = 1023} },
            {"Book", new CategoryDescriptor{ Id = 1117 } },
            {"CardGame", new CategoryDescriptor { Id = 1002 } },
            {"Children", new CategoryDescriptor { Id = 1041 } },
            {"CityBuilding", new CategoryDescriptor { Id = 1029 } },
            {"CivilWar", new CategoryDescriptor { Id = 1102 } },
            {"Civilization", new CategoryDescriptor { Id = 1015 } },
            {"Collectible", new CategoryDescriptor { Id = 1044 } },
            {"ComicStrip", new CategoryDescriptor { Id = 1116 } },
            {"Deduction", new CategoryDescriptor { Id = 1039 } },
            {"Dice", new CategoryDescriptor { Id = 1017 } },
            {"Economic", new CategoryDescriptor { Id = 1021 } },
            {"Educational", new CategoryDescriptor { Id = 1094 } },
            {"Electronic", new CategoryDescriptor { Id = 1072 } },
            {"Environmental", new CategoryDescriptor { Id = 1084 } },
            {"Expansion", new CategoryDescriptor { Id = 1042 } },
            {"Exploration", new CategoryDescriptor { Id = 1020 } },
            {"FanExpansion", new CategoryDescriptor { Id = 2687 } },
            {"Fantasy", new CategoryDescriptor { Id = 1010 } },
            {"Farming", new CategoryDescriptor { Id = 1013 } },
            {"Fighting", new CategoryDescriptor { Id = 1046 } },
            {"GameSystem", new CategoryDescriptor { Id = 1119 } },
            {"Horror", new CategoryDescriptor { Id = 1024 } },
            {"Humor", new CategoryDescriptor { Id = 1079 } },
            {"Industry", new CategoryDescriptor { Id = 1088 } },
            {"KoreanWar", new CategoryDescriptor { Id = 1091 } },
            {"Mafia", new CategoryDescriptor { Id = 1033 } },
            {"Math", new CategoryDescriptor { Id = 1104 } },
            {"Adult", new CategoryDescriptor { Id = 1118 } },
            {"Maze", new CategoryDescriptor { Id = 1059 } },
            {"Medical", new CategoryDescriptor { Id = 2145 } },
            {"Medieval", new CategoryDescriptor { Id = 1035 } },
            {"Memory", new CategoryDescriptor { Id = 1045 } },
            {"Miniatures", new CategoryDescriptor { Id = 1047 } },
            {"ModernWarfare", new CategoryDescriptor { Id = 1069 } },
            {"MediaTheme", new CategoryDescriptor { Id = 1064 } },
            {"Murder", new CategoryDescriptor { Id = 1040 } },
            {"Music", new CategoryDescriptor { Id = 1054 } },
            {"Mythology", new CategoryDescriptor { Id = 1082 } },
            {"Napoleonic", new CategoryDescriptor { Id = 1051 } },
            {"Nautical", new CategoryDescriptor { Id = 1008 } },
            {"Negotiation", new CategoryDescriptor { Id = 1026 } },
            {"NovelBased", new CategoryDescriptor { Id = 1093 } },
            {"Number", new CategoryDescriptor { Id = 1098 } },
            {"Party", new CategoryDescriptor { Id = 1030 } },
            {"PikeAndShot", new CategoryDescriptor { Id = 2725 } },
            {"Pirates", new CategoryDescriptor { Id = 1090 } },
            {"Political", new CategoryDescriptor { Id = 1001 } },
            {"PostNapoleoic", new CategoryDescriptor { Id = 2710 } },
            {"Prehistoric", new CategoryDescriptor { Id = 1036 } },
            {"PnP", new CategoryDescriptor { Id = 1120 } },
            {"Puzzle", new CategoryDescriptor { Id = 1028 } },
            {"Racing", new CategoryDescriptor { Id = 1031 } },
            {"RealTime", new CategoryDescriptor { Id = 1037 } },
            {"Religious", new CategoryDescriptor { Id = 1115 } },
            {"Renaissance", new CategoryDescriptor { Id = 1070 } },
            {"ScienceFiction", new CategoryDescriptor { Id = 1016 } },
            {"SpaceExploration", new CategoryDescriptor { Id = 1113 } },
            {"Spies", new CategoryDescriptor { Id = 1081 } },
            {"Sports", new CategoryDescriptor { Id = 1038 } },
            {"TerritoryBuilding", new CategoryDescriptor { Id = 1086 } },
            {"Trains", new CategoryDescriptor { Id = 1034 } },
            {"Transportation", new CategoryDescriptor { Id = 1011 } },
            {"Travel", new CategoryDescriptor { Id = 1097 } },
            {"Trivia", new CategoryDescriptor { Id = 1027 } },
            {"VideoGameTheme", new CategoryDescriptor { Id = 1101 } },
            {"VietnamWar", new CategoryDescriptor { Id = 1109 } },
            {"Wargame", new CategoryDescriptor { Id = 1019 } },
            {"WordGame", new CategoryDescriptor { Id = 1025 } },
            {"WWI", new CategoryDescriptor { Id = 1065 } },
            {"WWII", new CategoryDescriptor { Id = 1049 } },
            {"Zombies", new CategoryDescriptor { Id = 2481 } }
        };
        public Dictionary<string, CategoryDescriptor> Mechanics { get; } = new Dictionary<string, CategoryDescriptor>()
        {
            {"Acting", new CategoryDescriptor{ Id = 2073} },
            {"ActionMovementProgramming", new CategoryDescriptor{ Id = 2689} },
            {"ActionPointAllowance", new CategoryDescriptor{ Id = 2001} },
            {"AreaControl", new CategoryDescriptor { Id = 2080 } },
            {"AreaEnclosure", new CategoryDescriptor { Id = 2043 } },
            {"AreaMovement", new CategoryDescriptor { Id = 2046 } },
            {"AreaImpulse", new CategoryDescriptor { Id = 2021 } },
            {"Auction", new CategoryDescriptor { Id = 2012 } },
            {"Betting", new CategoryDescriptor { Id = 2014 } },
            {"CampaignBattleCardDriven", new CategoryDescriptor { Id = 2018 } },
            {"Drafting", new CategoryDescriptor { Id = 2041 } },
            {"ChitPull", new CategoryDescriptor { Id = 2057 } },
            {"CoOp", new CategoryDescriptor { Id = 2023 } },
            {"CommoditySpeculation", new CategoryDescriptor { Id = 2013 } },
            {"CrayonRail", new CategoryDescriptor { Id = 2010 } },
            {"DeckPoolBuilding", new CategoryDescriptor { Id = 2664 } },
            {"DiceRolling", new CategoryDescriptor { Id = 2072 } },
            {"GridMovement", new CategoryDescriptor { Id = 2676 } },
            {"HandManagement", new CategoryDescriptor { Id = 2040 } },
            {"HexAndCounter", new CategoryDescriptor { Id = 2026 } },
            {"LineDrawing", new CategoryDescriptor { Id = 2039 } },
            {"Memory", new CategoryDescriptor { Id = 2047 } },
            {"ModularBoard", new CategoryDescriptor { Id = 2011 } },
            {"PaperAndPencil", new CategoryDescriptor { Id = 2055 } },
            {"Partnerships", new CategoryDescriptor { Id = 2019 } },
            {"PatternBuilding", new CategoryDescriptor { Id = 2048 } },
            {"PatternRecognition", new CategoryDescriptor { Id = 2060 } },
            {"PickUpDeliver", new CategoryDescriptor { Id = 2007 } },
            {"PlayerElimination", new CategoryDescriptor { Id = 2685 } },
            {"PointToPointMovement", new CategoryDescriptor { Id = 2078 } },
            {"PressYourLuck", new CategoryDescriptor { Id = 2661 } },
            {"RockPaperScissors", new CategoryDescriptor { Id = 2003 } },
            {"RolePlaying", new CategoryDescriptor { Id = 2028 } },
            {"RollAndMove", new CategoryDescriptor { Id = 2035 } },
            {"RouteBuilding", new CategoryDescriptor { Id = 2081 } },
            {"SecretUnitDeployment", new CategoryDescriptor { Id = 2016 } },
            {"SetCollection", new CategoryDescriptor { Id = 2004 } },
            {"Simulation", new CategoryDescriptor { Id = 2070 } },
            {"SimActionSelection", new CategoryDescriptor { Id = 2020 } },
            {"Singing", new CategoryDescriptor { Id = 2038 } },
            {"StockHolding", new CategoryDescriptor { Id = 2005 } },
            {"Storytelling", new CategoryDescriptor { Id = 2027 } },
            {"TakeThat", new CategoryDescriptor { Id = 2686 } },
            {"TilePlacement", new CategoryDescriptor { Id = 2002 } },
            {"TimeTrack", new CategoryDescriptor { Id = 2663 } },
            {"Trading", new CategoryDescriptor { Id = 2008 } },
            {"TrickTaking", new CategoryDescriptor { Id = 2009 } },
            {"VariablePhaseOrder", new CategoryDescriptor { Id = 2079 } },
            {"VariablePlayerPowers", new CategoryDescriptor { Id = 2015 } },
            {"Voting", new CategoryDescriptor { Id = 2017 } },
            {"WorkerPlacement", new CategoryDescriptor { Id = 2082 } }
        };
        public Dictionary<string, CategoryDescriptor> Domains { get; } = new Dictionary<string, CategoryDescriptor>()
        {
            {"Abstract", new CategoryDescriptor{ Id = 4666 } },
            {"Children", new CategoryDescriptor{ Id = 4665 } },
            {"Customizable", new CategoryDescriptor { Id = 4667 } },
            {"Family", new CategoryDescriptor{ Id = 5499 } },
            {"Party", new CategoryDescriptor{ Id = 5498 } },
            {"Strategy", new CategoryDescriptor{ Id = 5497 } },
            {"Thematic", new CategoryDescriptor{ Id = 5496 } },
            {"Wargame", new CategoryDescriptor{ Id = 4664 } }
        };
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
            foreach (var val in Categories.Values.Union(Mechanics.Values))
            {
                string line = val.On != null
                    ? $"&{(val.On == false ? "no" : string.Empty)}propertyids[]={val.Id}"
                    : string.Empty;
                sb.Append(line);
            }
            foreach (var val in Domains.Values)
            {
                string line = val.On != null
                    ? $"&{(val.On == false ? "no" : string.Empty)}familyids[]={val.Id}"
                    : string.Empty;
                sb.Append(line);
            }
            return sb.ToString();
        }
    }
    internal static class URI
    {
        internal static string Top => "https://boardgamegeek.com/browse/boardgame/page/{0}";
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
                string URI = string.Format(BGG.URI.Ratings, gameId, page);
                string json = await GetStringFromAsync(URI);
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
                string URI = string.Format(BGG.URI.Plays, user, page);
                XDocument doc = await GetXMLFromAsync(URI);
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
            string URI = string.Format(BGG.URI.GeekList, listId);
            XDocument doc = await GetXMLFromAsync(URI);
            return doc.FilterGeekItems();
        }
        public async Task<List<IGame>> GetHotAsync()
        {
            string URI = BGG.URI.Hot;
            XDocument doc = await GetXMLFromAsync(URI);
            return doc.FilterHotItems();
        }
        public async Task<List<IGame>> GetQueryAsync(Query query)
        {
            HashSet<IGame> games = new HashSet<IGame>();
            // Keep looking for pages with games
            for (int page = 1; ; page++)
            {
                query.Page = page;
                int asBefore = games.Count;
                string URI = query.ToString();
                HtmlDocument doc = await GetHtmlFromAsync(URI);
                List<IGame> gamesOnPage = doc.FilterGames();
                games.UnionWith(gamesOnPage);
                if (games.Count == asBefore) break;
                // wait before asking for result again
                await Task.Delay(config.Delay);
            }
            return games.ToList();
        }
        public async Task<List<IGame>> GetTopAsync(int max)
        {
            if (max <= 0) throw new ArgumentOutOfRangeException();

            int pageSize = 100;
            int depth = max / pageSize + (max % pageSize > 0 ? 1 : 0);

            List<IGame> games = new List<IGame>();
            for (int i = 1; i <= depth; i++)
            {
                string URI = string.Format(BGG.URI.Top, i);
                HtmlDocument doc = await GetHtmlFromAsync(URI);
                List<IGame> gamesOnPage = doc.FilterGames();
                games.AddRange(gamesOnPage);
                // wait before asking for result again
                await Task.Delay(config.Delay);
            }
            return games;
        }
        private async Task<HtmlDocument> GetHtmlFromAsync(string URI)
        {
            using (var client = new WebClient())
            {
                // Have to use webclient for https
                Uri uri = new Uri(URI);
                string page = await client.DownloadStringTaskAsync(uri);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(page);
                return doc;
            }
        }
        private async Task<string> GetStringFromAsync(string URI)
        {
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
