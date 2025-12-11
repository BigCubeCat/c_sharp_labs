using System.Collections.Generic;

namespace DbLogger.Models
{
    /// <summary>
    /// Сущность вилки: статические данные (номер, принадлежность к Stage).
    /// </summary>
    public class ForkEntity
    {
        public int Id { get; set; }

        /// <summary>
        /// Номер вилки (1..N)
        /// </summary>
        public int Number { get; set; }

        public int StageId { get; set; }
        public Stage? Stage { get; set; }

        public ICollection<ForkEntityState> States { get; set; } = new List<ForkEntityState>();
    }
}

