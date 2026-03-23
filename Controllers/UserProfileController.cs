using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Npgsql;
using System.Data;

namespace NMPMS.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly dbconfig _db;

        public UserProfileController(dbconfig db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult get_details()
        {
            var adid = HttpContext.Session.GetString("adid");
            if (string.IsNullOrWhiteSpace(adid))
                return Json(new { valid = 0 });

            try
            {

                using (var con = _db.GetConnection())
                {
                    con.Open();

                    using (var cmd = new NpgsqlCommand(@"
                        SELECT * FROM public.tbl_users WHERE adid = @adid", con))
                    {
                        cmd.Parameters.Add("@adid", NpgsqlTypes.NpgsqlDbType.Char).Value = adid;

                        using (var dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                string fullName = dr["name"].ToString();
                                string empNo = dr["empno"].ToString();
                                string section = dr["section"].ToString();
                                string position = dr["Position"].ToString();
                                string status = dr["user_status"].ToString();
                                string userlevel = dr["user_level"].ToString();
                                string user_img = dr["user_img"].ToString();
                                DateTime dateAdded = Convert.ToDateTime(dr["date_added"]);
                                string formattedDate = dateAdded.ToString("MMMM dd, yyyy");

                                string user_imgPath = "/upload/UserImg/" + user_img;

                                dr.Close();

                                return Json(new
                                {
                                    valid = 1,
                                    fullName,
                                    empNo,
                                    section,
                                    adid,
                                    position,
                                    status,
                                    userlevel,
                                    user_imgPath,
                                    formattedDate
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
        [HttpPost]
        public async Task<IActionResult> Upload_Image(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "No file uploaded" });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                return Json(new { success = false, message = "Invalid file type" });

            if (file.Length > 2 * 1024 * 1024)
                return Json(new { success = false, message = "File too large (max 2MB)" });

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/upload/UserImg");

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            //var fileName = Guid.NewGuid().ToString() + extension;
            var adid = HttpContext.Session.GetString("adid");
            var fileName = adid + extension;
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string relativePath = "/upload/UserImg/" + fileName;

            using (var con = _db.GetConnection())
            {
                con.Open();

                using (var cmd = new NpgsqlCommand(@"UPDATE public.tbl_users SET user_img = @fileName WHERE adid = @adid ", con))
                {
                    cmd.Parameters.Add("@fileName", NpgsqlTypes.NpgsqlDbType.Char).Value = fileName;
                    cmd.Parameters.Add("@adid", NpgsqlTypes.NpgsqlDbType.Char).Value = adid;
                    cmd.ExecuteNonQuery();

                }
            }

            return Json(new { success = true, path = relativePath });
        }


        [HttpGet]
        public IActionResult get_detailsForHome()
        {
            var adid = HttpContext.Session.GetString("adid");
            if (string.IsNullOrWhiteSpace(adid))
                return Json(new { valid = 0 });

            try
            {

                using (var con = _db.GetConnection())
                {
                    con.Open();

                    using (var cmd = new NpgsqlCommand(@"
                        SELECT * FROM public.tbl_users WHERE adid = @adid", con))
                    {
                        cmd.Parameters.Add("@adid", NpgsqlTypes.NpgsqlDbType.Char).Value = adid;

                        using (var dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                string fullName1 = dr["name"].ToString();
                                string user_img1 = dr["user_img"].ToString();
                                string user_imgPath1 = "/upload/UserImg/" + user_img1;

                                dr.Close();

                                return Json(new
                                {
                                    valid = 1,
                                    fullName1,
                                    user_imgPath1
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
    }

}    
