﻿namespace VehicleRentalSystem.Application.Interfaces.Repositories;

public interface IUnitOfWork
{
    IAppUserRepository AppUser { get; set; }
    
    ICustomerRepository Customer { get; set; }

    IDamageRequestRepository DamageRequest { get; set; }

    IOfferRepository Offer { get; set; }

    IRentalRepository Rental { get; set; }

    IStaffRepository Staff { get; set; }

    IVehicleRepository Vehicle { get; set; }

    void Save();
}