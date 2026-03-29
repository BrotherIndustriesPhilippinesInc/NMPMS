using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Npgsql;

public class LoginController : Controller
{
    private readonly dbconfig _db;

    public LoginController(dbconfig db)
    {
        _db = db;
    }

    public IActionResult Index()
    {

        //string ip = "10.248.15.190";
        string? ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        if (ip == "127.0.0.1" || ip == "::1")
        {
            ip = "10.248.15.190";
        }


        using (var conn = _db.GetSqlServerConnection()) 
        {
            conn.Open();

            string sql = @"SELECT TOP 1 USERNAME 
                           FROM Tbl_LOGIN_Request 
                           WHERE HOSTNAME = @ip 
                           AND STATUS = 'ACTIVE' 
                           AND [SYSTEM ID] = 84
                           ORDER BY ID DESC";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ip", ip);

                var result = cmd.ExecuteScalar();

                if (result != null)
                {
                    HttpContext.Session.SetString("user_id", result.ToString());
                    return RedirectToAction("LoginControl");
                }
            }
        }

        return View("LoginError"); 
    }

    public IActionResult LoginControl()
    {
        var userId = HttpContext.Session.GetString("user_id");

        if (string.IsNullOrEmpty(userId))
            return Content("Session not set.");

        using (var conn = _db.GetConnection())
        {
            conn.Open();

            string sql = @"SELECT * FROM tbl_users 
                       WHERE empno = @empNo AND user_status = 1";

            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@empNo", userId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        HttpContext.Session.SetString("username", reader["name"].ToString());
                        HttpContext.Session.SetString("ID", reader["id"].ToString());
                        HttpContext.Session.SetString("adid", reader["adid"].ToString());
                        HttpContext.Session.SetString("section", reader["section"].ToString());

                        return RedirectToAction("Index", "Home");
                    }
                }
            }
        }

        return Content("No user found in Tbl_Users.");
    }
}