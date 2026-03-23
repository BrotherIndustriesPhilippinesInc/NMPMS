using Microsoft.AspNetCore.Mvc;
using NMPMS.Models;
using NMPMS.Services;
using NMPMS.ViewModels;
using Npgsql;
using System.Diagnostics;
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

            string query = @"SELECT control_no,pms_create,person_incharge,
                        problem_name,phenomenon_details,stage,model,
                        serial_number,area_detection,process,
                        issued_by, issued_date,part_name,supplier,attachment_name,problem_photo
                        FROM pms_records 
                        WHERE stage = @stage AND model = @model";

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
            //string issuedDate = form["issued_date"];
            string partCode = form["part_code"];
            string partName = form["part_name"];
            string supplier = form["supplier"];
            string controlNo = form["control_no"];
            DateTime? issuedDate = null;

            if (!string.IsNullOrWhiteSpace(form["issued_date"]))
            {
                issuedDate = DateTime.Parse(form["issued_date"]);
            }
            string personInCharge = "Jeffrey Reyes";
            string problemPhotoPath = null;
            if (form.Files["problem_photos[]"] != null && form.Files["problem_photos[]"].Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "upload/PhotoUpload");
                var file = form.Files["problem_photos[]"];
                string uniqueFileName = controlNo + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                problemPhotoPath = Path.Combine("upload/PhotoUpload", uniqueFileName);
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

                attachmentPath = Path.Combine("upload/AttachmentFile", uniqueFileName);
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
                    cmd.Parameters.AddWithValue("pms_create", pmsCreate);
                    cmd.Parameters.AddWithValue("person_incharge", personInCharge ?? "");
                    cmd.Parameters.AddWithValue("problem_name", problemName ?? "");
                    cmd.Parameters.AddWithValue("phenomenon_details", phenomenonDetails ?? "");
                    cmd.Parameters.AddWithValue("stage", stage ?? "");
                    cmd.Parameters.AddWithValue("model", model ?? "");
                    cmd.Parameters.AddWithValue("serial_number", serialNumber ?? "");
                    cmd.Parameters.AddWithValue("area_detection", areaDetection ?? "");
                    cmd.Parameters.AddWithValue("process", process ?? "");
                    cmd.Parameters.AddWithValue("issued_by", issuedBy ?? "");
                    //cmd.Parameters.AddWithValue("issued_date", issuedDate ?? "");
                    cmd.Parameters.AddWithValue("issued_date", issuedDate.HasValue ? issuedDate.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("part_code", partCode ?? "");
                    cmd.Parameters.AddWithValue("part_name", partName ?? "");
                    cmd.Parameters.AddWithValue("supplier", supplier ?? "");
                    cmd.Parameters.AddWithValue("problem_photo", (object)problemPhotoPath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("attachment", (object)attachmentPath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("attachment_name", attachmentName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("control_no", controlNo ?? "");

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return Json(new { status = "success", control_no = controlNo });
        }
        catch (Exception ex)
        {
            return Json(new { status = "error", message = ex.Message });
        }
    }


}
