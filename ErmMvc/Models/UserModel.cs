using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ErmMvc.Models
{
    public class UserModel
    {
        public long? ID { get; set; }
        /// <summary>
        /// 登录名
        /// </summary>
        public string LOGINNAME { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string USERNAME { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string USERPWD { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public short? STATE { get; set; } = 1;

        /// <summary>
        /// 状态
        /// </summary>
        public string STATE_ENUM { get; set; } = "启用";

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? EDT { get; set; }
    }

    public class UserToken 
    {
        public long? ID { get; set; }
        public long? UID { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? EDT { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? ENDEDT { get; set; }
    }

    public class ErmInfoModel 
    {

        /// <summary>
        /// 用户名
        /// </summary>
        public string USERNAME { get; set; }
        public long? ID { get; set; }
        /// <summary>
        /// 公司抬头
        /// </summary>
        public string COMPTITLE { get; set; }

        /// <summary>
        /// 法人
        /// </summary>
        public string CORPORATION { get; set; }

        /// <summary>
        /// 手机
        /// </summary>
        public string MOBILEPHONE { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public long? EID { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public short? STATE { get; set; } = 1;

        /// <summary>
        /// 状态
        /// </summary>
        public string STATE_ENUM { get; set; } = "启用";

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? EDT { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? STARTDATE { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? ENDDATE { get; set; }
    }

    public class GridResult
    { 
        public int total { get; set; }

        public int page { get; set; }

        public object data { get; set; }
    }
}