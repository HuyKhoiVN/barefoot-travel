namespace barefoot_travel.DTOs.Section
{
    public class SectionPreviewDto
    {
        public int SectionId { get; set; }
        public string HomepageTitle { get; set; } = string.Empty;
        public string LayoutStyle { get; set; } = "grid";
        public string SelectionMode { get; set; } = "auto";
        public List<HomepageTourDto> Tours { get; set; } = new();
    }
}

