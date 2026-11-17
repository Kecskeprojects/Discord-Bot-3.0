using System;
using System.Collections.Generic;

namespace DatabaseORM.Model;

public partial class IdolGroup
{
    public int GroupId { get; set; }

    public string Name { get; set; } = null!;

    public DateOnly? DebutDate { get; set; }

    public string? FullName { get; set; }

    public string? FullKoreanName { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }

    public virtual ICollection<Idol> Idols { get; set; } = new List<Idol>();
}
