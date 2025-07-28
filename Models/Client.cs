using System;
using System.Collections.Generic;

namespace test2.Models;

public partial class Client
{
    public int CId { get; set; }

    public string CName { get; set; } = null!;

    public string CAccount { get; set; } = null!;

    public string? CPassword { get; set; }

    public string? CPhone { get; set; }

    public int Permission { get; set; }

    public string? FacebookId { get; set; }

    public string? GoogleId { get; set; }

    public virtual ICollection<Borrow> Borrows { get; set; } = new List<Borrow>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Participation> Participations { get; set; } = new List<Participation>();

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
