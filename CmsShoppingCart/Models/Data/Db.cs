using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;

namespace CmsShoppingCart.Models.Data
{
    public class Db : DbContext
    {
        public DbSet<PageDTO> Pages{ get; set; }
        public DbSet<SidebarDTO> Sidebar { get; set; }
    }
}