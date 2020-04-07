﻿using CmsShoppingCart.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace CmsShoppingCart
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        //this methd belong to the mvc framework
        protected void Application_AuthenticateRequest()
        {
            //check if the user is logged in
            if(User == null) { return; }

            //get username
            string username = Context.User.Identity.Name;

            //declare array of roles
            string[] roles = null;

            using (Db db = new Db())
            {
                //populate the roles 
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);

                roles = db.UserRoles.Where(x => x.UserId == dto.Id).Select(x => x.Role.Name).ToArray();
            }

            //build IPrincipal object
            IIdentity userIdentity = new GenericIdentity(username);
            IPrincipal newUserObj = new GenericPrincipal(userIdentity, roles);

            //update Context.User
            Context.User = newUserObj;
        }
    }
}
