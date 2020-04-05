using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShoppingCart.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }

        public ActionResult CategoryMenuPartial()
        {
            //declare list of CategoryVM
            List<CategoryVM> categoryVMList;

            //Init the list
            using (Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting)
                    .Select(x => new CategoryVM(x))
                    .ToList();
            }

            //return partial with list
            return PartialView(categoryVMList);
        }

        public ActionResult Category(string name)
        {
            //declare a list of ProductVM
            List<ProductVM> productVMList;

            using (Db db = new Db())
            {
                //Get category id
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                int catId = categoryDTO.Id;

                //Init the list
                productVMList = db.Products.ToArray().Where(x => x.CategoryId == catId)
                    .Select(x => new ProductVM(x)).ToList();

                //Get the category name 
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();
                ViewBag.CategoryName = productCat.CategoryName;
            }

            //return view with list
            return View(productVMList);
        }

        //GET: /shop/"product-details/name
        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            //declare the VM and DTO
            ProductVM model;
            ProductDTO dto;

            //init product id
            int id = 0;

            using (Db db = new Db())
            {
                //check if the product exists
                if (! db.Products.Any(x => x.Slug.Equals(name)))
                {
                    RedirectToAction("Index", "Shop");
                }

                //init ProductDTO
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                //get the id
                id = dto.Id;

                //init the model 
                model = new ProductVM(dto);
            }

            //get the gallery images for the product
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Image/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));

            //return the view with the model
            return View("ProductDetails", model);
        }
    }
}