using System.Collections.Generic;

namespace AegisCore.Models
{
    // Модель для отображения списка объектов КИИ с пагинацией и сортировкой
    public class CriticalInfrastructureViewModel
    {
        public List<CiiObject> Objects { get; set; } = new List<CiiObject>();
        
        // Параметры поиска и сортировки
        public string Search { get; set; } = string.Empty;
        public string Sort { get; set; } = "name"; // Сортировка по умолчанию
        public string SortOrder { get; set; } = "asc";

        // Параметры пагинации
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalItems { get; set; } = 0;
        public int PageSize { get; set; } = 10;
    }

    // Модель самого объекта КИИ
    public class CiiObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;          // Наименование объекта
        public string Type { get; set; } = string.Empty;          // Тип объекта
        public string Address { get; set; } = string.Empty;       // Адрес
        public string Purpose { get; set; } = string.Empty;       // Назначение
        public string Sphere { get; set; } = string.Empty;        // Сфера деятельности
        public string Category { get; set; } = "Не категорирован"; // Категория значимости
    }
}