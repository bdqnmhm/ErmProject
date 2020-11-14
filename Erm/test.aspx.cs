using ERM.Core.DataBase;
using ERM.Manager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Erm
{
    public partial class test : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Getdata();
        }

        public void Getdata()
        {
            string sql = @" select * from erm_users";
            Database db = DataBaseManager.GetDataBaseByDomainConfig(AppDomain.CurrentDomain.BaseDirectory, "Public");
            DBCommandWrapper cmd = db.GetSqlStringCommandWrapper(sql);
            DataSet ds = new DataSet();
            var list = db.LoadDataSet(cmd, ds, "table");
        }
    }
}