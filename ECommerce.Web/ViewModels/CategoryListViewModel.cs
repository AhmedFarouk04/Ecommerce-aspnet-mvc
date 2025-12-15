using ECommerce.Application.DTOs;
using System;
using System.Collections.Generic;

namespace ECommerce.Web.ViewModels
{
    public class CategoryListViewModel
    {
        public IEnumerable<CategoryDto> Categories { get; set; } = new List<CategoryDto>();

        public string? Search { get; set; }

        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public int TotalPages =>
            (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
