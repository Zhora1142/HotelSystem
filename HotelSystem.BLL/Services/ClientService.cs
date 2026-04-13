using HotelSystem.BLL.DTOs;
using HotelSystem.DAL;
using HotelSystem.Domain;
using HotelSystem.Domain.Entities;
using HotelSystem.Domain.Security;

namespace HotelSystem.BLL.Services;

public interface IClientService
{
    Task<IEnumerable<ClientDto>> GetAllClientsAsync();
    Task<ClientDto?> GetClientByIdAsync(int id);
    Task<ClientDto?> AuthenticateAsync(string login, string password);
    Task CreateClientAsync(string fullName, string phone, string email, string login, string password);
    Task DeleteClientAsync(int id);
}

public class ClientService : IClientService
{
    private readonly IUnitOfWork _unitOfWork;

    public ClientService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ClientDto>> GetAllClientsAsync()
    {
        var clients = await _unitOfWork.Clients.FindAsync(c => c.Role == UserRole.Client);
        return clients
            .OrderBy(c => c.FullName)
            .Select(MapClient)
            .ToList();
    }

    public async Task<ClientDto?> GetClientByIdAsync(int id)
    {
        var client = await _unitOfWork.Clients.GetByIdAsync(id);
        return client == null ? null : MapClient(client);
    }

    public async Task<ClientDto?> AuthenticateAsync(string login, string password)
    {
        var client = await _unitOfWork.Clients.FirstOrDefaultAsync(c => c.Login == login);
        if (client == null || !PasswordHasher.VerifyPassword(password, client.PasswordHash))
        {
            return null;
        }

        return MapClient(client);
    }

    public async Task CreateClientAsync(string fullName, string phone, string email, string login, string password)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new InvalidOperationException("Введите ФИО клиента.");
        }

        if (string.IsNullOrWhiteSpace(login))
        {
            throw new InvalidOperationException("Введите логин клиента.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Введите пароль клиента.");
        }

        var existing = await _unitOfWork.Clients.FirstOrDefaultAsync(c => c.Login == login);
        if (existing != null)
        {
            throw new InvalidOperationException("Пользователь с таким логином уже существует.");
        }

        var client = new Client
        {
            FullName = fullName.Trim(),
            Phone = phone.Trim(),
            Email = email.Trim(),
            Login = login.Trim(),
            PasswordHash = PasswordHasher.HashPassword(password),
            Role = UserRole.Client
        };

        await _unitOfWork.Clients.AddAsync(client);
        await _unitOfWork.CompleteAsync();
    }

    public async Task DeleteClientAsync(int id)
    {
        var client = await _unitOfWork.Clients.GetByIdAsync(id);
        if (client == null)
        {
            return;
        }

        var hasActiveBookings = (await _unitOfWork.Bookings.FindAsync(b =>
            b.ClientId == id && b.Status != BookingStatus.Completed && b.Status != BookingStatus.Cancelled)).Any();

        if (hasActiveBookings)
        {
            throw new InvalidOperationException("Нельзя удалить клиента с активными бронированиями.");
        }

        _unitOfWork.Clients.Remove(client);
        await _unitOfWork.CompleteAsync();
    }

    private static ClientDto MapClient(Client client)
    {
        return new ClientDto
        {
            Id = client.Id,
            FullName = client.FullName,
            Phone = client.Phone,
            Email = client.Email,
            Login = client.Login,
            RoleName = client.Role == UserRole.Admin ? "Администратор" : "Клиент"
        };
    }
}
