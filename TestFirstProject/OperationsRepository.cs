using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text.Json;
using TestFirstProject.Contexts;
using TestFirstProject.Exceptions;
using TestFirstProject.Models;

namespace TestFirstProject
{
    public class OperationsRepository
    {
        private readonly PersonsContext _personsContext;

        private static readonly JsonSerializerOptions PersonJsonOptions = CreatePersonJsonOptions();

        private static readonly HashSet<string> AllowedSortProperties =
            new(StringComparer.OrdinalIgnoreCase) { "name", "age", "id" };

        public OperationsRepository(PersonsContext personsContext)
        {
            _personsContext = personsContext;
        }

        public async Task GetPersons(HttpRequest request, HttpResponse response)
        {
            request.Headers.TryGetValue("Data-filter", out var dataFilter);
            request.Headers.TryGetValue("Search-value", out var searchValue);

            string filterOption = dataFilter.ToString();
            string searchQuery = searchValue.ToString();

            IQueryable<Person> query = _personsContext.Persons.AsNoTracking();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = filterOption switch
                {
                    "name" => query.Where(p => p.Name.StartsWith(searchQuery)),
                    "age" => query.Where(p => p.Age.ToString().StartsWith(searchQuery)),
                    "id" => query.Where(p => p.Id.StartsWith(searchQuery)),
                    _ => query.Where(p => false)
                };
            }

            query = ApplyOrdering(request, query);

            var persons = await query.ToListAsync();
            await response.WriteAsJsonAsync(persons);
        }

        private static IQueryable<Person> ApplyOrdering(HttpRequest request, IQueryable<Person> query)
        {
            request.Headers.TryGetValue("Order-type", out var searchOrderType);
            request.Headers.TryGetValue("Order-filter", out var searchOrderFilter);
            var orderType = searchOrderType.ToString();
            var orderField = searchOrderFilter.ToString();

            if (orderType == "none" || string.IsNullOrEmpty(orderField))
                return query;

            if (!AllowedSortProperties.Contains(orderField))
                throw new ValidationException($"Invalid sort field: '{orderField}'. Allowed fields: {string.Join(", ", AllowedSortProperties)}");

            var parameter = Expression.Parameter(typeof(Person), "p");
            var property = Expression.Property(parameter, orderField);
            var lambda = Expression.Lambda<Func<Person, object>>(Expression.Convert(property, typeof(object)), parameter);

            return orderType switch
            {
                "ascending" => query.OrderBy(lambda),
                "descending" => query.OrderByDescending(lambda),
                _ => query
            };
        }

        public async Task DeletePerson(HttpResponse response, string id)
        {
            Person? person = await _personsContext.Persons.FindAsync(id);
            if (person == null)
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = "Person not found!" });
                return;
            }

            _personsContext.Persons.Remove(person);
            await _personsContext.SaveChangesAsync();
            await response.WriteAsJsonAsync(new { message = "Person deleted successfully", person });
        }

        public async Task CreatePerson(HttpResponse response, HttpRequest request)
        {
            Person? person = await request.ReadFromJsonAsync<Person>(PersonJsonOptions);
            if (person == null)
                throw new ValidationException("Invalid person data.");

            person.Id = GenerateId();
            _personsContext.Persons.Add(person);
            await _personsContext.SaveChangesAsync();
            await response.WriteAsJsonAsync(person);
        }

        public async Task UpdatePerson(HttpResponse response, HttpRequest request)
        {
            Person? personNewData = await request.ReadFromJsonAsync<Person>(PersonJsonOptions);
            if (personNewData == null)
                throw new ValidationException("Invalid person data.");

            Person? person = await _personsContext.Persons.FirstOrDefaultAsync(u => u.Id == personNewData.Id);
            if (person == null)
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = "Person not found!" });
                return;
            }

            person.Name = personNewData.Name;
            person.Age = personNewData.Age;
            await _personsContext.SaveChangesAsync();
            await response.WriteAsJsonAsync(person);
        }

        public static string GenerateId()
        {
            string guidString = Guid.NewGuid().ToString("N");
            return $"{guidString[..3].ToUpper()}-{guidString[3..7]}-{guidString[7..11]}";
        }

        private static JsonSerializerOptions CreatePersonJsonOptions()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new PersonConverter());
            return options;
        }
    }
}
