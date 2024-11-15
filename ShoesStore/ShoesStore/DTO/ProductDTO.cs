﻿using ShoesStore.Model;

namespace ShoesStore.DTO
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int SizeId {  get; set; }
        public int ImageId { get; set; }
    }
}
