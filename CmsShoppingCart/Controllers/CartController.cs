using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShoppingCart.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            //init the cart list
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            //check if cart is empty
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Your cart is empty.";
                return View();
            }

            //calculate total and save to the ViewBag
            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;

            //return with the list
            return View(cart);
        }

        public ActionResult CartPartial()
        {
            //Init CartVM
            CartVM model = new CartVM();

            //init cart quantity    
            int qty = 0;

            //init price
            decimal price = 0m;

            //check for cart session

            if (Session["cart"] != null)
            {
                //get total quantity and price 
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;
                }
                model.Quantity = qty;
                model.Price = price;
            }
            else
            {
                //or set quantity and price to 0    
                model.Quantity = 0;
                model.Price = 0m;
            }

            //return partial view with model
            return PartialView(model);
        }

        public ActionResult AddToCartPartial(int id)
        {
            //init CartVM list
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            //init CartVM
            CartVM model = new CartVM();

            using (Db db = new Db())
            {
                //get the product
                ProductDTO product = db.Products.Find(id);

                //check if the product is already in cart
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                //if not, add new
                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });
                }
                else
                {
                    //if it is, increment 
                    productInCart.Quantity++;
                }

            }

            //get total quantity and price and add it to the model
            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }
            model.Quantity = qty;
            model.Price = price;

            //save the cart back to session
            Session["cart"] = cart;

            //return the partial view with the model
            return PartialView(model);
        }

        public JsonResult IncrementProduct(int productId)
        {
            //init cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                //Get CartVM from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                //increment qty
                model.Quantity++;

                //store needed data
                var result = new { qty = model.Quantity, price = model.Price };

                //return json with data 
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult DecrementProduct(int productId)
        {
            //init cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                //Get CartVM (model) from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                //decrement qty
                if (model.Quantity > 1)
                {
                    model.Quantity--;
                }
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }
               
                //store needed data
                var result = new { qty = model.Quantity, price = model.Price };

                //return json with data 
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public void RemoveProduct(int productId)
        {
            //init the cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                //get model from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                //remove model from list 
                cart.Remove(model);
            }
        }
    }
}