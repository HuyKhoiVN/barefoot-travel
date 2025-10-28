using System;
using System.Collections.Generic;

namespace barefoot_travel.Models;

public partial class HomePageSectionCategory
{
    public int Id { get; set; }

    public int SectionId { get; set; }

    public int CategoryId { get; set; }

    public int DisplayOrder { get; set; }

    public bool Active { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual HomePageSection Section { get; set; } = null!;
}
