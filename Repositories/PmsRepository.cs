using NMPMS.Models;
using Npgsql;

namespace NMPMS.Repositories
{
    //public interface IPmsRepository
    //{
    //    Task CreateIssueAsync(PmsRecord record);
    //    Task<List<PmsRecord>> GetByStageAndModel(string stage, string model);
    //}

    public class IPmsRepository : PmsRepository
    {
        private readonly dbconfig _db;

        public IPmsRepository(dbconfig db)
        {
            _db = db;
        }

        // CREATE a new issue
        public async Task CreateIssueAsync(PmsRecord record)
        {
            using var con = _db.GetConnection();
            await con.OpenAsync();

            string sql = @"
                INSERT INTO pms_records (
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
                );";

            using var cmd = new NpgsqlCommand(sql, con);

            cmd.Parameters.AddWithValue("pms_create", record.PmsCreate ?? "");
            cmd.Parameters.AddWithValue("person_incharge", record.PersonInCharge ?? "");
            cmd.Parameters.AddWithValue("problem_name", record.ProblemName ?? "");
            cmd.Parameters.AddWithValue("phenomenon_details", record.PhenomenonDetails ?? "");
            cmd.Parameters.AddWithValue("stage", record.Stage ?? "");
            cmd.Parameters.AddWithValue("model", record.Model ?? "");
            cmd.Parameters.AddWithValue("serial_number", record.SerialNumber ?? "");
            cmd.Parameters.AddWithValue("area_detection", record.AreaDetection ?? "");
            cmd.Parameters.AddWithValue("process", record.Process ?? "");
            cmd.Parameters.AddWithValue("issued_by", record.IssuedBy ?? "");
            cmd.Parameters.AddWithValue("issued_date", (object)record.IssuedDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("part_code", record.PartCode ?? "");
            cmd.Parameters.AddWithValue("part_name", record.PartName ?? "");
            cmd.Parameters.AddWithValue("supplier", record.Supplier ?? "");
            cmd.Parameters.AddWithValue("problem_photo", (object)record.ProblemPhoto ?? DBNull.Value);
            cmd.Parameters.AddWithValue("attachment", (object)record.Attachment ?? DBNull.Value);
            cmd.Parameters.AddWithValue("attachment_name", (object)record.AttachmentName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("control_no", record.ControlNo ?? "");

            await cmd.ExecuteNonQueryAsync();
        }

        // READ: Fetch by Stage & Model
        public async Task<List<PmsRecord>> GetByStageAndModel(string stage, string model)
        {
            var list = new List<PmsRecord>();

            using var con = _db.GetConnection();
            await con.OpenAsync();

            string sql = @"
                SELECT control_no, pms_create, person_incharge,
                       problem_name, phenomenon_details, stage, model,
                       serial_number, area_detection, process,
                       issued_by, issued_date, part_name, supplier, attachment_name, problem_photo
                FROM pms_records
                WHERE stage = @stage AND model = @model;";

            using var cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("stage", stage ?? "");
            cmd.Parameters.AddWithValue("model", model ?? "");

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var record = new PmsRecord
                {
                    ControlNo = reader["control_no"]?.ToString(),
                    PmsCreate = reader["pms_create"]?.ToString(),
                    PersonInCharge = reader["person_incharge"]?.ToString(),
                    ProblemName = reader["problem_name"]?.ToString(),
                    PhenomenonDetails = reader["phenomenon_details"]?.ToString(),
                    Stage = reader["stage"]?.ToString(),
                    Model = reader["model"]?.ToString(),
                    SerialNumber = reader["serial_number"]?.ToString(),
                    AreaDetection = reader["area_detection"]?.ToString(),
                    Process = reader["process"]?.ToString(),
                    IssuedBy = reader["issued_by"]?.ToString(),
                    IssuedDate = reader["issued_date"] == DBNull.Value ? null : (DateTime?)reader["issued_date"],
                    PartName = reader["part_name"]?.ToString(),
                    Supplier = reader["supplier"]?.ToString(),
                    AttachmentName = reader["attachment_name"]?.ToString(),
                    ProblemPhoto = reader["problem_photo"]?.ToString()
                };

                list.Add(record);
            }

            return list;
        }
    }
}