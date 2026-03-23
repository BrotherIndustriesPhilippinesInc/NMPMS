using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Npgsql;
using Org.BouncyCastle.Asn1.Crmf;
using System.Diagnostics;

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
                            finish_analysisdate,
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

                    string sql = @"SELECT 
                            control_no,
                            assembly,
                            parts,
                            machine,
                            system,
                            ia_attachement,
                            fg_treatment,
                            process_change,
                            wi_change,
                            re_education,
                            change_manpower,
                            other,
                            action_date,
                            added_by
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
    }
}
