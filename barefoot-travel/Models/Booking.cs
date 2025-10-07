using System;
using System.Collections.Generic;

namespace barefoot_travel.Models;

public partial class Booking
{
    public int Id { get; set; }

    public int TourId { get; set; }

    public int? UserId { get; set; }

    public DateTime? StartDate { get; set; }

    public int People { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string NameCustomer { get; set; } = null!;

    public string? Email { get; set; }

    public string? Note { get; set; }

    public decimal TotalPrice { get; set; }

    public int StatusTypeId { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public DateTime CreatedTime { get; set; }

    public DateTime? UpdatedTime { get; set; }

    public string? UpdatedBy { get; set; }

    public bool Active { get; set; }
}
