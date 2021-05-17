using AutoMapper;
using Ecommerce.AdminPanel.ViewModels;
using Ecommerce.Shared.Entities;

namespace Ecommerce.AdminPanel.Models
{
    public class AdminPanelMappingProfile : Profile
    {
        public AdminPanelMappingProfile()
        {
            // TODO not used yet
            CreateMap<ProductForm,Product>();
            CreateMap<Product,ProductForm>();
        }
        
    }
}