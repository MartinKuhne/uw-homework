using System;
using System.Text.Json.Serialization;

namespace CartApi
{
    public class CartItem
    {
        [JsonPropertyName("productId")]
        public Guid ProductId { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }
}
