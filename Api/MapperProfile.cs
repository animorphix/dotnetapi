using Api.Models;
using Api.Models.Post;
using AutoMapper;
using Common;

namespace Api;

public class MapperProfile: Profile
{
    public MapperProfile(){

        CreateMap<Models.CreateUserModel, DAL.Entities.User>()
            .ForMember(d=>d.Id, m=>m.MapFrom(s=>Guid.NewGuid()))
            .ForMember(d=>d.PasswordHash, m=>m.MapFrom(s=>HashHelper.GetHash(s.Password)))
            .ForMember(d=>d.BirthDate, m=>m.MapFrom(s=>s.BirthDate.UtcDateTime));

        CreateMap<DAL.Entities.User, Models.UserModel>();

        CreateMap<DAL.Entities.Avatar, Models.AttachModel>();

        CreateMap<DAL.Entities.PostContent, Models.AttachModel>();

        CreateMap<MetaDataModel, DAL.Entities.PostContent>();

        CreateMap<MetaWithPath, DAL.Entities.PostContent>();

        CreateMap<CreatePostModel, DAL.Entities.Post>()
            .ForMember(d=>d.PostContents, m=>m.MapFrom(s=>s.Contents));
    }

}
