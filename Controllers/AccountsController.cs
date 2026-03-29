using Microsoft.AspNetCore.Mvc;
using Microsoft.Office.Interop.Excel;
using Npgsql;
using System.Data;
using Microsoft.Data.SqlClient;

namespace NMPMS.Controllers
{
    public class AccountsController : Controller
    {
        private readonly dbconfig _db;

        public AccountsController(dbconfig db)
        {
            _db = db;

        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult get_details(string empNo)
        {
            if (string.IsNullOrWhiteSpace(empNo))
                return Json(new { valid = 0 });

            try
            {

                using (SqlConnection con = _db.GetSqlServerConnection())
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand(@"
                        SELECT Full_Name, EmpNo, Section, Company, Position, Status,ADID, Email
                        FROM tbl_EMSVIEW
                        WHERE EmpNo = @empNo", con))
                    {
                        cmd.Parameters.Add("@empNo", SqlDbType.VarChar).Value = empNo;

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                              
                                string fullName = dr["Full_Name"].ToString();
                                //string empNo = dr["EmpNo"].ToString();
                                string section = dr["Section"].ToString();
                                string company = dr["Company"].ToString();
                                string position = dr["Position"].ToString();
                                string email = dr["Email"].ToString();
                                string adid = dr["ADID"].ToString();
                                string status = dr["Status"].ToString();

                                dr.Close();

                                return Json(new
                                {
                                    valid = 1,
                                    fullName,
                                    empNo,
                                    section,
                                    company,
                                    adid,
                                    email,
                                    position,
                                    status
                                });
                            }
                        }
                    }
                }

                return Json(new { valid = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> save_user(IFormCollection form)
        {
            try
            {
                string fullname = form["fullname"];
                string biph_id = form["biph_id"];
                string adid = form["adid"];
                string email = form["email"];
                string position = form["position"];
                string section = form["section"];
                string section_code = form["section_code"];
                string authority = form["authority"];


                using (var con = _db.GetConnection())
                //using (var sqlCon = _db.GetSqlServerConnection())
                {
                    await con.OpenAsync();

                    string sql = @"INSERT INTO public.tbl_users(empno, name, adid, position, section, section_code, user_level, user_status)
                    VALUES(@biph_id, @fullname, @adid, @position, @section, @section_code, @authority, 1)";

                    int? authorityInt = int.TryParse(form["authority"], out int tempValue) ? tempValue : (int?)null;

                    using (var cmd = new NpgsqlCommand(sql, con))
                    {
                        // Use (object)value ?? DBNull.Value to safely handle nulls
                        cmd.Parameters.AddWithValue("biph_id", (object)biph_id ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("fullname", (object)fullname ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("adid", (object)adid ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("position", (object)position ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("section", (object)section ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("section_code", (object)section_code ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("authority", (object)authorityInt ?? DBNull.Value);

                        await cmd.ExecuteNonQueryAsync();
                    }

                    //string query = @"INSERT INTO [dbo].[Tbl_System_Approver_list] ([SYSTEM ID],[SYSTEM NAME],[APPROVER NUMBER],[FULL NAME],[EMAIL ADDRESS],[SECTION],[POSITION],[ADID],[EMPLOYEE NUMBER]) VALUES(@system_id, @system_name, @approver_no, @fullname, @email, @section, @position, @adid, @empno)";

                    //using (var cmd2 = new SqlCommand(query, sqlCon))
                    //{
                    //    await sqlCon.OpenAsync();

                    //    cmd2.Parameters.AddWithValue("@system_id", 84);
                    //    cmd2.Parameters.AddWithValue("@system_name", "New Model Problem Management System");
                    //    cmd2.Parameters.AddWithValue("@approver_no", (object)biph_id ?? DBNull.Value);
                    //    cmd2.Parameters.AddWithValue("@fullname", (object)fullname ?? DBNull.Value);
                    //    cmd2.Parameters.AddWithValue("@email", (object)email ?? DBNull.Value);
                    //    cmd2.Parameters.AddWithValue("@section", (object)section ?? DBNull.Value);
                    //    cmd2.Parameters.AddWithValue("@position", (object)position ?? DBNull.Value);
                    //    cmd2.Parameters.AddWithValue("@adid", (object)adid ?? DBNull.Value);
                    //    cmd2.Parameters.AddWithValue("@empno", (object)biph_id ?? DBNull.Value);

                    //    await cmd2.ExecuteNonQueryAsync();
                    //}
                }

                return Json(new { status = "success"});
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }
        }

        public IActionResult fetch_accounts()
        {
            var list = new List<object>();

            using (var con = _db.GetConnection())
            {
                con.Open();

                string query = @"SELECT * FROM public.tbl_users ORDER BY id ASC ";

                using (var cmd = new NpgsqlCommand(query, con))
                {

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string user_img = reader["user_img"].ToString();
                            string user_imgPath = "/upload/UserImg/" + user_img;

                            list.Add(new
                            {
                                empno = reader["empno"]?.ToString(),
                                adid = reader["adid"]?.ToString(),
                                name = reader["name"]?.ToString(),
                                section = reader["section"]?.ToString(),
                                type = reader["user_level"]?.ToString(),
                                status = reader["user_status"]?.ToString(),
                                user_imgPath = user_imgPath?.ToString(),

                            });
                        }
                    }
                }

               
            }
            return Ok(list);
        }
    }
}
