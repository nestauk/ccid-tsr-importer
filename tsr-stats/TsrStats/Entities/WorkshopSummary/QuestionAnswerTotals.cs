namespace TsrStats.Entities.WorkshopSummary
{
    public class QuestionAnswerTotals
    {
        public string demographic_code { get; set; }
        public string stage_id { get; set; }
        public int min_boundary { get; set; }
        public string vote_id { get; set; }
        public Dictionary<string, int> totals { get; set; }
        public int max_boundary { get; set; }
        public int participants { get; set; }

    }
}