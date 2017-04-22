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

        public async Task<ActionResult> Search(string searchQuery)
        {
            //Run the search method if user has search for an item i.e. id isn't null
            if (!String.IsNullOrEmpty(searchQuery))
            {
                if (!String.IsNullOrEmpty(searchQuery.Trim()))
                {
                    Stopwatch timer = Stopwatch.StartNew();
                    await UserSearch.searchAsync(searchQuery);
                    timer.Stop();
                    Debug.WriteLine(timer.ElapsedMilliseconds);
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
            IEnumerable<MovieUserSearch> model = await searchResult.OrderByDescending(s => s.LastUpdated).ToListAsync();

            return View(model);
        }
    }
}