using System;
using System.Collections.Generic;

namespace test2.Models;

public partial class ParticipationStatus
{
    public int ParticipationStatusId { get; set; }

    public string? ParticipationStatus1 { get; set; }

    public virtual ICollection<Participation> Participations { get; set; } = new List<Participation>();
}
