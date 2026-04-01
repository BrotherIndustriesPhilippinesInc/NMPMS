using Microsoft.AspNetCore.Mvc;
using NMPMS.Controllers;
using Npgsql;

namespace NMPMS.Controllers
{
    public class pmsController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}
        private readonly dbconfig _db;

        public pmsController(dbconfig db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> SaveStep()
        {
            var form = Request.Form;
            int step = int.Parse(form["step"]);
            string controlNo = form["control_no"];

            switch (step)
            {
                case 2:
                    return await SaveAnalysis(controlNo, form);
                case 3:
                    return await SaveImmediate_Action(controlNo, form);
                case 4:
                    return await SaveTemp_Action(controlNo, form);
                case 5:
                    return await SavePer_Action(controlNo, form);
                case 6:
                    return await SaveHorizontal(controlNo, form);
                case 7:
                    return await SaveB_Action(controlNo, form);
            }

            return Json(new { status = "error", message = "Invalid step" });
        }

        [HttpPost]
        public async Task<IActionResult> SaveAnalysis(string controlNo, IFormCollection form)
        {
            string analysis_cause = form["analysis_cause"];
            string defect_details = form["defect_details"];
            string problem_category = form["problem_category"];
            string analysis_by = form["analysis_by"];
            string problem_rank = form["problem_rank"];
            string defect_attachment = form["defect_attachment"];
            string cause_photo = form["cause_photo"];

            DateTime? finish_date = null;
            if (!string.IsNullOrWhiteSpace(form["finish_date"]))
                finish_date = DateTime.Parse(form["finish_date"]);

            string analysis_imagePath = null;
            if (form.Files["cause_photo"] != null && form.Files["cause_photo"].Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "upload/AnalysisImages");
                var file = form.Files["cause_photo"];
                string uniqueFileName = controlNo + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                analysis_imagePath = Path.Combine("upload/AnalysisImages", uniqueFileName);
            }

            string attachmentPath = null;
            string attachmentName = null;
            var attachmentFile = form.Files["defect_attachment"];
            if (attachmentFile != null && attachmentFile.Length > 0)
            {
                var attachmentFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "upload", "AnalysisFiles");

                if (!Directory.Exists(attachmentFolder))
                    Directory.CreateDirectory(attachmentFolder);

                attachmentName = attachmentFile.FileName;

                string uniqueFileName = controlNo + Path.GetExtension(attachmentFile.FileName);
                var filePath = Path.Combine(attachmentFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await attachmentFile.CopyToAsync(stream);
                }

                attachmentPath = Path.Combine("upload/AnalysisFiles", uniqueFileName);
            }

            using (var con = _db.GetConnection())
            {
                await con.OpenAsync();

                string sql = @"INSERT INTO tbl_analysis(
                            control_no,
                            analysis_cause,
                            defect_analysis_details,
                            problem_category,
                            finish_analysisdate,
                            attachment,
                            image_cause,
                            analysis_by,
                            problem_rank
                        )
                        VALUES(
                            @controlNo,
                            @analysis_cause,
                            @defect_details,
                            @problem_category,
                            @finish_date,
                            @defect_attachment,
                            @cause_photo,
                            @analysis_by,
                            @problem_rank
                        )
                        ON CONFLICT (control_no)
                        DO UPDATE SET
                            analysis_cause = EXCLUDED.analysis_cause,
                            defect_analysis_details = EXCLUDED.defect_analysis_details,
                            problem_category = EXCLUDED.problem_category,
                            finish_analysisdate = EXCLUDED.finish_analysisdate,
                            attachment = COALESCE(NULLIF(EXCLUDED.attachment,''), tbl_analysis.attachment),
                            image_cause = COALESCE(NULLIF(EXCLUDED.image_cause,''), tbl_analysis.image_cause),
                            analysis_by = EXCLUDED.analysis_by,
                            problem_rank = EXCLUDED.problem_rank;";

                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@controlNo", controlNo);
                    cmd.Parameters.AddWithValue("@analysis_cause", analysis_cause ?? "");
                    cmd.Parameters.AddWithValue("@defect_details", defect_details ?? "");
                    cmd.Parameters.AddWithValue("@problem_category", problem_category ?? "");
                    cmd.Parameters.AddWithValue("@finish_date", finish_date ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@analysis_by", analysis_by ?? "");
                    cmd.Parameters.AddWithValue("@problem_rank", problem_rank ?? "");
                    cmd.Parameters.AddWithValue("@defect_attachment", attachmentPath ?? "");
                    cmd.Parameters.AddWithValue("@cause_photo", analysis_imagePath ?? "");

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return Json(new { status = "success", message = "Analysis saved" });
        }
        [HttpPost]
        public async Task<IActionResult> SaveImmediate_Action(string controlNo, IFormCollection form)
        {
            string assembly = form["assembly"];
            string parts = form["parts"];
            string machine = form["machine"];
            string system = form["system"];

            string fg_treatment = form["fg_treatment"];
            string process_change = form["process_change"];
            string wi_change = form["wi_change"];
            string re_education = form["re_education"];
            string change_manpower = form["change_manpower"];
            string other = form["other_action"];
            string enough_stocks_qty = form["enough_stocks_qty"];
            string trial_reason = form["trial_reason"];
            string sorting_result = form["sorting_result"];

            string action_by = form["action_by"];

            DateTime? action_date = null;
            if (!string.IsNullOrWhiteSpace(form["action_date"]))
                action_date = DateTime.Parse(form["action_date"]);

            string attachmentPath = null;
            var attachmentFile = form.Files["detail_attachment"];

            if (attachmentFile != null && attachmentFile.Length > 0)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "upload", "ImmediateAction");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string uniqueFileName = controlNo + Path.GetExtension(attachmentFile.FileName);
                var filePath = Path.Combine(folder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await attachmentFile.CopyToAsync(stream);
                }

                attachmentPath = Path.Combine("upload/ImmediateAction", uniqueFileName);
            }

            using (var con = _db.GetConnection())
            {
                await con.OpenAsync();

                string sql = @"INSERT INTO public.tbl_immediate_action(
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
                        date_added,
                        added_by,
                        stock_qty,
                        reason,
                        sort_result,
                    )
                    VALUES(
                        @control_no,
                        @assembly,
                        @parts,
                        @machine,
                        @system,
                        @attachment,
                        @fg_treatment,
                        @process_change,
                        @wi_change,
                        @re_education,
                        @change_manpower,
                        @other,
                        @action_date,
                        NOW(),
                        @added_by,
                        @enough_stocks_qty,
                        @trial_reason,
                        @sorting_result,
                    );";

                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@control_no", controlNo);
                    cmd.Parameters.AddWithValue("@assembly", assembly ?? "");
                    cmd.Parameters.AddWithValue("@parts", parts ?? "");
                    cmd.Parameters.AddWithValue("@machine", machine ?? "");
                    cmd.Parameters.AddWithValue("@system", system ?? "");
                    cmd.Parameters.AddWithValue("@attachment", attachmentPath ?? "");
                    cmd.Parameters.AddWithValue("@fg_treatment", fg_treatment ?? "");
                    cmd.Parameters.AddWithValue("@process_change", process_change ?? "");
                    cmd.Parameters.AddWithValue("@wi_change", wi_change ?? "");
                    cmd.Parameters.AddWithValue("@re_education", re_education ?? "");
                    cmd.Parameters.AddWithValue("@change_manpower", change_manpower ?? "");
                    cmd.Parameters.AddWithValue("@other", other ?? "");
                    cmd.Parameters.AddWithValue("@action_date", action_date ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@added_by", action_by ?? "");
                    cmd.Parameters.AddWithValue("@enough_stocks_qty", enough_stocks_qty ?? "");
                    cmd.Parameters.AddWithValue("@trial_reason", trial_reason ?? "");
                    cmd.Parameters.AddWithValue("@sorting_result", sorting_result ?? "");

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return Json(new { status = "success", message = "Immediate Action saved" });
        }

        [HttpPost]
        public async Task<IActionResult> SaveTemp_Action(string controlNo, IFormCollection form)
        {
            string assembly = form["s4_assembly"];
            string parts = form["s4_parts"];
            string machine = form["s4_machine"];
            string system = form["s4_system"];

            string detail_action_by = form["s4_detail_action_by"];

            DateTime? action_date = null;
            if (!string.IsNullOrWhiteSpace(form["implematation_Date"]))
                action_date = DateTime.Parse(form["implematation_Date"]);

            string attachmentPath = null;
            var attachmentFile = form.Files["detail_attachment"];

            if (attachmentFile != null && attachmentFile.Length > 0)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "upload", "ImmediateAction");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string uniqueFileName = controlNo + Path.GetExtension(attachmentFile.FileName);
                var filePath = Path.Combine(folder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await attachmentFile.CopyToAsync(stream);
                }

                attachmentPath = Path.Combine("upload/ImmediateAction", uniqueFileName);
            }

            using (var con = _db.GetConnection())
            {
                await con.OpenAsync();

                string sql = @"INSERT INTO public.tbl_temporaryaction(
                                control_no, 
                                assembly, 
                                parts, 
                                machine, 
                                system, 
                                action_by, 
                                temp_attachment, 
                                implementation_date, 
                                pic, 
                                date_added)
                                VALUES(
                                @control_no,
                                @assembly,
                                @parts,
                                @machine,
                                @system,
                                @added_by,
                                @attachment,
                                @action_date,
                                @added_by,
                                NOW()
                                
                                );";

                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@control_no", controlNo);
                    cmd.Parameters.AddWithValue("@assembly", assembly ?? "");
                    cmd.Parameters.AddWithValue("@parts", parts ?? "");
                    cmd.Parameters.AddWithValue("@machine", machine ?? "");
                    cmd.Parameters.AddWithValue("@system", system ?? "");
                    cmd.Parameters.AddWithValue("@attachment", attachmentPath ?? "");
                    cmd.Parameters.AddWithValue("@action_date", action_date ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@added_by", detail_action_by ?? "");
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return Json(new { status = "success", message = "Temporary Action saved" });
        }

        [HttpPost]
        public async Task<IActionResult> SavePer_Action(string controlNo, IFormCollection form)
        {
            string assembly = form["s5_assembly"];
            string parts = form["s5_parts"];
            string machine = form["s5_machine"];
            string system = form["s5_system"];

            string pic = form["s5_pic"];

            DateTime? action_date = null;
            if (!string.IsNullOrWhiteSpace(form["s5_implematation_Date"]))
                action_date = DateTime.Parse(form["s5_implematation_Date"]);


            using (var con = _db.GetConnection())
            {
                await con.OpenAsync();

                string sql = @"INSERT INTO public.tbl_permanentaction(
                                control_no, 
                                assembly, 
                                parts, 
                                machine, 
                                system, 
                                implementation_date, 
                                pic, 
                                date_added)
                                VALUES(
                                @control_no,
                                @assembly,
                                @parts,
                                @machine,
                                @system,
                                @action_date,
                                @added_by,
                                NOW()
                                
                                );";

                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@control_no", controlNo);
                    cmd.Parameters.AddWithValue("@assembly", assembly ?? "");
                    cmd.Parameters.AddWithValue("@parts", parts ?? "");
                    cmd.Parameters.AddWithValue("@machine", machine ?? "");
                    cmd.Parameters.AddWithValue("@system", system ?? "");
                    cmd.Parameters.AddWithValue("@action_date", action_date ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@added_by", pic ?? "");
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return Json(new { status = "success", message = "Permanent Action saved" });
        }

        [HttpPost]
        public async Task<IActionResult> SaveHorizontal(string controlNo, IFormCollection form)
        {
            string assembly = form["s6_assembly"];
            string parts = form["s6_parts"];
            string machine = form["s6_machine"];
            string system = form["s6_system"];
            string ishorizontal = form["ishorizontal"];
            string model = form["s6_model"];

            //string pic = form["s5_pic"];

            DateTime? action_date = null;
            if (!string.IsNullOrWhiteSpace(form["s6_implematation_Date"]))
                action_date = DateTime.Parse(form["s6_implematation_Date"]);


            using (var con = _db.GetConnection())
            {
                await con.OpenAsync();

                string sql = @"INSERT INTO public.tbl_horizontal(
                                control_no, 
                                assembly, 
                                parts, 
                                machine, 
                                system, 
                                model, 
                                ishorizontal, 
                                implementation_date, 
                                date_added)
                                VALUES(
                                @control_no,
                                @assembly,
                                @parts,
                                @machine,
                                @system,
                                @model,
                                @ishorizontal,
                                @action_date,
                                NOW()
                                
                                );";

                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@control_no", controlNo);
                    cmd.Parameters.AddWithValue("@assembly", assembly ?? "");
                    cmd.Parameters.AddWithValue("@parts", parts ?? "");
                    cmd.Parameters.AddWithValue("@machine", machine ?? "");
                    cmd.Parameters.AddWithValue("@system", system ?? "");
                    cmd.Parameters.AddWithValue("@model", model ?? "");
                    cmd.Parameters.AddWithValue("@ishorizontal", ishorizontal ?? "");
                    cmd.Parameters.AddWithValue("@action_date", action_date ?? (object)DBNull.Value);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return Json(new { status = "success", message = "Horizontal Expansion Action saved" });
        }

        [HttpPost]
        public async Task<IActionResult> SaveB_Action(string controlNo, IFormCollection form)
        {
            string s7_action_judgement = form["s7_action_judgement"];
            string s7_action_no = form["s7_action_no"];
            string s7_rank = form["s7_rank"];
            string s7_pic = form["s7_pic"];
            //string ishorizontal = form["ishorizontal"];
            //string model = form["s6_model"];

            //string pic = form["s5_pic"];

            //DateTime? action_date = null;
            //if (!string.IsNullOrWhiteSpace(form["s6_implematation_Date"]))
            //    action_date = DateTime.Parse(form["s6_implematation_Date"]);


            using (var con = _db.GetConnection())
            {
                await con.OpenAsync();

                string sql = @"INSERT INTO public.tbl_baction(
                                control_no, 
                                action_judgement, 
                                action_no, 
                                rank, 
                                pic, 
                                date_added)
                                VALUES(
                                @control_no,
                                @action_judgement,
                                @action_no,
                                @rank,
                                @pic,
                                NOW()
                                
                                );";

                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@control_no", controlNo);
                    cmd.Parameters.AddWithValue("@action_judgement", s7_action_judgement ?? "");
                    cmd.Parameters.AddWithValue("@action_no", s7_action_no ?? "");
                    cmd.Parameters.AddWithValue("@rank", s7_rank ?? "");
                    cmd.Parameters.AddWithValue("@pic", s7_pic ?? "");
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return Json(new { status = "success", message = "B Action saved" });
        }




    }
}
