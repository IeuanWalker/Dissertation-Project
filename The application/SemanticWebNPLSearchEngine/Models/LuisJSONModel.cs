using System.Collections.Generic;

namespace DissertationOriginal.Models
{
    public class TopScoringIntent
    {
        public string Intent { get; set; }
        public double Score { get; set; }
    }

    public class Intent
    {
        public string intent { get; set; }
        public double Score { get; set; }
    }

    public class Resolution
    {
        public string Value { get; set; }
        public string Comment { get; set; }
        public string Time { get; set; }
    }

    public class Entity
    {
        public string entity { get; set; }
        public string Type { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public Resolution Resolution { get; set; }
        public double? Score { get; set; }
    }

    public class LuisJsonModel
    {
        public string Query { get; set; }
        public TopScoringIntent TopScoringIntent { get; set; }
        public IList<Intent> Intents { get; set; }
        public IList<Entity> Entities { get; set; }
    }
}