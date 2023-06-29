public class QuestionMap
{
    public ModuleMap summary { get; set; } = new ModuleMap("summary");
    public ModuleMap transport { get; set; } = new ModuleMap("transport");
    public ModuleMap heat { get; set; } = new ModuleMap("heat");
    public ModuleMap food { get; set; } = new ModuleMap("food");
}

public class ModuleMap
{
    public ModuleMap(string name) { this.name = name; }
    public string name { get; set; }

    public PoliciesDiff policies { get; set; } = new PoliciesDiff();
    public List<QuestionChart> questions { get; set; } = new List<QuestionChart>();
}

public class PoliciesDiff
{
    public List<PolicyCSV> pre { get; set; } = new List<PolicyCSV>();
    public List<PolicyCSV> post { get; set; } = new List<PolicyCSV>();
}

public class QuestionChart
{
    public int? chart_index { get; set; }
    public string? section { get; set; }
    public string? stage_id { get; set; }
    public string? vote_id { get; set; }
    public string? type { get; set; }
    public List<QuestionCSV> votes { get; set; } = new List<QuestionCSV>();
}
