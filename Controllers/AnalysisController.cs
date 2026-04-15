using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Npgsql;
using Org.BouncyCastle.Asn1.Crmf;
using System.Data;
using System.Diagnostics;
using System.Net.Mail;

namespace NMPMS.Controllers
{
    public class AnalysisController : Controller
    {
       

        private readonly dbconfig _db;

        public AnalysisController(dbconfig db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPheno(string control_no)
        {
            try
            {
                using (var con = _db.GetConnection())
                {
                    await con.OpenAsync();

                    string sql = @"SELECT * FROM public.pms_records WHERE control_no = @control_no";

                    using (var cmd = new NpgsqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@control_no", control_no);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return Json(new
                                {
                                    status = "success",
                                    data = new
                                    {
                                        pms_name = reader["problem_name"]?.ToString(),
                                        pms_details = reader["phenomenon_details"]?.ToString(),
                                        attachment = reader["attachment"]?.ToString(),
                                        //problem_photos = reader["problem_photo"]?.ToString(),
                                        pms_stage = reader["stage"]?.ToString(),
                                        pms_model = reader["model"]?.ToString(),
                                        pms_serial = reader["serial_number"]?.ToString(),
                                        pms_area = reader["area_detection"]?.ToString(),
                                        pms_process = reader["process"]?.ToString(),
                                        pms_issued_by = reader["issued_by"]?.ToString(),
                                        pms_partcode = reader["part_code"]?.ToString(),
                                        pms_partname = reader["part_name"]?.ToString(),
                                        supplier = reader["supplier"]?.ToString(),
                                        pic = reader["person_incharge"]?.ToString(),

                                    }
                                });
                            }
                        }
                    }
                }

