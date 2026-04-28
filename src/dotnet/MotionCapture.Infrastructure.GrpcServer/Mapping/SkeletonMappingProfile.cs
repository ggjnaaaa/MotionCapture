using AutoMapper;
using System.Numerics;
using Proto = MotionCapture.Grpc.Contracts.Skeleton;

namespace MotionCapture.Infrastructure.GrpcServer.Mapping;

public class SkeletonMappingProfile : Profile
{
    public SkeletonMappingProfile()
    {
        CreateMap<Proto.SkeletonJoint, MotionCapture.Core.Models.SkeletonJoint>()
            .ForMember(d => d.Position, o => o.MapFrom(s => new Vector3(s.PosX, s.PosY, s.PosZ)))
            .ForMember(d => d.Rotation, o => o.MapFrom(s => new Quaternion(s.RotX, s.RotY, s.RotZ, s.RotW)));

        CreateMap<Proto.Skeleton, MotionCapture.Core.Models.Skeleton>()
            .ForMember(d => d.Joints, o => o.MapFrom(s => s.Joints.ToList()))
            .ForMember(d => d.TimestampMs, o => o.MapFrom(s => s.TimestampMs));

        CreateMap<MotionCapture.Core.Models.SkeletonJoint, Proto.SkeletonJoint>()
            .ForMember(d => d.PosX, o => o.MapFrom(s => s.Position.X))
            .ForMember(d => d.PosY, o => o.MapFrom(s => s.Position.Y))
            .ForMember(d => d.PosZ, o => o.MapFrom(s => s.Position.Z))
            .ForMember(d => d.RotX, o => o.MapFrom(s => s.Rotation.X))
            .ForMember(d => d.RotY, o => o.MapFrom(s => s.Rotation.Y))
            .ForMember(d => d.RotZ, o => o.MapFrom(s => s.Rotation.Z))
            .ForMember(d => d.RotW, o => o.MapFrom(s => s.Rotation.W));

        CreateMap<MotionCapture.Core.Models.Skeleton, Proto.Skeleton>()
            .ForMember(d => d.Joints, o => o.MapFrom(s => s.Joints))
            .ForMember(d => d.TimestampMs, o => o.MapFrom(s => s.TimestampMs));
    }
}
