using AutoMapper;
using MotionCapture.Core.Models;
using MotionCapture.Grpc.Contracts.Motion;
using FrameLandmarks2D = MotionCapture.Grpc.Contracts.Motion.FrameLandmarks2D;
using Joint2D = MotionCapture.Grpc.Contracts.Motion.Joint2D;
using Joint3D = MotionCapture.Grpc.Contracts.Motion.Joint3D;

namespace MotionCapture.Infrastructure.Grpc.Mapping;

public class MotionMappingProfile : Profile
{
    public MotionMappingProfile()
    {
        CreateMap<Joint2D, Core.Models.Joint2D>();
        CreateMap<Joint3D, Core.Models.Joint3D>();
        CreateMap<FrameLandmarks2D, Core.Models.FrameLandmarks2D>()
            .ForMember(dest => dest.Joint2D,
                opt => opt.MapFrom(src => src.Joints2D.ToList()));

        CreateMap<MotionResponse, MotionResult>()
            .ForMember(dest => dest.Joints,
                opt => opt.MapFrom(src => src.Joints.ToList()))
            .ForMember(dest => dest.FramesToDraw,
                opt => opt.MapFrom(src => src.FramesToDraw.ToList()));
    }
}
