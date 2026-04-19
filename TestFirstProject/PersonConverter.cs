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
            int personAge = 0;
            string userId = "Undefined";

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string? propertyName = reader.GetString();
                    reader.Read();

                    switch (propertyName?.ToLowerInvariant())
                    {
                        case "age" when reader.TokenType == JsonTokenType.Number:
                            personAge = reader.GetInt32();
                            break;
                        case "age" when reader.TokenType == JsonTokenType.String:
                            if (int.TryParse(reader.GetString(), out int parsedAge))
                                personAge = parsedAge;
                            break;
                        case "name":
                            personName = reader.GetString() ?? personName;
                            break;
                        case "id":
                            userId = reader.GetString() ?? userId;
                            break;
                    }
                }
                else if (reader.TokenType is JsonTokenType.StartObject or JsonTokenType.StartArray)
                {
                    reader.Skip();
                }
            }

            return new Person(personName, personAge) { Id = userId };
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
