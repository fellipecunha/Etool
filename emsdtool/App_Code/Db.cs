using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace emsdtool.App_Code
{
    public class Db
    {
        public static string ConnString =>
        ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
    }
}