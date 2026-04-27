using SmartCare.Domain.Entities;
using SmartCare.Domain.IRepositories;
using System.Threading.Tasks;

namespace SmartCare.Application.Handlers.RoleHandler
{
    public class PharmacistRoleLoginHandler : IRoleLoginHandler
    {
        private readonly IUnitOfWork _unitOfWork;

        public string Role => "PHARMACIST";

        public PharmacistRoleLoginHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task HandleAsync(ApplictionUser user)
        {
           var pharmacist = await _unitOfWork.Pharmacists.GetByUserIdAsync(user.Id , true);
            user.Pharmacist = pharmacist;
        }
    }
}
