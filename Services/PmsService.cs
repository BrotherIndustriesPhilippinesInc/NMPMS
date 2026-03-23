using NMPMS.Repositories;
using NMPMS.ViewModels;
using NMPMS.Models;

namespace NMPMS.Services
{
    public class PmsService : IPmsService
    {
        private readonly PmsRepository _repo;
        private readonly IWebHostEnvironment _env;

        public PmsService(PmsRepository repo, IWebHostEnvironment env)
        {
            _repo = repo;
            _env = env;
        }

        public async Task CreateIssueAsync(CreateIssueViewModel model)
        {
            string photoPath = await SaveFile(model.ProblemPhoto, "PhotoUpload", model.ControlNo);
            string attachmentPath = await SaveFile(model.Attachment, "AttachmentFile", model.ControlNo);

            var record = new PmsRecord
            {
                PmsCreate = model.PmsCreate,
                PersonInCharge = "Jeffrey Reyes",
                ProblemName = model.ProblemName,
                PhenomenonDetails = model.PhenomenonDetails,
                Stage = model.Stage,
                Model = model.Model,
                SerialNumber = model.SerialNumber,
                AreaDetection = model.AreaDetection,
                Process = model.Process,
                IssuedBy = model.IssuedBy,
                IssuedDate = model.IssuedDate,
                PartCode = model.PartCode,
                PartName = model.PartName,
                Supplier = model.Supplier,
                ProblemPhoto = photoPath,
                Attachment = attachmentPath,
                ControlNo = model.ControlNo
            };

            await _repo.CreateIssueAsync(record);
        }

        //public Task<List<PmsRecord>> FetchAsync(string stage, string model)
        //{
        //    throw new NotImplementedException();
        //}
        public async Task<List<PmsRecord>> FetchAsync(string stage, string model)
        {

            return await _repo.GetByStageAndModel(stage, model);
        }

        private async Task<string> SaveFile(IFormFile file, string folder, string controlNo)
        {
            if (file == null || file.Length == 0)
                return null;

            string uploadPath = Path.Combine(_env.WebRootPath, "upload", folder);

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            string fileName = controlNo + Path.GetExtension(file.FileName);
            string fullPath = Path.Combine(uploadPath, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return Path.Combine("upload", folder, fileName);
        }
    }
}
