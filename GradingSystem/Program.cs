using System;
using System.Collections.Generic;
using System.IO;

namespace SchoolGradingSystem
{
    // ===== Custom Exceptions =====
    public class InvalidScoreFormatException : Exception
    {
        public InvalidScoreFormatException(string message) : base(message) { }
    }

    public class MissingFieldException : Exception
    {
        public MissingFieldException(string message) : base(message) { }
    }

    // ===== Student Class =====
    public class Student
    {
        public int Id { get; }
        public string FullName { get; }
        public int Score { get; }

        public Student(int id, string fullName, int score)
        {
            Id = id;
            FullName = fullName;
            Score = score;
        }

        public string GetGrade()
        {
            if (Score >= 80 && Score <= 100) return "A";
            if (Score >= 70) return "B";
            if (Score >= 60) return "C";
            if (Score >= 50) return "D";
            return "F";
        }

        public override string ToString()
        {
            return $"{FullName} (ID: {Id}): Score = {Score}, Grade = {GetGrade()}";
        }
    }

    // ===== Student Result Processor =====
    public class StudentResultProcessor
    {
        public List<Student> ReadStudentsFromFile(string inputFilePath)
        {
            var students = new List<Student>();

            using (var reader = new StreamReader(inputFilePath))
            {
                string? line;
                int lineNum = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    lineNum++;
                    var parts = line.Split(',');

                    if (parts.Length != 3)
                    {
                        throw new MissingFieldException($"Line {lineNum}: Missing required fields.");
                    }

                    if (!int.TryParse(parts[0], out int id))
                    {
                        throw new FormatException($"Line {lineNum}: Invalid ID format.");
                    }

                    string fullName = parts[1].Trim();
                    if (string.IsNullOrWhiteSpace(fullName))
                    {
                        throw new MissingFieldException($"Line {lineNum}: Full name is missing.");
                    }

                    if (!int.TryParse(parts[2], out int score))
                    {
                        throw new InvalidScoreFormatException($"Line {lineNum}: Invalid score format.");
                    }

                    students.Add(new Student(id, fullName, score));
                }
            }

            return students;
        }

        public void WriteReportToFile(List<Student> students, string outputFilePath)
        {
            using (var writer = new StreamWriter(outputFilePath))
            {
                foreach (var student in students)
                {
                    writer.WriteLine(student.ToString());
                }
            }
        }
    }

    // ===== Main Entry Point =====
    public class Program
    {
        public static void Main(string[] args)
        {
            // Base directory of the running application
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            // Build absolute paths for input and output files
            string inputFilePath = Path.Combine(basePath, "students.txt");
            string outputFilePath = Path.Combine(basePath, "report.txt");

            var processor = new StudentResultProcessor();

            try
            {
                var students = processor.ReadStudentsFromFile(inputFilePath);
                processor.WriteReportToFile(students, outputFilePath);
                Console.WriteLine($"Report written to: {outputFilePath}");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Error: The file '{inputFilePath}' was not found.");
            }
            catch (InvalidScoreFormatException ex)
            {
                Console.WriteLine($"Invalid score format: {ex.Message}");
            }
            catch (MissingFieldException ex)
            {
                Console.WriteLine($"Missing field: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }
    }
}
