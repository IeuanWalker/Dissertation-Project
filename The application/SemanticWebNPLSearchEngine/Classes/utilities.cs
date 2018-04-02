using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Configuration;
using DissertationOriginal.Models;
using Newtonsoft.Json;
using VDS.RDF.Query;

namespace DissertationOriginal.Classes
{
    public static class Utilities
    {
        /// <summary>
        /// Method to query the Luis.ai
        /// </summary>
        /// <param name="query">String: Users search query</param>
        /// <returns>LuisJSONModel: Object containing luis.ai data</returns>
        public static async Task<LuisJsonModel> CallLuisAsync(string query)
        {
            LuisJsonModel data = new LuisJsonModel();
            using (HttpClient client = new HttpClient())
            {
                string luisUrl = WebConfigurationManager.AppSettings["LUIS_Url"];
                string luisId = WebConfigurationManager.AppSettings["LUIS_Id"];
                string luisSubscriptionKey = WebConfigurationManager.AppSettings["LUIS_Subscription_Key"];
                string luisQuery = Uri.EscapeDataString(query);

                string requestUri = String.Format("{0}{1}?subscription-key={2}&verbose=true&q={3}", luisUrl, luisId, luisSubscriptionKey, luisQuery);
                Console.WriteLine(requestUri);

                HttpResponseMessage msg = await client.GetAsync(requestUri);

                if (msg.IsSuccessStatusCode)
                {
                    var jsonDataResponse = await msg.Content.ReadAsStringAsync();
                    Debug.WriteLine(jsonDataResponse);
                    data = JsonConvert.DeserializeObject<LuisJsonModel>(jsonDataResponse);
                }
            }
            return data;
        }

        /// <summary>
        /// Method to extract data from the Luis.ai JSON file
        /// </summary>
        /// <param name="luisJson">LuisJSONModel: Method used to extract data from the LuisJSONModel</param>
        /// <returns>String: Custom SPARQL query</returns>
        public static string ExtractLuisData(LuisJsonModel luisJson)
        {
            int numberOfItems = 0;
            string genre = "";
            int year = 0;
            string exactDate = "";

            foreach (var i in luisJson.Entities)
            {
                switch (i.Type)
                {
                    case "builtin.number":
                        if (int.TryParse(i.Resolution.Value, out int number))
                        {
                            if (number < 1000)
                            {
                                numberOfItems = number;
                            }
                        }
                        break;

                    case "genre":
                        genre = i.entity;
                        break;

                    case "builtin.datetime.date":
                        if (DateTime.TryParse(i.entity, out DateTime exactDateTime))
                        {
                            exactDate = exactDateTime.ToString();
                        }
                        else if (int.TryParse(i.entity, out int yearDateTime) && (i.entity.Length == 4))
                        {
                            year = yearDateTime;
                        }
                        break;
                }
            }
            return CreateSparqlQuery(numberOfItems, genre, year, exactDate);
        }

        /// <summary>
        /// Method to create custom SPARQL query
        /// </summary>
        /// <param name="numberOfItems">Int: number of items that will be returned</param>
        /// <param name="genre">String: Genre of the movie</param>
        /// <param name="year">Int: Year of the films</param>
        /// <param name="exactDate">String: The exact date of the films</param>
        /// <returns>String: Returns a custom SPARQL Query</returns>
        private static string CreateSparqlQuery(int numberOfItems, string genre, int year, string exactDate)
        {
            string limit = numberOfItems > 0 ? String.Format("LIMIT({0})", numberOfItems) : "";
            string genreMatch = String.IsNullOrEmpty(genre.Trim()) ? "" : String.Format("FILTER ( regex (str(?genre), '{0}', 'i'))", genre);
            string dateMatch = "";

            if (exactDate.Equals(DateTime.Now) && year.Equals(0))
            {
                //Means that both haven't been assigned
            }
            else if (!String.IsNullOrEmpty(exactDate.Trim()))
            {
                dateMatch = String.Format("FILTER ( regex (str(?releaseDate), '{0}', 'i'))", exactDate);
            }
            else if (!year.Equals(0))
            {
                dateMatch = String.Format("FILTER ((?releaseDate >= '{0}-01-01'^^xsd:date) && (?releaseDate < '{0}-12-31'^^xsd:date))", year);
            }

            string queryPattern =
                "PREFIX xsd: <http://www.w3.org/2001/XMLSchema#> " +
                "PREFIX db: <http://dbpedia.org/ontology/> " +
                "PREFIX prop: <http://dbpedia.org/property/> " +
                "SELECT ?movieLink ?title ?genreLink ?genre ?releaseDate " +
                "WHERE {{ " +
                    "?movieLink rdf:type db:Film; " +
                               "foaf:name ?title. " +
                    "OPTIONAL {{ ?movieLink prop:genre ?genreLink. " +
                               "?genreLink rdfs:label ?genre. " +
                               "FILTER(lang(?genre) = 'en') }}. " +
                    "OPTIONAL {{ ?movieLink <http://dbpedia.org/ontology/releaseDate> ?releaseDate }}. " +

                    "{0}" +
                    "{1}" +
                    "FILTER(lang(?title) = 'en') " +
                "}}" +
                "ORDER BY DESC(?releaseDate)" +
                "{2}";

            Debug.WriteLine(String.Format(queryPattern, genreMatch, dateMatch, limit));
            return String.Format(queryPattern, genreMatch, dateMatch, limit);
        }

        /// <summary>
        /// Method to Query Dbpedia and return a Sparql Result set
        /// </summary>
        /// <param name="query">The SPARQL query</param>
        /// <returns>SparqlResutSet containing results from DBpedia</returns>
        public static SparqlResultSet QueryDbpedia(string query)
        {
            //Define a remote endpoint
            //Use the DBPedia SPARQL endpoint with the default Graph set to DBPedia
            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql"), "http://dbpedia.org");

            //Make a SELECT query against the Endpoint
            SparqlResultSet results = endpoint.QueryWithResultSet(query);

            return results;
        }

        /// <summary>
        /// Method to remove the @en at the end of strings
        /// </summary>
        /// <param name="word">String with '@en' at the end</param>
        /// <returns>The same word without '@en' at the end</returns>
        public static string RemoveLast3Cahracters(string word)
        {
            string result = String.Empty;
            if (word.Length > 3)
            {
                result = word.Substring(0, word.Length - 3);
            }
            return result;
        }

        /// <summary>
        /// Method to return just the date
        /// </summary>
        /// <param name="word">date string returned from DBpedia</param>
        /// <returns>The returned data as string</returns>
        public static string DateCreator(string word)
        {
            string date = String.Empty;
            if (!string.IsNullOrEmpty(word))
            {
                int index = word.IndexOf("^", StringComparison.Ordinal);
                date = word.Substring(0, index);
            }

            return date;
        }
    }
}