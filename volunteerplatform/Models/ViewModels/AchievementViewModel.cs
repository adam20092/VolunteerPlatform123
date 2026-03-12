namespace volunteerplatform.Models.ViewModels
{
    public class AchievementItem
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        /// <summary>Bootstrap Icons class, e.g. "bi-trophy-fill"</summary>
        public string Icon { get; set; } = string.Empty;
        /// <summary>CSS hex colour for the badge accent</summary>
        public string Color { get; set; } = string.Empty;
        public bool Unlocked { get; set; }
        /// <summary>Optional progress value (0–max)</summary>
        public int Progress { get; set; }
        public int MaxProgress { get; set; }
        /// <summary>Category label shown on the card</summary>
        public string Category { get; set; } = string.Empty;
    }

    public class AchievementsViewModel
    {
        public List<AchievementItem> Achievements { get; set; } = new();
        public int TotalPoints { get; set; }
        public int CompletedMissions { get; set; }
        public int DonationCount { get; set; }
        public int UnlockedCount => Achievements.Count(a => a.Unlocked);
        public string OverallBadge { get; set; } = string.Empty;
        public string OverallBadgeColor { get; set; } = string.Empty;
    }
}
