﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Daewoong.BI.Datas;
using Daewoong.BI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace Daewoong.BI.Controllers
{
    [Route("api/[controller]")]
    
    //20200109 김태규 수정 배포
    //public class UserController : ControllerBase
    public class UserController : Controller
    {
        //20200109 김태규 수정 배포
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        public UserController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration)
        {
            if (userManager != null)
            {
                _userManager = userManager;
                _signInManager = signInManager;
                _configuration = configuration;
            }
        }



        /// <summary>
        /// 모든사용자 취득
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        //[Authorize]
        public List<DWBIUser> Get()
        {
            List<DWBIUser> list = new List<DWBIUser>();

            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("get_users", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new DWBIUser()
                            {
                                ID = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"].ToString(),
                                CompanyID = Convert.ToInt32(reader["CompanyID"]),
                                CompanyCode = Convert.ToInt32(reader["CompanyCode"]),
                                UserID = reader["UserID"].ToString(),
                                CompanyName = reader["CompanyName"].ToString()
                            });
                        }
                    }
                }
                return list;
            }
        }

        public DWBIUser GetByKey(string id, HttpRequest Request)
        {
            try
            {
                List<DWBIUser> list = new List<DWBIUser>();
				//2019-12-26 김태규 수정 배포
				DWBIUser user = new DWBIUser();

                using (var db = new DWContext())
                {
                    using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                    {
                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand("get_userByID", conn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add(new MySqlParameter("@id", id));

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                                return null;

                            reader.Read();
                            //2019-12-26 김태규 수정 배포
                            //DWBIUser user = new DWBIUser();

                            user.ID = Convert.ToInt32(reader["Id"]);
                            user.Name = reader["Name"].ToString();
                            user.CompanyID = Convert.ToInt32(reader["CompanyID"]);
                            user.CompanyCode = Convert.ToInt32(reader["Code"]);
                            if (user.CompanyCode == 2000)
                                user.CompanyCode = 1100;
                            user.UserID = reader["UserID"].ToString();
                            user.RoleID = reader["RoleID"].ToString();
                            user.CompanyName = reader["CompanyName"].ToString();
                            user.IsAdmin = Convert.ToBoolean(reader["IsAdmin"]);
                            user.UserRole = (Role)Convert.ToInt32(reader["UserRole"]);

                            if (user.UserRole == Role.Manager)
                            {
                                if (Request != null && !string.IsNullOrEmpty(Request.Headers["company"]))
                                    user.CompanyCode = int.Parse(Request.Headers["company"]);
                            }

                            //return user;
                        }
                    }
                }
				//2019-12-26 김태규 수정 배포
				user.Companies = GetCompanies(user.ID);

                return user;

            }
            catch (MySqlProtocolException ex)
            {
                throw new Exception(ex.Message + ex.InnerException.Message);
            }
        }

        private List<Company> GetCompanies(int id)
        {
            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("get_CompaniesByUserID", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("@id", id));

                    List<Company> companies = new List<Company>();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            companies.Add(new Company()
                            {
                                CompanyName = reader["companyName"].ToString(),
                                Code = Convert.ToInt32(reader["code"]),
                            });
                        }
                    }
                    return companies;
                }
            }
        }

        public List<DWBIUser> GetUserByCompany(int companyID)
        {
            List<DWBIUser> list = new List<DWBIUser>();

            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("select * from user where companyID=" + companyID, conn);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new DWBIUser()
                            {
                                ID = Convert.ToInt32(reader["Id"]),
                                Name = reader["Name"].ToString(),
                                CompanyID = Convert.ToInt32(reader["CompanyID"]),
                                UserID = reader["UserID"].ToString()
                            });
                        }
                    }
                }
                return list;
            }

        }

        [HttpPost]
        [Route("")]
        public bool Post([FromBody] DWBIUser user)
        {
            return SaveUser(user);
        }

        public bool SaveUser(DWBIUser user)
        {
            try
            {
                using (var db = new DWContext())
                {
                    using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                    {
                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand("insert into user(Name, CompanyID, UserRole, isAdmin, UserID) values('"
                            + user.Name + "', '"
                            + user.CompanyID + "', '"
                            + (int)user.UserRole + "', '"
                            + 0 + "', '"
                            + user.UserID + "')", conn);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return true;
            }
        }

        [HttpPut]
        [Route("")]
        public bool Update([FromBody] DWBIUser user)
        {
            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("update user set name='"
                        + user.Name + "', companyID= '" + user.CompanyID
                        + "', UserID= '" + user.UserID
                        + "', UserRole= '" + (int)user.UserRole
                        + "' where id=" + user.ID, conn);

                    cmd.ExecuteNonQuery();
                }
            }
            return true;
        }

        [HttpDelete]
        [Route("")]
        //20200109 김태규 수정 배포
        //public bool Delete()
        public async Task<bool> Delete()
        {
            var id = HttpContext.Request.Query["id"];
            //20200109 김태규 수정 배포
			var email = HttpContext.Request.Query["userid"];
            using (var db = new DWContext())
            {
                using (MySqlConnection conn = new MySqlConnection(db.ConnectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("delete from user where id=" + id[0], conn);
                    cmd.ExecuteNonQuery();
                }
            }
            //user.Email = email[0];
            IdentityUser user = await _userManager.FindByEmailAsync(email[0]);
            
            user.UserName = email[0];
            var result = await _userManager.DeleteAsync(user);

            return true;
        }
    }
}