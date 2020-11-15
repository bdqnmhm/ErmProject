using ERM.Core.DataBase;
using ERM.Manager;
using ErmMvc.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ErmMvc.Controllers
{
    public class ErmController : Controller
    {
        // GET: Erm
        public ActionResult Login()
        {
            //  LoginUser("admin", "admin");
            return View();
        }

        public ActionResult SubmitLogin(string username, string password)
        {
            var user = LoginUser(username, password);
            if (user.Count > 0)
            {
                TempData["users"] = user;
                //写入cookies方式1
                Response.Cookies["tempToken"].Value = user.FirstOrDefault().ID.ToString();
                Response.Cookies["tempToken"].Expires = DateTime.Now.AddHours(24);
                return RedirectToAction("Index", "Erm");
            }
            else
            {
                TempData["msg"] = "用户名密码不正确或用户不存在";
                return RedirectToAction("Login", "Erm");
            }
        }

        public ActionResult Index()
        {
            if (TempData["users"] == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                var user = TempData["users"] as List<UserModel>;
                ViewData.Model = user.FirstOrDefault();
            }
            return View();
        }

        public ActionResult ErmInfo(string startdate, string enddate, string username, string pageNumber, string fw, string eid, string phone)
        {
            if (Request.Cookies["tempToken"].Value == null)
            {
                return RedirectToAction("Login", "Erm");
            }

            GridResult result = new GridResult();
            DataSet ds = GetErmInfoList(startdate, enddate, username, 10, int.Parse(string.IsNullOrEmpty(pageNumber) ? "1" : pageNumber), fw, eid, phone);
            if (ds != null)
            {
                result.data = ConvertToList<ErmInfoModel>(ds.Tables[0]);
                result.page = int.Parse(string.IsNullOrEmpty(pageNumber) ? "1" : pageNumber);
                result.total = int.Parse(ds.Tables[1].Rows[0][0].ToString());
            }
            ViewData.Model = result;
            return View();
        }

        public ActionResult ErmAdd(string Id)
        {
            if (!string.IsNullOrEmpty(Id))
            {
                List<ErmInfoModel> lstdata = GetErmInfoById(Id);
                if (lstdata.Count > 0)
                {
                    ViewData.Model = lstdata.FirstOrDefault();
                }
            }
            return View();
        }

        public ActionResult ErmSubmit(string ID, string COMPTITLE, string CORPORATION, string MOBILEPHONE)
        {
            string EID = string.Empty;
            if (Request.Cookies["tempToken"].Value == null)
            {
                return RedirectToAction("Login", "Erm");
            }
            else
            {
                EID = Request.Cookies["tempToken"].Value;
            }
            var lstdata = GetExistsErmInfo(COMPTITLE);
            if (lstdata.Count > 0)
            {
                var m = lstdata.FirstOrDefault();
                return Json(new { success = false, msg = string.Format("公司抬头【{0}】已经在【{1}】被【{2}】录入过，到期日期【{3}】", m.COMPTITLE, m.EDT.Value.ToString("yyyy-MM-dd HH:mm:ss"), m.USERNAME, m.ENDDATE.Value.ToString("yyyy-MM-dd HH:mm:ss")) });
            }
            string sql = string.Empty;
            if (string.IsNullOrEmpty(ID))
            {
                sql = string.Format(@"INSERT INTO erm_info(id, comptitle, corporation, eid, startdate,enddate, state,state_enum, edt, mobilephone)
                                values(gen_id('1900-01-01'::date,'2020-10-16'::date)::bigint,'{0}','{1}','{2}',now(),now()::timestamp + '1 month',1,'启用',now(),'{3}')",
                                COMPTITLE, CORPORATION, EID, MOBILEPHONE);
                RunSql(sql);
            }
            return Json(new { success = true });
            //return RedirectToAction("ErmInfo", "Erm");
        }

        public ActionResult ErmUsers()
        {
            return View();
        }

        public static List<T> GetDataBySql<T>(string sql) where T : new()
        {
            Database db = DataBaseManager.GetDataBaseByDomainConfig(AppDomain.CurrentDomain.BaseDirectory, "Public");
            DBCommandWrapper cmd = db.GetSqlStringCommandWrapper(sql);
            DataSet ds = new DataSet();
            var count = db.LoadDataSet(cmd, ds, "table");
            if (count > 0)
            {
                var lstdata = ConvertToList<T>(ds.Tables[0]);
                return lstdata;
            }
            else
            {
                return new List<T>();
            }
        }

        public bool RunSql(string sql, string domain = "Public")
        {
            Database db = DataBaseManager.GetDataBaseByDomain(domain);
            db.OpenConnection();
            try
            {
                if (sql.Length > 0)
                {
                    db.ExecuteNonQuery(sql.ToString());
                }
            }
            finally
            {
                db.CloseConnection();
            }
            return true;
        }

        public static List<T> ConvertToList<T>(DataTable table) where T : new()
        {

            List<T> list = null;
            if (table != null)
            {
                DataColumnCollection columns = table.Columns;
                int columnCount = columns.Count;
                T type = new T();
                Type columnType = type.GetType();
                PropertyInfo[] properties = columnType.GetProperties();
                //if (properties.Length == columnCount)
                //{

                list = new List<T>();
                foreach (DataRow currentRow in table.Rows)
                {
                    for (int i = 0; i < columnCount; i++)
                    {
                        for (int j = 0; j < properties.Length; j++)
                        {
                            if (columns[i].ColumnName.ToUpper() == properties[j].Name)
                            {
                                if (currentRow[i] != DBNull.Value)
                                {
                                    properties[j].SetValue(type, currentRow[i], null);
                                }
                            }
                        }
                    }

                    list.Add(type); type = new T();
                }
                //}
            }

            else
            {
                throw new ArgumentNullException("参数不能为空");
            }

            return list;
        }

        #region 处理仓储
        private string pageSql = "SELECT A.* FROM ({0}) A {1} LIMIT {2} OFFSET {3}; ";

        private string countSql = "SELECT COUNT(*) FROM ({0}) A ;";
        public List<UserModel> LoginUser(string loginame, string pwd)
        {
            string sql = string.Format(@" select * from erm_users where loginname ='{0}' and userpwd='{1}' ", loginame, pwd);
            var lstusers = GetDataBySql<UserModel>(sql);

            // 用户存在
            if (lstusers.Count > 0)
            {
                var tokens = GetDataBySql<UserToken>(string.Format(" select * from erm_token where uid = {0}", lstusers.FirstOrDefault().ID));
                if (tokens.Count > 0)
                {
                    if (tokens.FirstOrDefault().ENDEDT <= DateTime.Now)
                    {
                        RunSql(string.Format("  update erm_token set enddate= enddate::TIMESTAMP+'1 day' where uid = {0}; ", lstusers.FirstOrDefault().ID));
                    }
                }
                else
                {
                    RunSql(string.Format("  insert into erm_token(id,uid,edt,enddate) values(gen_id('1900-01-01'::date,'2020-10-16'::date)::bigint,{0},now(),now()::timestamp + '1 day');  ", lstusers.FirstOrDefault().ID));
                }
            }
            return lstusers;
        }

        public List<ErmInfoModel> GetErmInfoById(string Id)
        {
            string sql = string.Format(@" select erm_info.*,username from erm_info left join erm_users on erm_info.eid=erm_users.id where erm_info.Id ='{0}' ", Id);
            var lstusers = GetDataBySql<ErmInfoModel>(sql);
            return lstusers;
        }


        public List<ErmInfoModel> GetExistsErmInfo(string Key)
        {
            string sql = string.Format(@" select erm_info.*,username from erm_info left join erm_users on erm_info.eid=erm_users.id where comptitle ='{0}' ", Key);
            var lstusers = GetDataBySql<ErmInfoModel>(sql);
            return lstusers;
        }

        public DataSet GetUserList(string startedate, string enddate, string UserName, int PageSize, int PageIndex)
        {
            Database db = DataBaseManager.GetDataBaseByDomainConfig(AppDomain.CurrentDomain.BaseDirectory, "Public");
            string str = string.Empty;
            string where = string.Empty;

            if (!string.IsNullOrEmpty(startedate))
            {
                where += string.Format(" and edt>='{0}' ", startedate);
            }

            if (!string.IsNullOrEmpty(enddate))
            {
                where += string.Format(" and edt<='{0}' ", startedate);
            }

            if (!string.IsNullOrEmpty(UserName))
            {
                where += string.Format(" and username like '%{0}%' ", UserName);
            }

            str = string.Format(@" select * from erm_users where 1=1 {0}", where);


            StringBuilder runsql = new StringBuilder();
            string pageListSql = string.Format(pageSql, str, "", PageSize, PageSize * (PageIndex - 1));
            runsql.Append(pageListSql);

            string entityCountSql = string.Format(countSql, str);
            runsql.Append(entityCountSql);

            DataSet ds = new DataSet();
            DBCommandWrapper cmd = db.GetSqlStringCommandWrapper(runsql.ToString());
            db.LoadDataSet(cmd, ds, "table");
            int TotalRecordCount = int.Parse(ds.Tables[1].Rows[0][0].ToString());
            db.CloseConnection();
            return ds;
        }

        public DataSet GetErmInfoList(string startedate, string enddate, string UserName, int PageSize, int PageIndex, string fw, string eid, string phone)
        {
            Database db = DataBaseManager.GetDataBaseByDomainConfig(AppDomain.CurrentDomain.BaseDirectory, "Public");
            string str = string.Empty;
            string where = string.Empty;

            if (!string.IsNullOrEmpty(startedate))
            {
                where += string.Format(" and erm_info.edt>='{0}' ", startedate);
            }

            if (!string.IsNullOrEmpty(enddate))
            {
                where += string.Format(" and erm_info.edt<='{0}' ", enddate);
            }

            if (!string.IsNullOrEmpty(UserName))
            {
                where += string.Format(" and erm_info.comptitle like '%{0}%' ", UserName);
            }

            if (!string.IsNullOrEmpty(fw))
            {
                where += string.Format(" and erm_info.corporation like '%{0}%' ", fw);
            }

            if (!string.IsNullOrEmpty(eid))
            {
                where += string.Format(" and erm_users.username like '%{0}%' ", eid);
            }

            if (!string.IsNullOrEmpty(phone))
            {
                where += string.Format(" and erm_info.mobilephone like '%{0}%' ", phone);
            }

            str = string.Format(@" select erm_info.*,username from erm_info left join erm_users on erm_info.eid=erm_users.id where 1=1 {0}", where);


            StringBuilder runsql = new StringBuilder();
            string pageListSql = string.Format(pageSql, str, " order by edt desc ", PageSize, PageSize * (PageIndex - 1));
            runsql.Append(pageListSql);

            string entityCountSql = string.Format(countSql, str);
            runsql.Append(entityCountSql);

            DataSet ds = new DataSet();
            DBCommandWrapper cmd = db.GetSqlStringCommandWrapper(runsql.ToString());
            db.LoadDataSet(cmd, ds, "table");
            int TotalRecordCount = int.Parse(ds.Tables[1].Rows[0][0].ToString());
            db.CloseConnection();
            return ds;
        }
        #endregion
    }
}