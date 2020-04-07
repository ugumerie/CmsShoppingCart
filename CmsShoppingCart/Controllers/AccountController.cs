using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Account;
using CmsShoppingCart.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace CmsShoppingCart.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return Redirect("~/account/login");
        }

        // GET: /account/login
        public ActionResult Login()
        {
            //confirm user is not logged in
            string username = User.Identity.Name;

            if (!string.IsNullOrEmpty(username))
                return RedirectToAction("user-profile");

            //return the view
            return View();
        }

        // POST: /account/login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            //check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //check if the user is valid
            bool isValid = false;

            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.Username.Equals(model.Username)) && 
                    db.Users.Any(x => x.Password.Equals(model.Password)))
                {
                    isValid = true;
                }

                if (! isValid)
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View(model);
                }
                else
                {
                    FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                    return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
                }
            }
        }

        // GET: /account/create-account
        [ActionName("create-account")]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        // POST: /account/create-account
        [ActionName("create-account"), HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            //check model state
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }

            //if passwords match
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Passwords do not match");
                return View("CreateAccount", model);
            }

            using (Db db = new Db())
            {
                //make sure the username is unique
                if (db.Users.Any(x => x.Username.Equals(model.Username)))
                {
                    ModelState.AddModelError("", "Username '"+ model.Username +"' is taken.");
                    model.Username = "";
                    return View("CreateAccount", model);
                }

                //create UserDTO
                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    Username = model.Username,
                    Password = model.Password
                };

                //add the DTO
                db.Users.Add(userDTO);

                //save
                db.SaveChanges();

                //add to UserRolesDTO
                int id = userDTO.Id;
                UserRoleDTO userRoleDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };

                db.UserRoles.Add(userRoleDTO);
                db.SaveChanges();
                
            }

            //create TempData message 
            TempData["SM"] = "You are now registered and can login";

            //redirect
            return Redirect("~/account/login");
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/account/login");
        }

        [Authorize]
        public ActionResult UserNavPartial()
        {
            //get the username
            string username = User.Identity.Name;

            //declare the model
            UserNavPartialVM model;

            using (Db db = new Db())
            {
                //get the user
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);

                //build the model 
                model = new UserNavPartialVM
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };
            }

            //return partial view with the model
            return PartialView(model);
        }

        [ActionName("user-profile"), Authorize]
        public ActionResult UserProfile()
        {
            //get the username
            string username = User.Identity.Name;

            //declare the model
            UserProfileVM model;

            using (Db db = new Db())
            {
                //get the user
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);

                //build the model 
                model = new UserProfileVM(dto);
            }

            //return view with model
            return View("UserProfile", model);
        }

        [HttpPost, ActionName("user-profile"), Authorize]
        public ActionResult UserProfile(UserProfileVM model)
        {
            //check model state
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            //check if passwords match if need be
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Passwords do not match.");
                    return View("UserProfile", model);
                }
            }

            using (Db db = new Db())
            {
                //get the username
                string username = User.Identity.Name;

                //make sure username is unique
                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.Username == username))
                {
                    ModelState.AddModelError("", "Username '" + model.Username + "' already exist.");
                    model.Username = "";
                    return View("UserProfile", model);
                }

                //edit DTO
                UserDTO dto = db.Users.Find(model.Id);

                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;
                dto.Username = model.Username;

                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    dto.Password = model.Password;
                }

                //save 
                db.SaveChanges();
            }

            //set tempData message
            TempData["SM"] = "You have edited your profile!";

            //redirect
            return Redirect("~/account/user-profile");
        }

        [Authorize(Roles = "User")]
        public ActionResult Orders()
        {
            //init list of OrdersForUserVM
            List<OrdersForUserVM> ordersForUser = new List<OrdersForUserVM>();

            using (Db db = new Db())
            {
                //get the userId
                UserDTO user = db.Users.Where(x => x.Username == User.Identity.Name).FirstOrDefault();
                int userId = user.Id;

                //init list of OrderVM
                List<OrderVM> orders = db.Orders.Where(x => x.UserId == userId).ToArray()
                    .Select(x => new OrderVM(x)).ToList();

                //loop through list of OrderVM 
                foreach (var order in orders)
                {
                    //init product dictionary
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

                    //declare total
                    decimal total = 0m;

                    //init a list of OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsDTO = db.OrderDetails.Where(x => x.OrderId == order.OrderId)
                        .ToList();

                    //loop through the list of OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsDTO)
                    {
                        //get product
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId)
                            .FirstOrDefault();

                        //get product price
                        decimal price = product.Price;

                        //get product name
                        string productName = product.Name;

                        //add to product dictionary
                        productsAndQty.Add(productName, orderDetails.Quantity);

                        //get total
                        total += orderDetails.Quantity * price;
                    }

                    //Add to OrdersForUserVM list
                    ordersForUser.Add(new OrdersForUserVM
                    {
                        OrderNumber = order.OrderId,
                        Total = total,
                        ProductsAndQty = productsAndQty,
                        CreatedAt = order.CreatedAt,
                    });
                }
            }

            //return view with the list of OrdersForUserVM
            return View(ordersForUser);
        }
    }
}