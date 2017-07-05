using Newtonsoft.Json;
using SemanticWebNPLSearchEngine.Models;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Configuration;
using VDS.RDF.Query;

namespace SemanticWebNPLSearchEngine.Classes
{
    public static class utilities

    {
        /// <summary>
        /// Method to query the Luis.ai
        /// </summary>
        /// <param name="Query">String: Users search query</param>
        /// <returns>LuisJSONModel: Object containing luis.ai data</returns>
        public static async Task<LuisJSONModel> CallLuisAsync(string Query)
        {
            LuisJSONModel Data = new LuisJSONModel();
            using (HttpClient client = new HttpClient())
            {
                string LUIS_Url = WebConfigurationManager.AppSettings["LUIS_Url"];
                string LUIS_Id = WebConfigurationManager.AppSettings["LUIS_Id"];
                string LUIS_Subscription_Key = WebConfigurationManager.AppSettings["LUIS_Subscription_Key"];
                string LUIS_Query = Uri.EscapeDataString(Query);

                string RequestUri = $"{LUIS_Url}{LUIS_Id}?subscription-key={LUIS_Subscription_Key}&verbose=true&q={LUIS_Query}";
                Console.WriteLine(RequestUri);

                HttpResponseMessage msg = await client.GetAsync(RequestUri);

                if (msg.IsSuccessStatusCode)
                {
                    var JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    Debug.WriteLine(JsonDataResponse);
                    Data = JsonConvert.DeserializeObject<LuisJSONModel>(JsonDataResponse);
                }
            }
            return Data;
        }

        /// <summary>
        /// Method to extract data from the Luis.ai JSON file
        /// </summary>
        /// <param name="luisJson">LuisJSONModel: Method used to extract data from the LuisJSONModel</param>
        /// <returns>String: Custom SPARQL query</returns>
        public static string ExtractLuisData(LuisJSONModel luisJson)
        {
            #region private variables

            int numberOfItems = 0;
            string genre = String.Empty;
            int year = 0;
            string exactDate = String.Empty;
            int number;
            DateTime exactDateTime;
            int yearDateTime;

            #endregion private variables

            foreach (var i in luisJson.entities)
            {
                switch (i.type)
                {
                    case "builtin.number":
                        if (int.TryParse(i.resolution.value, out number))
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
                        if (DateTime.TryParse(i.entity, out exactDateTime))
                        {
                            exactDate = exactDateTime.ToString();
                        }
                        else if (int.TryParse(i.entity, out yearDateTime) && (i.entity.Length == 4))
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
            string limit = numberOfItems > 0 ? $"LIMIT({numberOfItems})" : String.Empty;
            string genreMatch = String.IsNullOrEmpty(genre.Trim()) ? String.Empty : $"FILTER ( regex (str(?genre), '{genre}', 'i'))";
            string dateMatch = String.Empty;

            if (exactDate.Equals(DateTime.Now) && year.Equals(0))
            {
                //Means that both haven't been assigned
            }
            else if (!String.IsNullOrEmpty(exactDate.Trim()))
            {
                dateMatch = $"FILTER ( regex (str(?releaseDate), '{exactDate}', 'i'))";
            }
            else if (!year.Equals(0))
            {
                dateMatch = $"FILTER ((?releaseDate >= '{year}-01-01'^^xsd:date) && (?releaseDate < '{year}-12-31'^^xsd:date))";
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
            if (word.Length > 3)
            {
                word = word.Substring(0, word.Length - 3);
            }
            return word;
        }

        /// <summary>
        /// Method to return just the date
        /// </summary>
        /// <param name="word">date string returned from DBpedia</param>
        /// <returns>The returned data as string</returns>
        public static string DateCreator(string word)
        {
            if (!(string.IsNullOrEmpty(word)))
            {
                int index = word.IndexOf("^", StringComparison.Ordinal);
                word = word.Substring(0, index);
            }
            return word;
        }
    }
}