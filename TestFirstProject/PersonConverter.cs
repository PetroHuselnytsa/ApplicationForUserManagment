using System.Text.Json.Serialization;
using System.Text.Json;
using TestFirstProject.Models;

namespace TestFirstProject
{
    public class PersonConverter : JsonConverter<Person>
    {
        public override Person? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string personName = "Undefined";
            var personAge = 0;
            string userId = "Undefined";

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString()!;
                    reader.Read();
                    switch (propertyName?.ToLower())
                    {
                        case "age" when reader.TokenType == JsonTokenType.Number:
                            personAge = reader.GetInt32();
                            break;
                        case "age" when reader.TokenType == JsonTokenType.String:
                            string stringValue = reader.GetString()!;
                            if (int.TryParse(stringValue, out int someValue))
                                personAge = someValue;
                            break;
                        case "name":
                            string? name = reader.GetString()!;
                            personName = name != null ? name : personName;
                            break;
                        case "id":
                            string? id = reader.GetString()!;
                            userId = id;
                            break;

                    }
                }
            }

            return new Person(personName, personAge) { Id = userId};
        }
        public override void Write(Utf8JsonWriter writer, Person value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("name", value.Name);
            writer.WriteNumber("age", value.Age);
            writer.WriteString("id", value.Id);
            writer.WriteEndObject();
        }
    }
}
