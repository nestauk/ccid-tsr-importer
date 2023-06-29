
using Newtonsoft.Json;

var POLICIES_CSV_URL = args[0];
var QUESTIONS_CSV_URL = args[1];

Console.WriteLine($"Questions CSV: {QUESTIONS_CSV_URL}");
Console.WriteLine($"Policies CSV: {POLICIES_CSV_URL}");

var questions = await DownloadHelper.DownloadCsvAsync<QuestionCSV>(QUESTIONS_CSV_URL);
var policies = await DownloadHelper.DownloadCsvAsync<PolicyCSV>(POLICIES_CSV_URL);

Console.WriteLine($"{questions.Count()} question records.");
Console.WriteLine($"{policies.Count()} policy records.");

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
    var relevant_questions = questions.Where(q => q.module == module.name);
    var chart_indices = relevant_questions.Select(q => q.chart_index).Distinct();
    foreach (var chart_index in chart_indices.OrderBy(i => i))
    {
        var questionCsvSet = relevant_questions.Where(q => q.chart_index == chart_index);
        var record = new QuestionChart()
        {
            chart_index = chart_index,
            votes = questionCsvSet.ToList(),
            section = questionCsvSet.First().section,
            stage_id = questionCsvSet.First().stage_id,
            vote_id = questionCsvSet.First().vote_id,
            type = questionCsvSet.First().type,
        };
        module.questions.Add(record);
    }
}

var json = JsonConvert.SerializeObject(questionMap, Formatting.Indented);

Console.WriteLine();
Console.WriteLine(json);

await File.WriteAllTextAsync("question-map.json", json);
