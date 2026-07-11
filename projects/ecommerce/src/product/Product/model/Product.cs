using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProductApi.Model
{
	/// <summary>
	/// Represents a product in the product catalog.
	/// </summary>
	public class Product
	{
		/// <summary>Unique identifier for the product.</summary>
		[JsonPropertyName("id")]
		public Guid Id { get; set; } = Guid.Empty;

		/// <summary>Product display name.</summary>
		[Required]
		[JsonPropertyName("name")]
		public string Name { get; set; } = string.Empty;

		/// <summary>Optional longer description of the product.</summary>
		[JsonPropertyName("description")]
		public string? Description { get; set; }

		/// <summary>Unit price for the product in the indicated currency.</summary>
		[Required]
		[Range(0, double.MaxValue)]
		[JsonPropertyName("price")]
		public decimal Price { get; set; }

		/// <summary>ISO currency code (e.g. "USD"). Defaults to USD.</summary>
		[JsonPropertyName("currency")]
		public string Currency { get; set; } = "USD";

        /// <summary>Reference to category (foreign key).</summary>
        [JsonPropertyName("categoryId")]
        public Guid? CategoryId { get; set; }

        /// <summary>Navigation property for the product's category.</summary>
        [JsonPropertyName("category")]
        public Category? Category { get; set; }

		/// <summary>Whether the product is active and should be shown in listings.</summary>
		[JsonPropertyName("isActive")]
		public bool IsActive { get; set; } = true;

		/// <summary>When the product was created (UTC).</summary>
		[JsonPropertyName("createdAt")]
		public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

		/// <summary>When the product was last updated (UTC).</summary>
		[JsonPropertyName("updatedAt")]
		public DateTimeOffset? UpdatedAt { get; set; }

		// Optional physical properties
		[JsonPropertyName("weightKg")]
		public double? WeightKg { get; set; }

		[JsonPropertyName("widthCm")]
		public double? WidthCm { get; set; }

		[JsonPropertyName("heightCm")]
		public double? HeightCm { get; set; }

		[JsonPropertyName("depthCm")]
		public double? DepthCm { get; set; }
	}
}
