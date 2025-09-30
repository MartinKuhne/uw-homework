using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProductApi.Model
{
    public class Category
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}
