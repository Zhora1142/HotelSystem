using HotelSystem.BLL.DTOs;
using HotelSystem.DAL;
using HotelSystem.Domain;

namespace HotelSystem.BLL.Services;

public interface IAdditionalServiceService
{
    Task<IEnumerable<AdditionalServiceDto>> GetAllServicesAsync(bool activeOnly = false);
    Task CreateServiceAsync(string name, decimal price, ServiceChargeType chargeType, string description);
    Task UpdateServiceAsync(int id, string name, decimal price, ServiceChargeType chargeType, string description, bool isActive);
    Task DeleteServiceAsync(int id);
}

public class AdditionalServiceService : IAdditionalServiceService
{
    private readonly IUnitOfWork _unitOfWork;

    public AdditionalServiceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<AdditionalServiceDto>> GetAllServicesAsync(bool activeOnly = false)
    {
        var services = await _unitOfWork.AdditionalServices.GetAllAsync();
        return services
            .Where(s => !activeOnly || s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new AdditionalServiceDto
            {
                Id = s.Id,
                Name = s.Name,
                Price = s.Price,
                ChargeTypeKey = s.ChargeType.ToString(),
                ChargeTypeName = s.ChargeType == ServiceChargeType.PerDay ? "За день" : "За заезд",
                Description = s.Description,
                IsActive = s.IsActive
            })
            .ToList();
    }

    public async Task CreateServiceAsync(string name, decimal price, ServiceChargeType chargeType, string description)
    {
        ValidateService(name, price);

        await _unitOfWork.AdditionalServices.AddAsync(new HotelSystem.Domain.Entities.AdditionalService
        {
            Name = name.Trim(),
            Price = price,
            ChargeType = chargeType,
            Description = description.Trim(),
            IsActive = true
        });

        await _unitOfWork.CompleteAsync();
    }

    public async Task UpdateServiceAsync(int id, string name, decimal price, ServiceChargeType chargeType, string description, bool isActive)
    {
        ValidateService(name, price);

        var service = await _unitOfWork.AdditionalServices.GetByIdAsync(id);
        if (service == null)
        {
            throw new InvalidOperationException("Услуга не найдена.");
        }

        service.Name = name.Trim();
        service.Price = price;
        service.ChargeType = chargeType;
        service.Description = description.Trim();
        service.IsActive = isActive;

        await _unitOfWork.CompleteAsync();
    }

    public async Task DeleteServiceAsync(int id)
    {
        var service = await _unitOfWork.AdditionalServices.GetByIdAsync(id);
        if (service == null)
        {
            return;
        }

        service.IsActive = false;
        await _unitOfWork.CompleteAsync();
    }

    private static void ValidateService(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Введите название услуги.");
        }

        if (price <= 0)
        {
            throw new InvalidOperationException("Стоимость услуги должна быть положительной.");
        }
    }
}
