using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using ProductGrpc.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductGrpc.Mapper
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Models.Product, ProductModel>()
                .ForMember(dest => dest.Created, opt => opt.MapFrom(src => 
                                Timestamp.FromDateTime(src.CreatedTime.ToUniversalTime())));

            CreateMap<ProductModel ,Models.Product>()
                .ForMember(dest => dest.CreatedTime, opt => opt.MapFrom(src => 
                                src.Created.ToDateTime()));
        }
    }
}
