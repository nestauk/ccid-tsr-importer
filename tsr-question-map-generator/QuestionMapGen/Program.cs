
using Newtonsoft.Json;

var POLICIES_CSV_URL = args[0];
var QUESTIONS_CSV_URL = args[1];
var SECTIONS_CSV_URL = args[2];
var MODULES_CSV_URL = args[3];

Console.WriteLine($"Questions CSV: {QUESTIONS_CSV_URL}");
Console.WriteLine($"Policies CSV:  {POLICIES_CSV_URL}");
Console.WriteLine($"Sections CSV:  {SECTIONS_CSV_URL}");
Console.WriteLine($"Modules CSV:   {SECTIONS_CSV_URL}");

var questions = await DownloadHelper.DownloadCsvAsync<QuestionCSV>(QUESTIONS_CSV_URL);
var policies = await DownloadHelper.DownloadCsvAsync<PolicyCSV>(POLICIES_CSV_URL);
var sections = await DownloadHelper.DownloadCsvAsync<SectionCSV>(SECTIONS_CSV_URL);
var modules = await DownloadHelper.DownloadCsvAsync<ModuleCSV>(MODULES_CSV_URL);

Console.WriteLine($"{questions.Count()} question records.");
Console.WriteLine($"{policies.Count()} policy records.");
Console.WriteLine($"{sections.Count()} section records.");
Console.WriteLine($"{modules.Count()} module records.");

var questionMap = new QuestionMap();

questionMap.summary.policies.pre.AddRange(policies.Where(p => p.conversation == "pre"));
questionMap.transport.policies.pre.AddRange(policies.Where(p => p.module == "transport" && p.conversation == "pre"));
questionMap.heat.policies.pre.AddRange(policies.Where(p => p.module == "heat" && p.conversation == "pre"));
questionMap.food.policies.pre.AddRange(policies.Where(p => p.module == "food" && p.conversation == "pre"));

questionMap.summary.policies.post.AddRange(policies.Where(p => p.conversation == "post"));
questionMap.transport.policies.post.AddRange(policies.Where(p => p.module == "transport" && p.conversation == "post"));
questionMap.heat.policies.post.AddRange(policies.Where(p => p.module == "heat" && p.conversation == "post"));
questionMap.food.policies.post.AddRange(policies.Where(p => p.module == "food" && p.conversation == "post"));

foreach (var module in new[] { questionMap.summary, questionMap.transport, questionMap.heat, questionMap.food })
{
    module.policies_title = modules.Single(m => m.module == module.name).policies_title;
    module.policies_sub_title = modules.Single(m => m.module == module.name).policies_sub_title;

    var module_questions = questions.Where(q => q.module == module.name);
    var chart_indices = module_questions.Select(q => q.chart_index).Distinct();
    var module_section_names = module_questions.Select(q => q.section).Distinct();
    var module_sections = module_section_names.Select(section_name => new Section()
    {
        module = module.name,
        section = section_name,
        section_index = sections.Single(s => s.module == module.name && s.section == section_name).section_index,
        title = sections.Single(s => s.module == module.name && s.section == section_name).title,
        sub_title = sections.Single(s => s.module == module.name && s.section == section_name).sub_title,
        background = sections.Single(s => s.module == module.name && s.section == section_name).background

    }).ToList();
    module.sections.AddRange(module_sections);

    foreach (var chart_index in chart_indices.OrderBy(i => i))
    {
        var questionCsvSet = module_questions.Where(q => q.chart_index == chart_index);
        var record = new QuestionChart()
        {
            chart_index = chart_index,
            votes = questionCsvSet.ToList(),
            section = questionCsvSet.First().section,
            stage_id = questionCsvSet.First().stage_id,
            vote_id = questionCsvSet.First().vote_id,
            type = questionCsvSet.First().type,
            chart_type = questionCsvSet.First().chart_type,
            chart_width = questionCsvSet.First().chart_width,
            show_legend = questionCsvSet.First().show_legend,
            module = questionCsvSet.First().module,
        };
        module.sections.Single(s => s.section == record.section && s.module == record.module).questions.Add(record);
    }

}

var json = JsonConvert.SerializeObject(questionMap, Formatting.Indented);

Console.WriteLine();
Console.WriteLine(json);

await File.WriteAllTextAsync("question-map.json", json);
