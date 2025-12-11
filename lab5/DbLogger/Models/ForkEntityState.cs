using System;

namespace DbLogger.Models
{
    /// <summary>
    /// Запись состояния вилки: UsedTime, BlockTime, AvailableTime, Owner (id философа или null).
    /// </summary>
    public class ForkEntityState
    {
        public int Id { get; set; }

        public int ForkId { get; set; }
        public ForkEntity? Fork { get; set; }

        /// <summary>
        /// Время использования (например, миллисекунды) — long
        /// </summary>
        public long UsedTime { get; set; }

        /// <summary>
        /// Время блокировки (например, миллисекунды) — long
        /// </summary>
        public long BlockTime { get; set; }

        /// <summary>
        /// Время доступности — long
        /// </summary>
        public long AvailableTime { get; set; }

        /// <summary>
        /// Id философа-владельца или null.
        /// </summary>
        public int? OwnerPhilosopherId { get; set; }

        /// <summary>
        /// (Опционально) навигация к философу, если нужно иметь ссылку на объект
        /// </summary>
        public PhilosopherEntity? OwnerPhilosopher { get; set; }

        public int TimeStampId { get; set; }
        public TimeStamp? TimeStamp { get; set; }
    }
}

