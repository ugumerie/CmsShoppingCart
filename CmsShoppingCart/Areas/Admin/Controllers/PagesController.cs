using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CmsShoppingCart.Models.ViewModels.Pages;
using CmsShoppingCart.Models.Data;

namespace CmsShoppingCart.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            //declare list of pagevm
            List<PageVM> pageList;


            using (Db db = new Db())
            {
                //init list
                pageList = db.Pages.ToArray().OrderBy(x => x.Sorting)
                    .Select(x => new PageVM(x)).ToList();
            }

            //return view
            return View(pageList);
        }

        public ActionResult AddPage()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            //check the model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using (Db db = new Db())
            {
                //declare slug
                string slug;

                //init pageDTO
                PageDTO dto = new PageDTO();

                //DTO title
                dto.Title = model.Title;

                //check for and set slug if need be
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }

                //make sure title and slug are unique
                if (db.Pages.Any(x => x.Title == model.Title) || db.Pages.Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "That title or slug already exists.");
                    return View(model);
                }

                //DTO the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;

                //save DTO 
                db.Pages.Add(dto);
                db.SaveChanges();
            }

            //set the TempData Message
            TempData["SM"] = "You have added a new page!";

            //redirect

            return RedirectToAction("AddPage");
        }

        public ActionResult EditPage(int id)
        {
            //declare PageVM
            PageVM model;

            using (Db db = new Db())
            {
                //get the page
                PageDTO dto = db.Pages.Find(id);

                //confirm page exists
                if (dto == null)
                {
                    return Content("The page does not exist");
                }

                //Init pageVM 
                model = new PageVM(dto);
            }

            //return view with the model
            return View(model);
        }

        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            //check the model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                //get the page id
                int id = model.Id;

                //init the slug
                string slug = "home";

                //get the page 
                PageDTO dto = db.Pages.Find(id);

                //DTO the title
                dto.Title = model.Title;

                //check for slug and set it if need be
                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }

                //make sure the title and the slug are unique
                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title) ||
                    db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "The title or slug already exists.");
                    return View(model);
                }
                //DTO the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;

                //save the DTO 
                db.SaveChanges();
            }

            //set the TempData message
            TempData["SM"] = "You have edited the page!";

            //redirect
            return RedirectToAction("EditPage");
        }

        public ActionResult PageDetails(int id)
        {
            //Declare PageVM
            PageVM model;

            using (Db db = new Db())
            {
                //get the page
                PageDTO dto = db.Pages.Find(id);

                //confirm the page exists
                if (dto == null)
                {
                    return Content("The page does not exist.");
                }

                //Init PageVM 
                model = new PageVM(dto);
            }

            //return view
            return View(model);
        }

        public ActionResult DeletePage(int id)
        {
            using (Db db = new Db())
            {
                //get the page 
                PageDTO dto = db.Pages.Find(id);

                //remove the page
                db.Pages.Remove(dto);

                //save 
                db.SaveChanges();
            }

            //redirect
            return RedirectToAction("Index");
        }

        [HttpPost]
        public void ReorderPages(int[] id)
        {
            using (Db db = new Db())
            {
                //set initial count
                int count = 1; //since home is 0

                //declare the pageDTO
                PageDTO dto;

                //set sorting for each page 
                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);

                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }
        }

        public ActionResult EditSidebar()
        {
            //declare model
            SidebarVM model;

            using (Db db = new Db())
            {
                //get the DTO
                SidebarDTO dto = db.Sidebar.Find(1);

                //init the model 
                model = new SidebarVM(dto);
            }

            //return view with the model
            return View(model);
        }

        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using (Db db = new Db())
            {
                //Get the DTO
                SidebarDTO dto = db.Sidebar.Find(1);

                //DTO the body
                dto.Body = model.Body;

                //save 
                db.SaveChanges();
            }

            //set TempData
            TempData["SM"] = "You have edited the sidebar!";

            //redirect
            return RedirectToAction("EditSidebar");
        }
    }
}