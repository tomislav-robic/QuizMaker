using QuizMaker.Core.Entities;
using QuizMaker.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;

namespace UseCases.Services
{
    public class ExportService : IExportService
    {
        [ImportMany(typeof(IExporter))]
        public IEnumerable<IExporter> Exporters { get; set; }

        public ExportService()
        {
            var catalog = new AggregateCatalog();

            string binPath = AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;
            catalog.Catalogs.Add(new DirectoryCatalog(binPath));

            var container = new CompositionContainer(catalog);
            try
            {
                container.ComposeParts(this);
            }
            catch (ReflectionTypeLoadException ex)
            {
                var loaderExceptions = ex.LoaderExceptions;
                foreach (var loaderException in loaderExceptions)
                {
                    Console.WriteLine(loaderException.Message);
                }
            }
        }

        public IExporter GetExporter(string format)
        {
            return Exporters.FirstOrDefault(e => e.ExportFormat.Equals(format, StringComparison.OrdinalIgnoreCase));
        }

        public List<string> GetAvailableFormats()
        {
            return Exporters.Select(e => e.ExportFormat).ToList();
        }

        public byte[] ExportQuiz(Quiz quiz, string format)
        {
            var exporter = GetExporter(format);
            if (exporter == null)
                throw new InvalidOperationException($"Exporter for format '{format}' not found.");

            return exporter.ExportQuiz(quiz);
        }
    }
}
