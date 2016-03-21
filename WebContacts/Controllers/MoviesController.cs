using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebContacts.Models;

namespace WebContacts.Controllers
{
    public class MoviesController : Controller
    {
        public ActionResult Index()
        {
            // Retreive from the Session dictionary the reference 
            // of the contactsview instance and pass it to the view
            return View(((MoviesView)Session["MoviesView"]).ToList());
        }

        public ActionResult Sort(String sortField)
        {
            // Retreive from the Session dictionary the reference of the contactsview instance
            // and sort it according the parameter sortField
            // (the sort direction is toggled)
            ((MoviesView)Session["MoviesView"]).Sort(sortField);

            // Return the Index action of this controller
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Create()
        {
            // Retreive from the Session dictionary the reference of the contactsview instance
            // and store it in the ViewBag dictionary in order to allow this action
            // view to have acces to it
            ViewBag.Movies = ((MoviesView)Session["MoviesView"]).ToList();

            // Create a new instance of contact and pass it to this action view
            Movie movie = new Movie();
            return View(movie);
        }

        [HttpPost]
        public ActionResult Create(Movie movie)
        {
            // Retreive from the Application dictionary the reference of the contacts instance
            Movies movies = (Movies)HttpRuntime.Cache["Movies"];

            // Pass the Http post Request object to UpLoadAvatar 
            // so that it can retreive its uploaded image file
            movie.UpLoadAvatar(Request);

            // insert the new contact and retreive its Id
            int newMovieID = movies.Add(movie);

            // Retreive from the Application dictionary the reference of the Friends instance
            Friends Friends = (Friends)HttpRuntime.Cache["Friends"];

            // Extract the friends Id list from the hidden input FriendsListItems
            // embedded in the Http post request
            String[] FriendsListItems = Request["FriendsListItems"].Split(',');

            // Add friends to the Friends collection
            foreach (String friendId in FriendsListItems)
            {
                if (!String.IsNullOrEmpty(friendId))
                {
                    Friends.Add(new Friend(newMovieID, int.Parse(friendId)));
                }
            }

            // Return the Index action of this controller
            return RedirectToAction("index");
        }

        [HttpGet]
        public ActionResult Edit(String id)
        {
            // Make sure that this action is called with an id
            if (!String.IsNullOrEmpty(id))
            {
                Movie movieToEdit = ((Movies)HttpRuntime.Cache["Movies"]).Get(int.Parse(id));

                // Make sure the contact exist
                if (movieToEdit != null)
                {
                    // Retreive from the Session dictionary the reference of the ContactsView instance
                    MoviesView moviesView = (MoviesView)Session["MoviesView"];

                    // Retreive from the Application dictionary the reference of the Friends instance
                    Friends Friends = (Friends)HttpRuntime.Cache["Friends"];

                    // Store in ViewBag the friend list of the contact to edit
                    ViewBag.FriendsList = movieToEdit.GetFriendsList(Friends, moviesView);

                    // Store in ViewBag the "not yet friend" contact list  of the contact to edit
                    ViewBag.NotYetFriendsList = movieToEdit.GetNotYetFriendsList(Friends, moviesView);

                    // Pass the contact to edit reference to this action view
                    return View(movieToEdit);
                }
            }

            // Return the Index action of this controller
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Edit(Movie movie)
        {
            // Pass the Http post Request object to UpLoadAvatar 
            // so that it can retreive its uploaded image file
            movie.UpLoadAvatar(Request);

            // Update the contact
            ((Movies)HttpRuntime.Cache["Movies"]).Update(movie);

            // Retreive from the Application dictionary the reference of the Friends instance
            Friends Friends = (Friends)HttpRuntime.Cache["Friends"];

            // Reset the contact friend list
            movie.ClearFriendList(Friends);

            // Extract the friends Id list from the hidden input 
            // FriendsListItems embedded in the Http post request
            String[] FriendsListItems = Request["FriendsListItems"].Split(',');

            // Add friends to the Friends collection
            foreach (String friendId in FriendsListItems)
            {
                if (!String.IsNullOrEmpty(friendId))
                {
                    Friends.Add(new Friend(movie.Id, int.Parse(friendId)));
                }
            }
            // Return the Index action of this controller
            return RedirectToAction("Index");
        }

        public ActionResult Delete(String id)
        {         
            // Make sure that this action is called with an id
            if (!String.IsNullOrEmpty(id))
            {
                ((Movies)HttpRuntime.Cache["Movies"]).Delete(id);
            }
            // Return the Index action of this controller
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Details(String id)
        {
            // Make sure that this action is called with an id
            if (!String.IsNullOrEmpty(id))
            {
                Movie movieToView = ((Movies)HttpRuntime.Cache["Movies"]).Get(int.Parse(id));
                if (movieToView != null)
                {
                    // Store in ViewBag the friend list of the contact to view
                    ViewBag.FriendsList = movieToView.GetFriendsList((Friends)HttpRuntime.Cache["Friends"],
                                                                       (MoviesView)Session["MoviesView"]);

                    // Pass the reference of the contact to view to this action view
                    return View(movieToView);
                }
            }

            // Return the Index action of this controller
            return RedirectToAction("Index");
        }
    }
}