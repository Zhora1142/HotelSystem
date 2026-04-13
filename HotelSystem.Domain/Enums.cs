namespace HotelSystem.Domain;

public enum UserRole
{
    Admin,
    Client
}

public enum BookingStatus
{
    Reserved,
    CheckedIn,
    Completed,
    Cancelled
}

public enum RoomAvailabilityStatus
{
    Available,
    Occupied,
    Maintenance
}

public enum ServiceChargeType
{
    PerStay,
    PerDay
}
