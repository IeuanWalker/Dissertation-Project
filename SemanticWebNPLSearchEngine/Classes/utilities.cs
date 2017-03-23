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
    public class utilities
    {
        public static async Task<LuisJSONModel> callLuisAsync(string Query)
        {
            LuisJSONModel Data = new LuisJSONModel();
            using (HttpClient client = new HttpClient())
            {
                string LUIS_Url = WebConfigurationManager.AppSettings["LUIS_Url"];
                string LUIS_Id = WebConfigurationManager.AppSettings["LUIS_Id"];
                string LUIS_Subscription_Key = WebConfigurationManager.AppSettings["LUIS_Subscription_Key"];
                string LUIS_Query = Uri.EscapeDataString(Query);

                string RequestUri = String.Format("{0}{1}?subscription-key={2}&verbose=true&q={3}", LUIS_Url, LUIS_Id, LUIS_Subscription_Key, LUIS_Query);
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

        public static string extractLuisData(LuisJSONModel luisJson)
        {
            int numberOfItems = 0;
            string genre = "";
            int year = 0;
            string exactDate = "";

            foreach (var i in luisJson.entities)
            {
                switch (i.type)
                {
                    case "builtin.number":
                        if (int.TryParse(i.resolution.value, out int number))
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

        //Method to Query Dbpedia and return a Sparql Result set
        public static SparqlResultSet QueryDbpedia(string query)
        {
            //Define a remote endpoint
            //Use the DBPedia SPARQL endpoint with the default Graph set to DBPedia
            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql"), "http://dbpedia.org");

            //Make a SELECT query against the Endpoint
            SparqlResultSet results = endpoint.QueryWithResultSet(query);

            return results;
        }
    }
}

//PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
//PREFIX db: <http://dbpedia.org/ontology/>
//PREFIX prop: <http://dbpedia.org/property/>
//SELECT ?movieLink ?title ?genreLink ?genre ?releaseDate
//WHERE {
//    ?movieLink rdf:type db:Film;
//               foaf:name ?title.
//    OPTIONAL { ?movieLink prop:genre ?genreLink.
//               ?genreLink rdfs:label ?genre.
//               FILTER(lang(?genre) = 'en') }.
//    OPTIONAL{ ?movieLink <http://dbpedia.org/ontology/releaseDate> ?releaseDate }.

//    FILTER(lang(?title) = 'en')
//    FILTER((?releaseDate >= '2010-01-01'^^xsd:date) && (?releaseDate < '2010-12-31'^^xsd:date))
//}
//ORDER BY DESC(?releaseDate)
//LIMIT(100)

//string query = String.Format(
//                    "PREFIX xsd: <http://www.w3.org/2001/XMLSchema#> " +
//                    "PREFIX db: <http://dbpedia.org/ontology/> " +
//                    "PREFIX prop: < http://dbpedia.org/property/> " +
//                    "SELECT? movieLink ?title? genreLink ?genre? releaseDate " +
//                    "WHERE { " +
//                        "?movieLink rdf:type db:Film; " +
//                                   "foaf:name ?title. " +
//                        "OPTIONAL { ?movieLink prop:genre ?genreLink. " +
//                                   "?genreLink rdfs:label ?genre. " +
//                                   "FILTER(lang(?genre) = 'en') }. " +
//                        "OPTIONAL{ ?movieLink <http://dbpedia.org/ontology/releaseDate> ?releaseDate }. " +

//                        "{0}" +
//                        "{1}" +
//                        "FILTER(lang(?title) = 'en') " +
//                    "}" +
//                    "ORDER BY DESC(?releaseDate)" +
//                    "{2}"
//                    , genreMatch, dateMatch, limit
//                 );