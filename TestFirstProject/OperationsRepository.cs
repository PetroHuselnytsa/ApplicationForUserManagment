using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text.Json;
using TestFirstProject.Contexts;
using TestFirstProject.Models;

namespace TestFirstProject
{
    public class OperationsRepository
    {
        private readonly PersonsContext _personsContext;

        private static readonly JsonSerializerOptions PersonJsonOptions = new()
        {
            Converters = { new PersonConverter() }
        };

        // Whitelist of allowed sort properties to prevent expression injection
        private static readonly HashSet<string> AllowedSortProperties =
            new(StringComparer.OrdinalIgnoreCase) { "Name", "Age", "Id" };

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
                    "name" => query.Where(p => p.Name.ToLower().StartsWith(searchQuery.ToLower())),
                    "age" => query.Where(p => p.Age.ToString().StartsWith(searchQuery)),
                    "id" => query.Where(p => p.Id.ToLower().StartsWith(searchQuery.ToLower())),
                    _ => query.Where(_ => false) // Unknown filter returns empty
                };
            }

            query = ApplyOrderFilters(request, query);

            var persons = await query.ToListAsync();
            await response.WriteAsJsonAsync(persons);
        }

        private static IQueryable<Person> ApplyOrderFilters(HttpRequest request, IQueryable<Person> query)
        {
            request.Headers.TryGetValue("Order-type", out var searchOrderType);
            request.Headers.TryGetValue("Order-filter", out var searchOrderFilter);
            var orderType = searchOrderType.ToString();
            var orderFilter = searchOrderFilter.ToString();

            if (orderType == "none" || string.IsNullOrEmpty(orderType))
            {
                return query;
            }

            if (!AllowedSortProperties.Contains(orderFilter))
            {
                return query;
            }

            var parameter = Expression.Parameter(typeof(Person), "p");
            var property = Expression.Property(parameter, orderFilter);
            var lambda = Expression.Lambda<Func<Person, object>>(
                Expression.Convert(property, typeof(object)), parameter);

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
            Person? person;
            try
            {
                person = await request.ReadFromJsonAsync<Person>(PersonJsonOptions);
            }
            catch (JsonException)
            {
                response.StatusCode = 400;
                await response.WriteAsJsonAsync(new { message = "Invalid JSON format." });
                return;
            }

            if (person == null)
            {
                response.StatusCode = 400;
                await response.WriteAsJsonAsync(new { message = "Request body cannot be empty." });
                return;
            }

            person.Id = GenerateId();
            _personsContext.Persons.Add(person);
            await _personsContext.SaveChangesAsync();
            await response.WriteAsJsonAsync(person);
        }

        public async Task UpdatePerson(HttpResponse response, HttpRequest request)
        {
            Person? personNewData;
            try
            {
                personNewData = await request.ReadFromJsonAsync<Person>(PersonJsonOptions);
            }
            catch (JsonException)
            {
                response.StatusCode = 400;
                await response.WriteAsJsonAsync(new { message = "Invalid JSON format." });
                return;
            }

            if (personNewData == null)
            {
                response.StatusCode = 400;
                await response.WriteAsJsonAsync(new { message = "Request body cannot be empty." });
                return;
            }

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
    }
}
