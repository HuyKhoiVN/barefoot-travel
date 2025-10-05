using System;
using System.Collections.Generic;

namespace barefoot_travel.Models;

public partial class Account
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Photo { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public DateTime CreatedTime { get; set; }

    public DateTime? UpdatedTime { get; set; }

    public string? UpdatedBy { get; set; }

    public bool Active { get; set; }

    public int RoleId { get; set; }
}
