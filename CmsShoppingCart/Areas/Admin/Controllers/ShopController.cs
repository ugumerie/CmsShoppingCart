using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using PagedList;
using CmsShoppingCart.Areas.Admin.Models.ViewModels.Shop;

namespace CmsShoppingCart.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShopController : Controller
    {
        // GET: Admin/Shop
        public ActionResult Categories()
        {
            //declare a list of models
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {
                //init the list 
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting)
                    .Select(x => new CategoryVM(x))
                    .ToList();
            }

            //return the view with list
            return View(categoryVMList);
        }

        [HttpPost]
        public string AddNewCategory(string catName)
        {
            //declare the id
            string id;

            using (Db db = new Db())
            {
                //check that the category name is unique
                if (db.Categories.Any(x => x.Name == catName))
                    return "titletaken";

                //init DTO
                CategoryDTO dto = new CategoryDTO();

                //add to dto
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                //save DTO
                db.Categories.Add(dto);
                db.SaveChanges();

                //get the id 
                id = dto.Id.ToString();
            }

            //return id
            return id;
        }

        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                //set initial count
                int count = 1; //since home is 0

                //declare the CategoryDTO
                CategoryDTO dto;

                //set sorting for each category 
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);

                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }
        }

        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                //get the category 
                CategoryDTO dto = db.Categories.Find(id);

                //remove the category
                db.Categories.Remove(dto);

                //save 
                db.SaveChanges();
            }

            //redirect
            return RedirectToAction("Categories");
        }

        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                //check category name is unique
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "titletaken";

                //get DTO
                CategoryDTO dto = db.Categories.Find(id);

                //edit DTO
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                //save 
                db.SaveChanges();
            }

            //return
            return "ok";
        }

        public ActionResult AddProduct()
        {
            //init the model
            ProductVM model = new ProductVM();

            //add select list of categories to model
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            //return view with model
            return View(model);
        }

        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            //check model state
            if (! ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }

            //make sure product name is unique
            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);
                }
            }

            //declare product id
            int id;

            //init and save productDTO
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();

                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;

                db.Products.Add(product);
                db.SaveChanges();

                //get the id
                id = product.Id;
            }

            //set TempData message
            TempData["SM"] = "You have added a product!";

            #region Upload Image
            //create the necessary directories
            var originalDirectory = new DirectoryInfo(string.Format("{0}Image\\Uploads", Server.MapPath(@"\")));

          
            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);

            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);

            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);

            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);

            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            //check if a file was uploaded
            if (file != null && file.ContentLength > 0)
            {
                //get the file extension
                string ext = file.ContentType.ToLower();

                //verify extension
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        if (db.Products.Any(x => x.Name == model.Name))
                        {
                            model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                            ModelState.AddModelError("", "The image was not uploaded - wrong image extension.");
                            return View(model);
                        }
                    }
                }

                //init image name
                string imageName = file.FileName;

                //save image name to DTO
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                //set original and thumb image paths
                var path = string.Format("{0}\\{1}", pathString2, imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);

                //save original image
                file.SaveAs(path);

                //create and save thumb 
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }
            #endregion

            //redirect
            return RedirectToAction("AddProduct");
        }

        public ActionResult Products(int? page, int? catId)
        {
            //declare list of productVM
            List<ProductVM> listOfProductVM;

            //set page number
            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {
                //init the list
                listOfProductVM = db.Products.ToArray()
                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                    .Select(x => new ProductVM(x))
                    .ToList();

                //populate categories select list
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //set selected category 
                ViewBag.SelectedCat = catId.ToString();
            }

            //set pagination
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.OnePageOfProducts = onePageOfProducts;

            //return view with list
            return View(listOfProductVM);
        }

        public ActionResult EditProduct(int id)
        {
            //declare productVM
            ProductVM model;

            using (Db db = new Db())
            {
                //get the product
                ProductDTO dto = db.Products.Find(id);

                //make sure the product exists
                if (dto == null)
                {
                    return Content("That product does not exist.");
                }

                //init model
                model = new ProductVM(dto);

                //make a select list
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //get all gallery images
                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Image/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));
            }

            //return view with model
            return View(model);
        }

        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            //get product id
            int id = model.Id;

            //populate categories select list and gallery
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Image/Uploads/Products/" + id + "/Gallery/Thumbs"))
                   .Select(fn => Path.GetFileName(fn));

            //check model state
            if (! ModelState.IsValid)
            {
                return View(model);
            }

            //make sure product name is unique
            using (Db db = new Db())
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);
                }
            }

            //update product
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;

                db.SaveChanges();
            }

            //set TempData message
            TempData["SM"] = "You have edited the product!";

            #region Image Upload
            //check for file upload
            if (file != null && file.ContentLength > 0)
            {
                //get extension
                string ext = file.ContentType.ToLower();

                //verify extension
                //verify extension
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        if (db.Products.Any(x => x.Name == model.Name))
                        {
                            ModelState.AddModelError("", "The image was not uploaded - wrong image extension.");
                            return View(model);
                        }
                    }
                }

                //set upload directory path
                var originalDirectory = new DirectoryInfo(string.Format("{0}Image\\Uploads", Server.MapPath(@"\")));

                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                //delete files from directories
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (FileInfo file2 in di1.GetFiles())
                    file2.Delete();

                foreach (FileInfo file3 in di2.GetFiles())
                    file3.Delete();

                //save image name
                string imageName = file.FileName;

                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                //save original and thumb images
                var path = string.Format("{0}\\{1}", pathString1, imageName);
                var path2 = string.Format("{0}\\{1}", pathString2, imageName);

                //save original image
                file.SaveAs(path);

                //create and save thumb 
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }
            #endregion

            //redirect
            return RedirectToAction("EditProduct");
        }

        public ActionResult DeleteProduct(int id)
        {
            //delete product from db
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);

                db.SaveChanges();
            }

            //delete product folder
            var originalDirectory = new DirectoryInfo(string.Format("{0}Image\\Uploads", Server.MapPath(@"\")));
            var pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
                Directory.Delete(pathString, true);
            
            //redirect
            return RedirectToAction("Products");
        }

        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            //loop through the files
            foreach (string fileName in Request.Files)
            {
                //init the file
                HttpPostedFileBase file = Request.Files[fileName];

                //check it's not null
                if (file != null && file.ContentLength > 0)
                {
                    //set directory path
                    var originalDirectory = new DirectoryInfo(string.Format("{0}Image\\Uploads", Server.MapPath(@"\")));

                    string pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    //set image path
                    var path = string.Format("{0}\\{1}", pathString1, file.FileName);
                    var path2 = string.Format("{0}\\{1}", pathString2, file.FileName);


                    //save the original image and the thumb  
                    file.SaveAs(path);

                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200);
                    img.Save(path2);
                }
            }
        }

        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Image/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
            string fullPath2 = Request.MapPath("~/Image/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);

            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);
        }

        public ActionResult Orders()
        {
            //init list of OrdersForAdminVM
            List<OrdersForAdminVM> ordersForAdmin = new List<OrdersForAdminVM>();

            using (Db db = new Db())
            {
                //init list of OrderVM
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();

                //loop through list of OrderVM
                foreach (var order in orders)
                {
                    //init product dictionary
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

                    //declare total
                    decimal total = 0m;

                    //init a list OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(x => x.OrderId == order.OrderId)
                        .ToList();

                    //get username
                    UserDTO user = db.Users.Where(x => x.Id == order.UserId).FirstOrDefault();
                    string username = user.Username;

                    //loop through list OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsList)
                    {
                        //get product
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        //get product price
                        decimal price = product.Price;

                        //get product name
                        string productName = product.Name;

                        //Add to product dict
                        productsAndQty.Add(productName, orderDetails.Quantity);

                        //get total
                        total += orderDetails.Quantity * price;
                    }

                    //Add to OrdersForAdminVM list
                    ordersForAdmin.Add(new OrdersForAdminVM
                    {
                        OrderNumber = order.OrderId,
                        Username = username,
                        Total = total,
                        ProductsAndQty = productsAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }
            }

            //return the view with OrdersForAdminVM list
            return View(ordersForAdmin);
        }
    }
}