namespace DbLogger
{
    public class ForkStateSnapshot
    {
        public int Id { get; set; }
        public bool Used { get; set; }
        public string OwnerName { get; set; } = string.Empty;

        public int TableStateSnapshotId { get; set; }
        public TableStateSnapshot? TableStateSnapshot { get; set; }
    }
}
