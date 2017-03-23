using SemanticWebNPLSearchEngine.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VDS.RDF.Query;

namespace SemanticWebNPLSearchEngine.Classes
{
    public class movieSearch
    {
        private readonly MovieDBContext db = new MovieDBContext();
        private int minutesOld = -2;

        public async Task searchAsync(string query)
        {
            //Check if search exists
            if (db.movieUserSearchTable.Any(o => o.SearchedFor.Equals(query)))
            {
                var queryDate = from s in db.movieUserSearchTable
                                where s.SearchedFor == query
                                select s;

                var dateTimeNow = DateTime.Now;
                var dateTimeOldest = dateTimeNow.AddMinutes(minutesOld);
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
                    var moviesSearch = from s in db.movieUserSearchTable select s;
                    moviesSearch = moviesSearch.Where(s => s.SearchedFor.Equals(query));
                    foreach (MovieUserSearch i in moviesSearch)
                    {
                        db.movieUserSearchTable.Remove(db.movieUserSearchTable.Find(i.ID));
                    }
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("ERROR: Removing items from database");
                        Debug.WriteLine(e);
                    }
                    //Request new information
                    await gatherNewDataAsync(query);
                }
                else if (lastUpdated > dateTimeNow)
                {
                    Debug.WriteLine("ERROR: Database information is newer than possible");
                }
            }
            else
            {
                //Request new information
                await gatherNewDataAsync(query);
            }
        }

        private async Task gatherNewDataAsync(string query)
        {
            //Luis.ai report
            LuisJSONModel LuisJSON = await utilities.callLuisAsync(query);
            //Create Sparql query from luis report
            string sparqlQuery = utilities.extractLuisData(LuisJSON);

            //Dbpedia data
            SparqlResultSet resultSetMovieSearch = utilities.QueryDbpedia(sparqlQuery);

            //Add to database
            loopValuesToDatabase(query, resultSetMovieSearch);
        }

        private void loopValuesToDatabase(string searchString, SparqlResultSet resultSet)
        {
            foreach (SparqlResult result in resultSet)
            {
                string movieLink = result["movieLink"].ToString();
                string title = result["title"].ToString();
                string genreLink = String.IsNullOrEmpty(result["genreLink"].ToString()) ? "" : result["genreLink"].ToString();
                string genre = String.IsNullOrEmpty(result["genre"].ToString()) ? "" : result["genre"].ToString();

                string releaseDate = "";
                if (!(result["releaseDate"] == null))
                {
                    releaseDate = result["releaseDate"].ToString();
                }

                AddToDatabase(searchString, movieLink, title, genreLink, genre, releaseDate);
            }
        }

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
            db.movieUserSearchTable.Add(movieSearch);

            //Submit change to database
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Adding to database");
                Console.WriteLine(e);
            }
        }
    }
}