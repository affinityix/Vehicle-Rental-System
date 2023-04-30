﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VehicleRentalSystem.Domain.Entities;

public class DamageRequest
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid RentalId { get; set; }

    [Required]
    public double RepairCost { get; set; }

    [Required]
    public string DamageDescription { get; set; }

    [Required]
    public bool IsPaid { get; set; } = false;

    public string? PaymentStatus { get; set; }

    public string? ApprovedBy { get; set; }

    [Required]
    public bool IsApproved { get; set; } = false;

    public string ImageURL { get; set; }

    [ValidateNever]
    [ForeignKey("RentalId")]
    public Rental? Rental { get; set; }

    [ValidateNever]
    [ForeignKey("ApprovedBy")]
    public AppUser? Approver { get; set; }
}
