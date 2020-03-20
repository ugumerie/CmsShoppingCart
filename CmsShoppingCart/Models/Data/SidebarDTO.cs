using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CmsShoppingCart.Models.Data
{
    [Table("tblSidebar")]
    public class SidebarDTO
    {
        [Key]
        public int Id { get; set; }
        public string Body { get; set; }
    }
}