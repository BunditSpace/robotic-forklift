using CsvHelper;
using CsvHelper.Configuration;
using Forklift.Application.DTOs;
using Forklift.Application.Interfaces;
using Forklift.Core.Entities;
using Forklift.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Text.Json;

namespace Forklift.Application.Services
{
    public class ForkliftService : IForkliftService
    {
        private readonly IForkliftRepository _repository;

        public ForkliftService(IForkliftRepository repository)
        {
            ArgumentNullException.ThrowIfNull(repository);
            _repository = repository;
        }

        /// <summary>
        /// Asynchronously retrieves all forklifts and maps them to data transfer objects.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of forklift data
        /// transfer objects.</returns>
        public async Task<IEnumerable<ForkliftDto>> GetAllForkliftsAsync()
        {
            var forklifts = await _repository.GetAllAsync();
            return forklifts.Select(f => new ForkliftDto
            {
                Id = f.Id,
                Name = f.Name,
                ModelNumber = f.ModelNumber,
                ManufacturingDate = f.ManufacturingDate
            });
        }

        /// <summary>
        /// Imports forklifts from a file.
        /// </summary>
        /// <param name="file">The file to import.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ImportForkliftsAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null.");

            using var fileStream = file.OpenReadStream();
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var dtos = extension switch
            {
                ".csv" => ParseCsv(fileStream),
                ".json" => await ParseJsonAsync(fileStream),
                _ => throw new ArgumentException("Unsupported file format. Only .csv and .json are supported.")
            };

            await ValidateNoDuplicateDataAsync(dtos);

            var newForklifts = dtos.Select(dto => new ForkLift
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                ModelNumber = dto.ModelNumber,
                ManufacturingDate = dto.ManufacturingDate ?? DateTime.UtcNow
            });

            await _repository.AddRangeAsync(newForklifts);
        }

        /// <summary>
        /// Parses a CSV file stream into a list of ForkliftDto objects.
        /// </summary> <param name="fileStream">The stream of the CSV file to parse.</param>
        /// <returns>A list of ForkliftDto objects parsed from the CSV file.</returns>
        private static List<ForkliftDto> ParseCsv(Stream fileStream)
        {
            using var reader = new StreamReader(fileStream);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null,
                PrepareHeaderForMatch = args => string.Concat(
                    args.Header.Where(char.IsLetterOrDigit)
                ).ToLowerInvariant()
            };

            using var csv = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<ForkliftDtoMap>();
            return csv.GetRecords<ForkliftDto>().ToList();
        }

        /// <summary>
        /// Parses a JSON file stream into a list of ForkliftDto objects.   
        /// </summary> <param name="fileStream">The stream of the JSON file to parse.</param>
        /// <returns>A list of ForkliftDto objects parsed from the JSON file.</returns>
        private static async Task<List<ForkliftDto>> ParseJsonAsync(Stream fileStream)
        {
            using var reader = new StreamReader(fileStream);
            var jsonContent = await reader.ReadToEndAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var records = JsonSerializer.Deserialize<List<ForkliftDto>>(jsonContent, options);
            return records ?? new List<ForkliftDto>();
        }

        /// <summary>
        /// Validates that there are no duplicate forklift records in the imported data or existing in the database.
        /// </summary> <param name="forkliftDtos">The collection of ForkliftDto objects to validate.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ValidateNoDuplicateDataAsync(IEnumerable<ForkliftDto> forkliftDtos)
        {
            var importedKeys = forkliftDtos
                .Select(CreateDuplicateKey)
                .ToList();

            var hasDuplicatesInFile = importedKeys
                .GroupBy(key => key)
                .Any(group => group.Count() > 1);

            if (hasDuplicatesInFile)
            {
                throw new ArgumentException("Duplicate forklift records found in the import file.");
            }

            var existingKeys = (await _repository.GetAllAsync())
                .Select(forklift => CreateDuplicateKey(forklift.Name, forklift.ModelNumber))
                .ToHashSet();

            var hasDuplicatesInDatabase = importedKeys
                .Any(key => existingKeys.Contains(key));

            if (hasDuplicatesInDatabase)
            {
                throw new ArgumentException("One or more forklift records already exist.");
            }
        }

        /// Creates a unique key for a forklift record based on its name and model number, used for duplicate detection.
        /// <param name="forkliftDto">The ForkliftDto object to create the key for.</param>
        /// <returns>A string representing the unique key for the forklift record.</returns>
        private static string CreateDuplicateKey(ForkliftDto forkliftDto)
        {
            return CreateDuplicateKey(forkliftDto.Name, forkliftDto.ModelNumber);
        }

        /// <summary>
        /// Creates a unique key for a forklift record based on its name and model number, used for duplicate detection.
        /// </summary>
        /// <param name="name">The name of the forklift.</param>
        /// <param name="modelNumber">The model number of the forklift.</param>
        /// <returns>A string representing the unique key for the forklift record.</returns>
        private static string CreateDuplicateKey(string name, string modelNumber)
        {
            var normalizedName = name.Trim().ToLowerInvariant();
            var normalizedModelNumber = modelNumber.Trim().ToLowerInvariant();
            return $"{normalizedName}|{normalizedModelNumber}";
        }

        /// <summary> Deletes a forklift item from the repository by its ID.
        /// </summary> <param name="id">The ID of the forklift item to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task DeleteForkliftByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Invalid forklift ID.");
            return _repository.DeleteForkliftByIdAsync(id);
        }

        /// <summary>
        /// Deletes all forklift records from the repository.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task DeleteAllForkliftAsync()
        {
            return _repository.DeleteAllForkliftAsync();
        }

        private sealed class ForkliftDtoMap : ClassMap<ForkliftDto>
        {
            public ForkliftDtoMap()
            {
                Map(x => x.Name).Name("name", "forkliftname", "forklift");
                Map(x => x.ModelNumber).Name("modelnumber", "model", "modelno");
                Map(x => x.ManufacturingDate).Name("manufacturingdate", "manufactureddate", "date");
            }
        }
    }
}
