using System;
using System.Collections.Generic;

namespace test2.Models;

public partial class Participation
{
    public int ParticipationId { get; set; }

    public int CId { get; set; }

    public int ActivityId { get; set; }

    public DateTime ParticipationDate { get; set; }

    public int ParticipationStatusId { get; set; }

    public virtual Activity Activity { get; set; } = null!;

    public virtual Client CIdNavigation { get; set; } = null!;

    public virtual ParticipationStatus ParticipationStatus { get; set; } = null!;
}
