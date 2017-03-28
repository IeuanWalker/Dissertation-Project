using PagedList;
using SemanticWebNPLSearchEngine.Classes;
using SemanticWebNPLSearchEngine.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SemanticWebNPLSearchEngine.Controllers
{
    public class HomeController : Controller
    {
        //Access to database
        private readonly MovieDBContext db = new MovieDBContext();

        //Access to different classes
        private movieSearch UserSearch = new movieSearch();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "About the project";

            return View();
        }

        public ActionResult LearnMore()
        {
            ViewBag.Message = "Technical aspects and more details about the project";

            return View();
        }

        public async Task<ActionResult> Search(string searchQuery, int page = 1, int pageSize = 10)
        {
            //Run the search method if user has search for an item i.e. id isn't null
            if (!String.IsNullOrEmpty(searchQuery))
            {
                if (!String.IsNullOrEmpty(searchQuery.Trim()))
                {
                    var watch = Stopwatch.StartNew();
                    await UserSearch.searchAsync(searchQuery);
                    watch.Stop();


                    Console.WriteLine(watch);
                    Console.WriteLine("TIMER");
                }
                else
                {
                    Response.Redirect("Index");
                }
            }
            else
            {
                Response.Redirect("Index");
            }
            ViewBag.searchString = searchQuery;

            var searchResult = from b in db.movieUserSearchTable select b;
            searchResult = searchResult.Where(s => s.SearchedFor.Equals(searchQuery));

            //Create a list
            IEnumerable<MovieUserSearch> movieList = await searchResult.OrderByDescending(s => s.LastUpdated).ToListAsync();

            //Using pagedList package to add pagination
            PagedList<MovieUserSearch> model = new PagedList<MovieUserSearch>(movieList, page, pageSize);

            return View(model);
        }
    }
}