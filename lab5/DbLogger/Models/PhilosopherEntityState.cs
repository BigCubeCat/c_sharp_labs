using System;

namespace DbLogger.Models
{
    /// <summary>
    /// Запись состояния философа в определённый момент времени.
    /// Поля соответствуют вашему описанию:
    /// PhilosopherId (FK) CountEatingFood (int) HungryTime (int) IsEating (int) LeftFork (int)
    /// </summary>
    public class PhilosopherEntityState
    {
        public int Id { get; set; }

        public int PhilosopherId { get; set; }
        public PhilosopherEntity? Philosopher { get; set; }

        public int CountEatingFood { get; set; }
        public int HungryTime { get; set; }

        /// <summary>
        /// 0 или 1 в соответствии с вашим требованием (можно хранить как int)
        /// </summary>
        public int IsEating { get; set; }

        /// <summary>
        /// Ссылка на левую вилку (Id вилки). Храним как int FK.
        /// </summary>
        public int LeftForkId { get; set; }
        public ForkEntity? LeftFork { get; set; }

        /// <summary>
        /// Ссылка на временную метку (TimeStamp), в которой эта запись была сделана.
        /// TimeStamp содержит набор состояний (несколько).
        /// </summary>
        public int TimeStampId { get; set; }
        public TimeStamp? TimeStamp { get; set; }
    }
}
