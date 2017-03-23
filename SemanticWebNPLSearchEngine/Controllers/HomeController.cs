using SemanticWebNPLSearchEngine.Classes;
using SemanticWebNPLSearchEngine.Models;
using System;
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

        public async Task<ActionResult> Search(string searchQuery)
        {
            //Run the search method if user has search for an item i.e. id isn't null
            if (!String.IsNullOrEmpty(searchQuery.Trim()))
            {
                await UserSearch.searchAsync(searchQuery);
            }
            else
            {
                Response.Redirect("Index");
            }

            var searchResult = from b in db.movieUserSearchTable select b;
            searchResult = searchResult.Where(s => s.SearchedFor.Equals(searchQuery));

            ViewBag.searchString = searchQuery;
            //asdas
            return View(searchResult);
        }
    }
}