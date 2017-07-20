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
        private readonly MovieDbContext _db = new MovieDbContext();

        //Access to different classes
        private readonly MovieSearch _userSearch = new MovieSearch();

        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> Search(string searchQuery)
        {
            //Run the search method if user has search for an item i.e. id isn't null
            if (String.IsNullOrEmpty(searchQuery) || String.IsNullOrEmpty(searchQuery.Trim()))
            {
                Response.Redirect("Index");
            }
            else
            {
                Stopwatch timer = Stopwatch.StartNew();
                await _userSearch.SearchAsync(searchQuery);
                timer.Stop();
                Debug.WriteLine(timer.ElapsedMilliseconds);
            }

            ViewBag.searchString = searchQuery;

            var searchResult = from b in _db.MovieUserSearchTable select b;
            searchResult = searchResult.Where(s => s.SearchedFor.Equals(searchQuery));

            //Create a list
            IEnumerable<MovieUserSearch> model = await searchResult.OrderByDescending(s => s.LastUpdated).ToListAsync();

            return View(model);
        }
    }
}