                return Json(new { status = "empty" });
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProblemFiles(string control_no,int steps)
        {
            var list = new List<string>();

            using (var con = _db.GetConnection())
            {
                await con.OpenAsync();

                string sql = @"SELECT file_path 
                       FROM pms_problem_files 
                       WHERE control_no = @control_no and steps = @steps";

                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@control_no", control_no);
                    //cmd.Parameters.AddWithValue("@steps", steps);
                    cmd.Parameters.Add("@steps", NpgsqlTypes.NpgsqlDbType.Integer).Value = Convert.ToInt32(steps);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(reader["file_path"].ToString());
                        }
                    }
                }
            }

            return Json(list);
        }
        [HttpGet]
        public async Task<IActionResult> GetAnalysis(string control_no)
        {
            try
            {
                using (var con = _db.GetConnection())
                {
                    await con.OpenAsync();

                    string sql = @"SELECT 
                            analysis_cause,
                            defect_analysis_details,
                            attachment,
                            problem_category,
                            TO_CHAR(finish_analysisdate, 'YYYY-MM-DD') AS finish_analysisdate,
                            image_cause,
                            analysis_by,
                            problem_rank
                           FROM tbl_analysis
                           WHERE control_no = @control_no";

                    using (var cmd = new NpgsqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@control_no", control_no);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return Json(new
                                {
                                    status = "success",
                                    data = new
                                    {
                                        analysis_cause = reader["analysis_cause"]?.ToString(),
                                        defect_details = reader["defect_analysis_details"]?.ToString(),
                                        attachment = reader["attachment"]?.ToString(),
                                        problem_category = reader["problem_category"]?.ToString(),
                                        finish_date = reader["finish_analysisdate"]?.ToString(),
                                        image_cause = reader["image_cause"]?.ToString(),
                                        analysis_by = reader["analysis_by"]?.ToString(),
                                        problem_rank = reader["problem_rank"]?.ToString()
                                    }
                                });
                            }
                        }
                    }
                }

                return Json(new { status = "empty" });
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetImmediateAction(string control_no)
        {
            try
            {
                using (var con = _db.GetConnection())
                {
                    await con.OpenAsync();

                    string sql = @"SELECT TO_CHAR(action_date, 'YYYY-MM-DD') as action_date,
                          *
                        FROM public.tbl_immediate_action
                        WHERE control_no = @control_no";

                    using (var cmd = new NpgsqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@control_no", control_no);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return Json(new
                                {
                                    status = "success",
                                    data = new
                                    {
                                        assembly = reader["assembly"]?.ToString(),
                                        parts = reader["parts"]?.ToString(),
                                        machine = reader["machine"]?.ToString(),
                                        system = reader["system"]?.ToString(),
                                        attachment = reader["ia_attachement"]?.ToString(),
                                        fg_treatment = reader["fg_treatment"]?.ToString(),
                                        process_change = reader["process_change"]?.ToString(),
                                        wi_change = reader["wi_change"]?.ToString(),
                                        re_education = reader["re_education"]?.ToString(),
                                        change_manpower = reader["change_manpower"]?.ToString(),
                                        other = reader["other"]?.ToString(),
                                        action_date = reader["action_date"]?.ToString(),
                                        trial_reason = reader["reason"]?.ToString(),
                                        sorting_result = reader["sort_result"]?.ToString(),
                                        enough_stocks_qty = reader["stock_qty"]?.ToString(),
                                        action_by = reader["added_by"]?.ToString()
                                    }
                                });
                            }
                        }
                    }
                }

                return Json(new { status = "empty" });
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetTempAction(string control_no)
        {
            try
            {
                using (var con = _db.GetConnection())
                {
                    await con.OpenAsync();

                    string sql = @"SELECT TO_CHAR(implementation_date,'YYYY-MM-DD') as implementation_date, * FROM public.tbl_temporaryaction
                        WHERE control_no = @control_no";

                    using (var cmd = new NpgsqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@control_no", control_no);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return Json(new
                                {
                                    status = "success",
                                    data = new
                                    {
                                        s4_assembly = reader["assembly"]?.ToString(),
                                        s4_parts = reader["parts"]?.ToString(),
                                        s4_machine = reader["machine"]?.ToString(),
                                        s4_system = reader["system"]?.ToString(),
                                        s4_actionby = reader["action_by"]?.ToString(),
                                        s4_attachment = reader["temp_attachment"]?.ToString(),
                                        s4_impdate = reader["implementation_date"]?.ToString(),
                                        s4_pic = reader["pic"]?.ToString(),

                                    }
                                });
                            }
                        }
                    }
                }

                return Json(new { status = "empty" });
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPerAction(string control_no)
        {
            try
            {
                using (var con = _db.GetConnection())
                {
                    await con.OpenAsync();

                    string sql = @"SELECT TO_CHAR(implementation_date,'YYYY-MM-DD') as implementation_date, * FROM public.tbl_permanentaction
                        WHERE control_no = @control_no";

                    using (var cmd = new NpgsqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@control_no", control_no);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return Json(new
                                {
                                    status = "success",
                                    data = new
                                    {
                                        s5_assembly = reader["assembly"]?.ToString(),
                                        s5_parts = reader["parts"]?.ToString(),
                                        s5_machine = reader["machine"]?.ToString(),
                                        s5_system = reader["system"]?.ToString(),
                                        s5_impdate = reader["implementation_date"]?.ToString(),
                                        s5_attachment = reader["per_attachment"]?.ToString(),
                                        s5_pic = reader["pic"]?.ToString(),
                                    }
                                });
                            }
                        }
                    }
                }

                return Json(new { status = "empty" });
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetHorizontal(string control_no)
        {
            try
            {
                using (var con = _db.GetConnection())
                {
                    await con.OpenAsync();

                    string sql = @"SELECT TO_CHAR(implementation_date,'YYYY-MM-DD') as implementation_date, * FROM public.tbl_horizontal
                        WHERE control_no = @control_no";

                    using (var cmd = new NpgsqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@control_no", control_no);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return Json(new
                                {
                                    status = "success",
                                    data = new
                                    {
                                        s6_assembly = reader["assembly"]?.ToString(),
                                        s6_parts = reader["parts"]?.ToString(),
                                        s6_machine = reader["machine"]?.ToString(),
                                        s6_system = reader["system"]?.ToString(),
                                        s6_impdate = reader["implementation_date"]?.ToString(),
                                        s6_model = reader["model"]?.ToString(),
                                        s6_ishorizontal = reader["ishorizontal"]?.ToString()
                                    }
                                });
                            }
                        }
                    }
                }

                return Json(new { status = "empty" });
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBAction(string control_no)
        {
            try
            {
                using (var con = _db.GetConnection())
                {
                    await con.OpenAsync();

                    string sql = @"SELECT * FROM public.tbl_baction
                        WHERE control_no = @control_no";

                    using (var cmd = new NpgsqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@control_no", control_no);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return Json(new
                                {
                                    status = "success",
                                    data = new
                                    {
                                        s7_actionjudgement = reader["action_judgement"]?.ToString(),
                                        s7_actionno = reader["action_no"]?.ToString(),
                                        s7_rank = reader["rank"]?.ToString(),
                                        s7_pic = reader["pic"]?.ToString(),

                                    }
                                });
                            }
                        }
                    }
                }

                return Json(new { status = "empty" });
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }
        }

    }
}
