using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SemanticWebNPLSearchEngine.Models;
using VDS.RDF.Query;

namespace SemanticWebNPLSearchEngine.Classes
{
    public class MovieSearch
    {
        private readonly MovieDbContext _db = new MovieDbContext();
        private const int MinutesOld = -2;

        /// <summary>
        /// This method is used to run the search process
        /// </summary>
        /// <param name="query">Search string entered by user</param>
        public async Task SearchAsync(string query)
        {
            //Check if search exists
            if (_db.MovieUserSearchTable.Any(o => o.SearchedFor.Equals(query)))
            {
                var queryDate = from s in _db.MovieUserSearchTable
                                where s.SearchedFor == query
                                select s;

                var dateTimeNow = DateTime.Now;
                var dateTimeOldest = dateTimeNow.AddMinutes(MinutesOld);
                var lastUpdated = queryDate.FirstOrDefault().LastUpdated;

                //check date
                if (lastUpdated <= dateTimeNow && lastUpdated >= dateTimeOldest)
                {
                    //Information in the database is within the date period no need to get new information
                }
                else if (lastUpdated < dateTimeOldest)
                {
                    //Information in database is too old, need newer information
                    //Delete current information
                    var moviesSearch = from s in _db.MovieUserSearchTable select s;
                    moviesSearch = moviesSearch.Where(s => s.SearchedFor.Equals(query));
                    foreach (MovieUserSearch i in moviesSearch)
                    {
                        _db.MovieUserSearchTable.Remove(_db.MovieUserSearchTable.Find(i.Id));
                    }
                    try
                    {
                        _db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("ERROR: Removing items from database");
                        Debug.WriteLine(e);
                    }
                    //Request new information
                    await GatherNewDataAsync(query).ConfigureAwait(false);
                }
                else if (lastUpdated > dateTimeNow)
                {
                    Debug.WriteLine("ERROR: Database information is newer than possible");
                }
            }
            else
            {
                //Request new information
                await GatherNewDataAsync(query);
            }
        }

        /// <summary>
        /// Method used to gather new data
        /// </summary>
        /// <param name="query">String: users search query</param>
        private async Task GatherNewDataAsync(string query)
        {
            //Luis.ai report
            LuisJsonModel luisJson = await Utilities.CallLuisAsync(query);
            //Create Sparql query from luis report
            string sparqlQuery = Utilities.ExtractLuisData(luisJson);

            //Dbpedia data
            SparqlResultSet resultSetMovieSearch = Utilities.QueryDbpedia(sparqlQuery);

            //Add to database
            LoopValuesToDatabase(query, resultSetMovieSearch);
        }

        /// <summary>
        /// Methods used to format data and loop to database
        /// </summary>
        /// <param name="searchString">String: Users search string</param>
        /// <param name="resultSet">SparqlResultSet: Result set from DBpedia</param>
        private void LoopValuesToDatabase(string searchString, SparqlResultSet resultSet)
        {
            foreach (SparqlResult result in resultSet)
            {
                string movieLink = result["movieLink"].ToString();
                string title = Utilities.RemoveLast3Cahracters(result["title"].ToString());
                string genreLink = String.Empty;
                if (result["genreLink"] != null)
                {
                    genreLink = result["genreLink"].ToString();
                }
                string genre = "";
                if (result["genre"] != null)
                {
                    genre = Utilities.RemoveLast3Cahracters(result["genre"].ToString());
                }
                string releaseDate = "";
                if (result["releaseDate"] != null)
                {
                    releaseDate = Utilities.DateCreator(result["releaseDate"].ToString());
                }

                AddToDatabase(searchString, movieLink, title, genreLink, genre, releaseDate);
            }
        }

        /// <summary>
        /// Method to add an item to the database
        /// </summary>
        /// <param name="searchString">String: Users search string</param>
        /// <param name="movieLink">String: movie link</param>
        /// <param name="tilte">String: movie title</param>
        /// <param name="genreLink">String: movie genre link</param>
        /// <param name="genre">String: movie genre</param>
        /// <param name="releaseDate">String: Movie release date</param>
        private void AddToDatabase(string searchString, string movieLink, string tilte, string genreLink, string genre, string releaseDate)
        {
            //Create movie search object
            MovieUserSearch movieSearch = new MovieUserSearch
            {
                SearchedFor = searchString,
                MovieLink = movieLink,
                Title = tilte,
                GenreLink = genreLink,
                Genre = genre,
                ReleaseDate = releaseDate
            };

            //Add the new object to the table
            _db.MovieUserSearchTable.Add(movieSearch);

            //Submit change to database
            try
            {
                _db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Adding to database");
                Console.WriteLine(e);
            }
        }
    }
}