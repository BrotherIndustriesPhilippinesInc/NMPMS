using NMPMS.Models;
//using NMPMS.Repositories;

namespace NMPMS.Repositories
{
    public interface PmsRepository
    {
        Task CreateIssueAsync(PmsRecord record);
        Task<List<PmsRecord>> GetByStageAndModel(string stage, string model);
    }
}
    