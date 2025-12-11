using System;
using System.Collections.Generic;

namespace DbLogger.Models
{
    /// <summary>
    /// Временная метка, содержащая набор состояний (несколько PhilosopherEntityState и ForkEntityState).
    /// Используется для сохранения полного снимка состояния на конкретный момент.
    /// </summary>
    public class TimeStamp
    {
        public int Id { get; set; }

        /// <summary>
        /// Момент времени (UTC)
        /// </summary>
        public DateTime CreatedAtUtc { get; set; }

        /// <summary>
        /// Опционально: ссылка на Stage (чтобы привязать снимки к конкретной сессии / run)
        /// </summary>
        public int StageId { get; set; }
        public Stage? Stage { get; set; }

        public ICollection<PhilosopherEntityState> PhilosopherStates { get; set; } = new List<PhilosopherEntityState>();
        public ICollection<ForkEntityState> ForkStates { get; set; } = new List<ForkEntityState>();
    }
}
