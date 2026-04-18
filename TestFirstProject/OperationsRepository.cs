using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text.Json;
using TestFirstProject.Contexts;
using TestFirstProject.Models;

namespace TestFirstProject
{
    public class OperationsRepository
    {
        private static readonly JsonSerializerOptions PersonJsonOptions = new()
        {
            Converters = { new PersonConverter() }
        };

        private readonly PersonsContext _personsContext;

        public OperationsRepository(PersonsContext personsContext)
        {
            _personsContext = personsContext;
        }

        public async Task GetPersons(HttpRequest request, HttpResponse response)
        {
            List<Person> persons = await _personsContext.Persons.AsNoTracking()
                                                                .ToListAsync();
            request.Headers.TryGetValue("Data-filter", out var dataFilter);
            request.Headers.TryGetValue("Search-value", out var searchValue);

            string filterOption = dataFilter.ToString();
            string searchQuery = searchValue.ToString();

            if (string.IsNullOrEmpty(searchQuery))
            {
                await response.WriteAsJsonAsync(SetOrderFilters(request, persons));
                return;
            }

            Dictionary<string, Func<IEnumerable<Person>>> filterActions = new()
            {
                { "name", () => persons.Where(p => p.Name.StartsWith(searchQuery, StringComparison.OrdinalIgnoreCase))},
                { "age", () => persons.Where(p => p.Age.ToString().StartsWith(searchQuery))},
                { "id", () => persons.Where(p => p.Id.StartsWith(searchQuery, StringComparison.OrdinalIgnoreCase))},
            };

            if (!filterActions.TryGetValue(filterOption, out var action))
            {
                await response.WriteAsJsonAsync(new List<Person>());
                return;
            }

            List<Person> filteredPersons = action().ToList();
            await response.WriteAsJsonAsync(SetOrderFilters(request, filteredPersons));
        }

        private List<Person> SetOrderFilters(HttpRequest request, List<Person> persons)
        {
            request.Headers.TryGetValue("Order-type", out var searchOrderType);
            request.Headers.TryGetValue("Order-filter", out var searchOrderFilter);
            var searchType = searchOrderType.ToString();
            var searchFilter = searchOrderFilter.ToString();
            if (searchType != "none")
            {
                var parameter = Expression.Parameter(typeof(Person), "UserParametr");
                var property = Expression.Property(parameter, searchFilter);
                var lambda = Expression.Lambda<Func<Person, object>>(Expression.Convert(property, typeof(object)), parameter);
                var queryable = persons.AsQueryable();
                switch (searchType)
                {
                    case "ascending":
                        return [.. queryable.OrderBy(lambda)];
                    case "descending":
                        return [.. queryable.OrderByDescending(lambda)];
                }
            }
            return persons;
        }

        public async Task DeletePerson(HttpResponse response, string id)
        {
            Person? person = await _personsContext.Persons.FindAsync(id);
            if (person != null)
            {
                _personsContext.Persons.Remove(person);
                await _personsContext.SaveChangesAsync();
                await response.WriteAsJsonAsync(new { message = "Person deleted successfully", person });
            }
            else
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = "Person not found!" });
            }
        }

        public async Task CreatePerson(HttpResponse response, HttpRequest request)
        {
            try
            {
                Person? person = await request.ReadFromJsonAsync<Person>(PersonJsonOptions);
                if (person == null)
                    throw new ArgumentException("Invalid person data.");

                person.Id = GenerateId();
                _personsContext.Persons.Add(person);
                await _personsContext.SaveChangesAsync();
                await response.WriteAsJsonAsync(person);
            }
            catch (Exception exception)
            {
                response.StatusCode = 400;
                await response.WriteAsJsonAsync(new { message = exception.Message });
            }
        }

        public async Task UpdatePerson(HttpResponse response, HttpRequest request)
        {
            try
            {
                Person? personNewData = await request.ReadFromJsonAsync<Person>(PersonJsonOptions);
                if (personNewData == null)
                    throw new ArgumentException("Invalid person data.");

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
            catch (Exception exception)
            {
                response.StatusCode = 400;
                await response.WriteAsJsonAsync(new { message = exception.Message });
            }
        }

        public static string GenerateId()
        {
            Guid guid = Guid.NewGuid();
            string guidString = guid.ToString("N");
            return $"{guidString.Substring(0, 3).ToUpper()}-{guidString.Substring(3, 4)}-{guidString.Substring(7, 4)}";
        }
    }
}
