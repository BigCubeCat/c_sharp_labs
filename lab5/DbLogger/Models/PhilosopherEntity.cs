using System.Collections.Generic;

namespace DbLogger.Models
{
    /// <summary>
    /// Сущность философа (статическая часть: id, имя, принадлежность к Stage).
    /// </summary>
    public class PhilosopherEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        // Ссылка на Stage (чтобы Stage содержал ровно 5 философов)
        public int StageId { get; set; }
        public Stage? Stage { get; set; }

        // Исторические записи состояния
        public ICollection<PhilosopherEntityState> States { get; set; } = new List<PhilosopherEntityState>();
    }
}

