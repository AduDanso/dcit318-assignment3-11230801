using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthSystem
{
    // ===== Generic Repository =====
    public class Repository<T>
    {
        private readonly List<T> items = new();

        public void Add(T item)
        {
            items.Add(item);
        }

        public List<T> GetAll()
        {
            // Return a copy to avoid external mutation of internal list
            return new List<T>(items);
        }

        // Returns first match or default (null)
        public T? GetById(Func<T, bool> predicate)
        {
            return items.FirstOrDefault(predicate);
        }

        // Remove by predicate; returns true if removed
        public bool Remove(Func<T, bool> predicate)
        {
            var item = items.FirstOrDefault(predicate);
            if (item is null) return false;
            return items.Remove(item);
        }
    }

    // ===== Models =====
    public class Patient
    {
        public int Id { get; }
        public string Name { get; }
        public int Age { get; }
        public string Gender { get; }

        public Patient(int id, string name, int age, string gender)
        {
            Id = id;
            Name = name;
            Age = age;
            Gender = gender;
        }

        public override string ToString()
        {
            return $"Patient: {Name} (ID: {Id}), Age: {Age}, Gender: {Gender}";
        }
    }

    public class Prescription
    {
        public int Id { get; }
        public int PatientId { get; }
        public string MedicationName { get; }
        public DateTime DateIssued { get; }

        public Prescription(int id, int patientId, string medicationName, DateTime dateIssued)
        {
            Id = id;
            PatientId = patientId;
            MedicationName = medicationName;
            DateIssued = dateIssued;
        }

        public override string ToString()
        {
            return $"Prescription {Id} for Patient {PatientId}: {MedicationName} (Issued: {DateIssued:yyyy-MM-dd})";
        }
    }

    // ===== HealthSystemApp =====
    public class HealthSystemApp
    {
        private readonly Repository<Patient> _patientRepo = new();
        private readonly Repository<Prescription> _prescriptionRepo = new();
        private readonly Dictionary<int, List<Prescription>> _prescriptionMap = new();

        public void SeedData()
        {
            // Add patients
            _patientRepo.Add(new Patient(1, "Grace Oduro", 30, "Female"));
            _patientRepo.Add(new Patient(2, "Sandra Vulley", 45, "Feman"));
            _patientRepo.Add(new Patient(3, "Esther Attim", 60, "Female"));

            // Add prescriptions (ensure PatientIds match patients above)
            _prescriptionRepo.Add(new Prescription(100, 1, "Amoxicillin 500mg", DateTime.Now.AddDays(-10)));
            _prescriptionRepo.Add(new Prescription(101, 1, "Ibuprofen 200mg", DateTime.Now.AddDays(-5)));
            _prescriptionRepo.Add(new Prescription(102, 2, "Metformin 500mg", DateTime.Now.AddDays(-30)));
            _prescriptionRepo.Add(new Prescription(103, 3, "Atorvastatin 20mg", DateTime.Now.AddDays(-15)));
            _prescriptionRepo.Add(new Prescription(104, 2, "Lisinopril 10mg", DateTime.Now.AddDays(-2)));
        }

        public void BuildPrescriptionMap()
        {
            _prescriptionMap.Clear();
            var allPrescriptions = _prescriptionRepo.GetAll();

            foreach (var p in allPrescriptions)
            {
                if (!_prescriptionMap.ContainsKey(p.PatientId))
                {
                    _prescriptionMap[p.PatientId] = new List<Prescription>();
                }

                _prescriptionMap[p.PatientId].Add(p);
            }
        }

        public void PrintAllPatients()
        {
            var patients = _patientRepo.GetAll();
            Console.WriteLine("=== All Patients ===");
            foreach (var patient in patients)
            {
                Console.WriteLine(patient);
            }
            Console.WriteLine();
        }

        public List<Prescription> GetPrescriptionsByPatientId(int patientId)
        {
            if (_prescriptionMap.TryGetValue(patientId, out var list))
            {
                // return a copy
                return new List<Prescription>(list);
            }
            return new List<Prescription>();
        }

        public void PrintPrescriptionsForPatient(int patientId)
        {
            var patient = _patientRepo.GetById(p => (p as Patient)!.Id == patientId);
            if (patient is null)
            {
                Console.WriteLine($"No patient found with ID {patientId}");
                return;
            }

            Console.WriteLine($"Prescriptions for {patient.Name} (ID: {patientId}):");

            var prescriptions = GetPrescriptionsByPatientId(patientId);
            if (prescriptions.Count == 0)
            {
                Console.WriteLine("  No prescriptions found.");
            }
            else
            {
                foreach (var pres in prescriptions)
                {
                    Console.WriteLine("  " + pres);
                }
            }

            Console.WriteLine();
        }
    }

    // ===== Main entry =====
    public class Program
    {
        public static void Main(string[] args)
        {
            var app = new HealthSystemApp();

            // Seed and build
            app.SeedData();
            app.BuildPrescriptionMap();

            // Print all patients
            app.PrintAllPatients();

            // Demonstrate fetching prescriptions for a particular patient
            // (choose an existing patientId e.g., 1 or 2 or 3)
            int chosenPatientId = 2;
            app.PrintPrescriptionsForPatient(chosenPatientId);

            // Try a patient without prescriptions
            int missingPatientId = 999;
            app.PrintPrescriptionsForPatient(missingPatientId);
        }
    }
}
