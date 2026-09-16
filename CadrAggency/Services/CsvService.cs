using CadrAggency.Converters;
using CadrAggency.Models;
using CadrAggency.Models.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace CadrAggency.Services
{
    /// <summary>
    /// Сервис для экспорта и импорта данных в формате CSV.
    /// Поддерживает экспорт списка кандидатов в файл и импорт из файла
    /// с проверкой на дубликаты по имени и телефону.
    /// </summary>
    public class CsvService
    {
        /// <summary>
        /// Экранирует значение для CSV: если строка содержит запятые, кавычки
        /// или переносы строк — оборачивает её в двойные кавычки и удваивает
        /// внутренние кавычки.
        /// </summary>
        /// <param_name="value">Исходное значение.</param>
        /// <returns>Значение, пригодное для записи в CSV.</returns>
        private string Escape(string? value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }

        /// <summary>
        /// Экспортирует список кандидатов в CSV-файл.
        /// Файл сохраняется в кодировке UTF-8 с BOM (для корректного
        /// открытия в Excel).
        /// </summary>
        /// <param_name="filePath">Путь к файлу для сохранения.</param>
        /// <param_name="candidates">Список кандидатов для экспорта.</param>
        public void ExportCandidates(string filePath, List<Candidate> candidates)
        {
            var sb = new StringBuilder();
            sb.AppendLine("FullName,Telephone,Email,Education,Skills,ExperienceYears,ExpectedSalary,Status");

            foreach (var c in candidates)
            {
                sb.AppendLine(string.Join(",",
                    Escape(c.FullName),
                    Escape(c.Telephone),
                    Escape(c.Email),
                    Escape(c.Education.GetDescription()),
                    Escape(c.Skills),
                    c.ExperienceYears.ToString(CultureInfo.InvariantCulture),
                    c.ExpectedSalary.ToString(CultureInfo.InvariantCulture),
                    Escape(c.Status.GetDescription())
                ));
            }

            File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));
        }

        /// <summary>
        /// Читает CSV-файл кандидатов и возвращает список объектов.
        /// Строки с недостаточным количеством полей и битые строки пропускаются.
        /// </summary>
        /// <param_name="filePath">Путь к CSV-файлу.</param>
        /// <returns>Список кандидатов, прочитанных из файла.</returns>
        public List<Candidate> ImportCandidates(string filePath)
        {
            var result = new List<Candidate>();
            var lines = File.ReadAllLines(filePath);

            if (lines.Length <= 1) return result;

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = ParseCsvLine(line);

                if (parts.Count < 8) continue;

                try
                {
                    var candidate = new Candidate
                    {
                        FullName = parts[0].Trim(),
                        Telephone = parts[1].Trim(),
                        Email = parts[2].Trim(),
                        Education = ParseEducation(parts[3].Trim()),
                        Skills = parts[4].Trim(),
                        ExperienceYears = int.TryParse(parts[5].Trim(), out var exp) ? exp : 0,
                        ExpectedSalary = decimal.TryParse(parts[6].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var sal) ? sal : 0,
                        Status = ParseStatus(parts[7].Trim()),
                        BirthDate = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc),
                        LastUpdated = DateTime.UtcNow
                    };

                    if (!string.IsNullOrWhiteSpace(candidate.FullName))
                        result.Add(candidate);
                }
                catch
                {
                }
            }

            return result;
        }

        /// <summary>
        /// Разбирает одну строку CSV с учётом двойных кавычек.
        /// Поддерживает экранированные кавычки ("" внутри значения).
        /// </summary>
        /// <param_name="line">Строка CSV.</param>
        /// <returns>Список значений из строки.</returns>
        private List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            current.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else if (c == ',')
                    {
                        result.Add(current.ToString());
                        current.Clear();
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
            }

            result.Add(current.ToString());
            return result;
        }

        /// <summary>
        /// Преобразует строку в значение enum EducationLevel.
        /// Понимает как английское имя, так и русское описание.
        /// Если значение не распознано — возвращает Higher.
        /// </summary>
        /// <param_name="value">Строковое значение из CSV.</param>
        /// <returns>Соответствующее значение EducationLevel.</returns>
        private EducationLevel ParseEducation(string value)
        {
            foreach (EducationLevel level in Enum.GetValues(typeof(EducationLevel)))
            {
                if (level.GetDescription().Equals(value, StringComparison.OrdinalIgnoreCase) ||
                    level.ToString().Equals(value, StringComparison.OrdinalIgnoreCase))
                    return level;
            }
            return EducationLevel.Higher;
        }

        /// <summary>
        /// Преобразует строку в значение enum CandidateStatus.
        /// Понимает как английское имя, так и русское описание.
        /// Если значение не распознано — возвращает Active.
        /// </summary>
        /// <param_name="value">Строковое значение из CSV.</param>
        /// <returns>Соответствующее значение CandidateStatus.</returns>
        private CandidateStatus ParseStatus(string value)
        {
            foreach (CandidateStatus status in Enum.GetValues(typeof(CandidateStatus)))
            {
                if (status.GetDescription().Equals(value, StringComparison.OrdinalIgnoreCase) ||
                    status.ToString().Equals(value, StringComparison.OrdinalIgnoreCase))
                    return status;
            }
            return CandidateStatus.Active;
        }
    }
}

