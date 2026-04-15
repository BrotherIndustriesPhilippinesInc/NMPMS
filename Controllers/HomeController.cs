using Microsoft.AspNetCore.Mvc;
using NMPMS.Models;
using NMPMS.Services;
using NMPMS.ViewModels;
using Npgsql;
using System.Diagnostics;
using System.Net.Mail;
using System.Net;
using Org.BouncyCastle.Asn1.Crmf;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace NMPMS.Controllers;

public class HomeController : Controller
{

    private readonly ILogger<HomeController> _logger;
    private readonly dbconfig _db;
    private readonly IWebHostEnvironment _env;
    //private readonly IPmsService _service;
    //, IPmsService service
    //_service = service;


    public HomeController(ILogger<HomeController> logger, dbconfig db, IWebHostEnvironment env)
    {
        _logger = logger;
        _db = db;
        _env = env;
      
    }

    public IActionResult Index()
    {

        return View();
    }

    public IActionResult Privacy()
    {
        return View(); 
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    //[HttpPost]
    //public async Task<IActionResult> CreateIssue(CreateIssueViewModel model)
    //{
    //    if (!ModelState.IsValid)
    //        return BadRequest(ModelState);

    //    await _service.CreateIssueAsync(model);

    //    return Json(new { status = "success", control_no = model.ControlNo });
    //}

    //[HttpGet]
    //public async Task<IActionResult> FetchPml(string stage, string model)
    //{
    //    var data = await _service.FetchAsync(stage, model);
    //    return Ok(data);
    //}

    [HttpGet]
    public IActionResult fetch_pml(string stage, string model)
    {
        var list = new List<object>();

        using (var con = _db.GetConnection())
        {
            con.Open();
            int.TryParse(HttpContext.Session.GetString("userlevel"), out int userlevel);

            string query = @"SELECT control_no,pms_create,person_incharge,
            problem_name,phenomenon_details,stage,model,
            serial_number,area_detection,process,
            issued_by, issued_date,part_name,supplier,attachment_name,problem_photo
            FROM pms_records WHERE 1=1";

         
            if (!string.IsNullOrEmpty(stage) && !string.IsNullOrEmpty(model))
            {
                query += " AND stage = @stage AND model = @model";
            }
            else if (userlevel != 1)
            {
                query += " AND stage = @stage AND model = @model";
            }

            using (var cmd = new NpgsqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@stage", stage ?? "");
                cmd.Parameters.AddWithValue("@model", model ?? "");

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new
                        {
                            control_no = reader["control_no"]?.ToString(),
                            pms_create = reader["pms_create"]?.ToString(),
                            person_incharge = reader["person_incharge"]?.ToString(),
                            problem_name = reader["problem_name"]?.ToString(),
                            phenomenon_details = reader["phenomenon_details"]?.ToString(),
                            stage = reader["stage"]?.ToString(),
                            model = reader["model"]?.ToString(),
                            serial_number = reader["serial_number"]?.ToString(),
                            area_detection = reader["area_detection"]?.ToString(),
                            process = reader["process"]?.ToString(),
                            issued_by = reader["issued_by"]?.ToString(),
                            issued_date = reader["issued_date"]?.ToString(),
                            attachment_name = reader["attachment_name"]?.ToString(),
                            part_name = reader["part_name"]?.ToString(),
                            problem_photo = reader["problem_photo"]?.ToString(),
                            supplier = reader["supplier"]?.ToString()
                        });
                    }
                }
            }
        }

        return Ok(list);
    }
    [HttpPost]
    public IActionResult createnew(string mName, string sName)
    {
        if (string.IsNullOrWhiteSpace(mName) || string.IsNullOrWhiteSpace(sName))
            return Json(new { success = false, massage = "Invalid Data" });
        try
        {
            using (var con = _db.GetConnection())
            {
                con.Open();

                using (NpgsqlCommand cmd = new NpgsqlCommand(@"INSERT INTO tbl_model(model_name, date_added) 
                VALUES(@modelName, CURRENT_TIMESTAMP)
                ", con))
                {
                    cmd.Parameters.Add("@modelName", NpgsqlTypes.NpgsqlDbType.Char).Value = mName;
                    cmd.ExecuteNonQuery();

                }

                using (NpgsqlCommand cmd1 = new NpgsqlCommand(@"INSERT INTO tbl_stage(stage_name, date_added) 
                VALUES(@stageName, CURRENT_TIMESTAMP)
                ", con))
                {
                    cmd1.Parameters.Add("@stageName", NpgsqlTypes.NpgsqlDbType.Char).Value = sName;
                    cmd1.ExecuteNonQuery();

                }

            }

            return Json(new { success = true });

        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> get_latestcontrolNo(string stage)
    {
        string controlNo;

        using (var con = _db.GetConnection())
        {
            await con.OpenAsync();

            using (var check = new NpgsqlCommand("SELECT control_no FROM pms_records ORDER BY ID DESC LIMIT 1", con))
            {
                var result = await check.ExecuteScalarAsync();

                if (result != null)
                {
                    string lastControlNo = result.ToString();

                    // Example format: STAGE-0001
                    var parts = lastControlNo.Split('-');

                    int lastNumber = 0;

                    if (parts.Length > 1 && int.TryParse(parts[1], out lastNumber))
                    {
                        lastNumber++;
                    }
                    else
                    {
                        lastNumber = 1;
                    }

                    controlNo = $"{stage}-{lastNumber.ToString("D4")}";
                }
                else
                {
                    controlNo = $"{stage}-0001";
                }
            }
        }

        return Json(new { success = true, control_no = controlNo });
    }

    [HttpPost]
    public async Task<IActionResult> CreateIssue(IFormCollection form)
    {
        try
        {
            string pmsCreate = form["pms_create"];
            string problemName = form["problem_name"];
            string phenomenonDetails = form["phenomenon_details"];
            string stage = form["stage"];
            string model = form["model"];
            string serialNumber = form["serial_number"];
            string areaDetection = form["area_detection"];
            string process = form["process"];
            string issuedBy = form["issued_by"];
            string partCode = form["part_code"];
            string partName = form["part_name"];
            string supplier = form["supplier"];
            string controlNo = form["control_no"];
            //string controlNo ;
            string stepno = form["stepno"];
            //string serialNumber;

            DateTime? issuedDate = null;
            if (!string.IsNullOrWhiteSpace(form["issued_date"]))
            {
                issuedDate = DateTime.Parse(form["issued_date"]);
            }

            //string serialNumber = 

            //string personInCharge = "Jeffrey Reyes";
            var personInCharge = HttpContext.Session.GetString("username"); 

            var uploadedFiles = form.Files.Where(f => f.Name == "problem_photos[]").ToList();
            List<string> filePaths = new List<string>();

            if (uploadedFiles.Count > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "upload/PhotoUpload");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                int index = 1;

                foreach (var file in uploadedFiles)
                {
                    string extension = Path.GetExtension(file.FileName).ToLower();

                    string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".mp4", ".mov", ".avi" };

                    if (!allowedExtensions.Contains(extension))
                        continue;

                    string uniqueFileName = $"{controlNo}_{index}{extension}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    filePaths.Add(Path.Combine("upload/PhotoUpload/", uniqueFileName));
                    index++;
                }
            }
            string attachmentPath = null;
            string attachmentName = null;
            var attachmentFile = form.Files["attachment"];

            if (attachmentFile != null && attachmentFile.Length > 0)
            {
                var attachmentFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "upload", "AttachmentFile");

                if (!Directory.Exists(attachmentFolder))
                    Directory.CreateDirectory(attachmentFolder);

                attachmentName = attachmentFile.FileName;

                string uniqueFileName = controlNo + Path.GetExtension(attachmentFile.FileName);
                var filePath = Path.Combine(attachmentFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await attachmentFile.CopyToAsync(stream);
                }

                attachmentPath = Path.Combine("upload/AttachmentFile/", uniqueFileName);
            }

            using (var con = _db.GetConnection())
            {

                await con.OpenAsync();
               
                
                string sql = @"INSERT INTO pms_records (
                pms_create,
                person_incharge,
                problem_name,
                phenomenon_details,
                stage,
                model,
                serial_number,
                area_detection,
                process,
                issued_by,
                issued_date,
                part_code,
                part_name,
                supplier,
                problem_photo,
                attachment,
                attachment_name,
                control_no
            ) VALUES (
                @pms_create, @person_incharge, @problem_name, @phenomenon_details,
                @stage, @model, @serial_number, @area_detection, @process,
                @issued_by, @issued_date, @part_code, @part_name, @supplier,
                @problem_photo, @attachment, @attachment_name, @control_no
            )";

                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("pms_create", pmsCreate ?? "");
                    cmd.Parameters.AddWithValue("person_incharge", personInCharge ?? "");
                    cmd.Parameters.AddWithValue("problem_name", problemName ?? "");
                    cmd.Parameters.AddWithValue("phenomenon_details", phenomenonDetails ?? "");
                    cmd.Parameters.AddWithValue("stage", stage ?? "");
                    cmd.Parameters.AddWithValue("model", model ?? "");
                    cmd.Parameters.AddWithValue("serial_number", serialNumber ?? "");
                    cmd.Parameters.AddWithValue("area_detection", areaDetection ?? "");
                    cmd.Parameters.AddWithValue("process", process ?? "");
                    cmd.Parameters.AddWithValue("issued_by", issuedBy ?? "");
                    cmd.Parameters.AddWithValue("issued_date", issuedDate.HasValue ? issuedDate.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("part_code", partCode ?? "");
                    cmd.Parameters.AddWithValue("part_name", partName ?? "");
                    cmd.Parameters.AddWithValue("supplier", supplier ?? "");

                    cmd.Parameters.AddWithValue("problem_photo", DBNull.Value);

                    cmd.Parameters.AddWithValue("attachment", (object)attachmentPath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("attachment_name", attachmentName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("control_no", controlNo ?? "");

                    await cmd.ExecuteNonQueryAsync();
                }

                foreach (var path in filePaths)
                {
                    string fileSql = @"INSERT INTO pms_problem_files (control_no, file_path,steps)
                                   VALUES (@control_no, @file_path,@steps)";

                    using (var fileCmd = new NpgsqlCommand(fileSql, con))
                    {
                        fileCmd.Parameters.AddWithValue("control_no", controlNo);
                        fileCmd.Parameters.AddWithValue("file_path", path);
                        fileCmd.Parameters.AddWithValue("steps",int.TryParse(stepno, out int stepsValue) ? stepsValue : (object)DBNull.Value);
                        await fileCmd.ExecuteNonQueryAsync();
                    }
                }
            }


            string link = $"http://apbiphbpsts01:2026/";
            string formattedDate = issuedDate?.ToString("dd-MMM") ?? "";

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress("nmpms@brother-biph.com.ph", "[BIPH_NMPMS] New Problem Information");
                mail.To.Add("charisse.devera@brother-biph.com.ph");
                mail.To.Add("bheanicole.corcolon@brother-biph.com.ph");
                mail.CC.Add("jeffrey.reyes@brother-biph.com.ph");
                mail.CC.Add("arravellah.magsino@brother-biph.com.ph");
                mail.Subject = "[BIPH_NMPMS] " + model + " " + stage + " New Problem Information";
                mail.IsBodyHtml = true;

                string body = $@"
                        <div style='font-family:Segoe UI, Arial, sans-serif; background:#f4f6f9; padding:20px;'>

                            <div style='max-width:700px; margin:auto; background:#ffffff; border-radius:10px; 
                                        box-shadow:0 4px 15px rgba(0,0,0,0.1); overflow:hidden;'>

                                <div style='background:#1f2937; color:#fff; padding:15px 20px; font-size:18px; font-weight:bold;'>
                                    BIPH New Model Problem Management System
                                </div>

                                <div style='padding:20px; color:#333;'>

                                    <h2 style='margin-top:0; color:#111827;'>New Problem Notification</h2>

                                    <table style='width:100%; border-collapse:collapse; font-size:14px;'>

                                        <tr>
                                            <td style='padding:8px; font-weight:bold;'>Problem Control #:</td>
                                            <td style='padding:8px;'>#{controlNo}</td>
                                        </tr>

                                        <tr style='background:#f9fafb;'>
                                            <td style='padding:8px; font-weight:bold;'>Problem Name:</td>
                                            <td style='padding:8px;'>{problemName}</td>
                                        </tr>

                                        <tr>
                                            <td style='padding:8px; font-weight:bold;'>Process:</td>
                                            <td style='padding:8px;'>{process}</td>
                                        </tr>

                                        <tr style='background:#f9fafb;'>
                                            <td style='padding:8px; font-weight:bold;'>Encountered Date:</td>
                                            <td style='padding:8px;'>{formattedDate}</td>
                                        </tr>

                                        <tr>
                                            <td style='padding:8px; font-weight:bold;'>Serial Number:</td>
                                            <td style='padding:8px;'>{serialNumber}</td>
                                        </tr>

                                        <tr style='background:#f9fafb;'>
                                            <td style='padding:8px; font-weight:bold;'>Area of Detection:</td>
                                            <td style='padding:8px;'>{areaDetection}</td>
                                        </tr>

                                    </table>

                                    <div style='margin-top:20px;'>
                                        <strong>Phenomenon Details:</strong>
                                        <div style='margin-top:8px; padding:12px; background:#f3f4f6; border-radius:6px;'>
                                            {phenomenonDetails}
                                        </div>
                                    </div>

                                    <div style='text-align:center; margin:25px 0;'>
                                        <a href='{link}' style='background:#2563eb; color:#fff; padding:12px 20px; 
                                           text-decoration:none; border-radius:6px; font-weight:bold; display:inline-block;'>
                                            View Full Details
                                        </a>
                                    </div>

                                    <!-- Signature -->
                                    <div style='margin-top:30px; border-top:1px solid #e5e7eb; padding-top:15px;'>
                                        <table>
                                            <tr>
                                               
                                                <td style='font-size:13px; color:#374151;'>
                                                    <b>BIPH DE Concurrent</b><br>
                                                    New Model Problem Management System<br>
                                                    Brother Industries (Philippines) Inc.<br>
                                                    
                                                </td>
                                            </tr>
                                        </table>
                                    </div>

                                      <img src='cid:signatureImage' width='200' />

                                    <div style='font-size:12px; color:#6b7280; margin-top:15px;'>
                                        This is an automated message. Please do not reply.
                                    </div>

                                </div>
                            </div>
                        </div>";

                // Create AlternateView
                AlternateView altView = AlternateView.CreateAlternateViewFromString(body, null, "text/html");

                // Attach Image
                string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/phishing.png"); // adjust path
                LinkedResource signature = new LinkedResource(imagePath, "image/png");
                signature.ContentId = "signatureImage";

                altView.LinkedResources.Add(signature);
                mail.AlternateViews.Add(altView);

                using (SmtpClient smtp = new SmtpClient("smtp.brother.co.jp", 25))
                {
                    smtp.UseDefaultCredentials = true;
                    smtp.EnableSsl = false;

                    await smtp.SendMailAsync(mail);
                }
            
        }

            return Json(new { status = "success", control_no = controlNo });
        }
        catch (Exception ex)
        {
            return Json(new { status = "error", message = ex.Message });
        }
    }


    public IActionResult fetch_graph(string stage, string model)
    {
        var list = new List<object>();

        using (var con = _db.GetConnection())
        {
            con.Open();
            int.TryParse(HttpContext.Session.GetString("userlevel"), out int userlevel);

            string query = @"SELECT b.stage, b.model, a.problem_category FROM public.tbl_analysis a join public.pms_records b on a.control_no = b.control_no WHERE 1=1";

            if (!string.IsNullOrEmpty(stage) && !string.IsNullOrEmpty(model))
            {
                query += " AND stage = @stage AND model = @model";
            }
            else if (userlevel != 1)
            {
                query += " AND stage = @stage AND model = @model";
            }


           

            using (var cmd = new NpgsqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@stage", stage ?? "");
                cmd.Parameters.AddWithValue("@model", model ?? "");

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new
                        {
                            problem_category = reader["problem_category"]?.ToString(),
                           
                        });
                    }
                }
            }
        }

        return Ok(list);
    }


    [HttpGet]
    public IActionResult get_userlist()
    {
        var list = new List<object>();
        using (var con = _db.GetConnection())
        {
            con.Open();
            string sql = @"SELECT * FROM tbl_users where user_status = 1";

            using (var cmd = new NpgsqlCommand(sql, con))
            {
                using (var row = cmd.ExecuteReader())
                {
                    while (row.Read())
                    {
                        list.Add(new
                        {
                            name = row["name"].ToString(),
                            empno = row["empno"].ToString()
                        });
                        //row["name"].ToString();
                    }
                }
            }
            return Ok(list);

        }

    }


    [HttpGet]
    public IActionResult additionalOptions()
    {
        var models = new List<string>();
        var stages = new List<string>();

        using (var con = _db.GetConnection())
        {
            con.Open();

            // Models
            string sql = @"SELECT model_name FROM tbl_model";
            using (var cmd = new NpgsqlCommand(sql, con))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    models.Add(reader["model_name"].ToString());
                }
            }

            // Stages
            string sql2 = @"SELECT stage_name FROM tbl_stage";
            using (var cmd = new NpgsqlCommand(sql2, con))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    stages.Add(reader["stage_name"].ToString());
                }
            }
        }

        return Ok(new { models, stages });
    }

}
