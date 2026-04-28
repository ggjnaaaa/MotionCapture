using AutoMapper;
using Google.Protobuf.Collections;
using MotionCapture.Grpc.Contracts.Motion;
using ProtoCameraFrame = MotionCapture.Grpc.Contracts.Motion.CameraFrame;
using ProtoFrameLandmarks2D = MotionCapture.Grpc.Contracts.Motion.FrameLandmarks2D;
using ProtoJoint2D = MotionCapture.Grpc.Contracts.Motion.Joint2D;
using ProtoJointPosition3D = MotionCapture.Grpc.Contracts.Motion.JointPosition3D;

namespace MotionCapture.Infrastructure.GrpcClient.Mapping;

public class MotionMappingProfile : Profile
{
    public MotionMappingProfile()
    {
        CreateMap<ProtoJoint2D, MotionCapture.Core.Models.Joint2D>()
            .ForMember(d => d.X, o => o.MapFrom(s => (int)MathF.Round(s.X)))
            .ForMember(d => d.Y, o => o.MapFrom(s => (int)MathF.Round(s.Y)));

        CreateMap<ProtoJointPosition3D, MotionCapture.Core.Models.JointPosition3D>()
            .ForMember(d => d.PosX, o => o.MapFrom(s => (int)MathF.Round(s.PosX)))
            .ForMember(d => d.PosY, o => o.MapFrom(s => (int)MathF.Round(s.PosY)))
            .ForMember(d => d.PosZ, o => o.MapFrom(s => (int)MathF.Round(s.PosZ)));

        CreateMap<ProtoFrameLandmarks2D, MotionCapture.Core.Models.FrameLandmarks2D>()
            .ForMember(d => d.Joint2D, o => o.MapFrom(s => s.Joints2D.ToList()))
            .ForMember(d => d.Timestamp, o => o.MapFrom(s => s.TimestampMs));

        CreateMap<ProtoCameraFrame, MotionCapture.Core.Models.CameraFrame>()
            .ForMember(d => d.ImageBytes, o => o.MapFrom(s => s.Image.ToByteArray()))
            .ForMember(d => d.Timestamp, o => o.MapFrom(s => s.TimestampMs));

        CreateMap<MotionCapture.Core.Models.CameraFrame, ProtoCameraFrame>()
            .ForMember(d => d.Image, o => o.MapFrom(s => Google.Protobuf.ByteString.CopyFrom(s.ImageBytes)))
            .ForMember(d => d.TimestampMs, o => o.MapFrom(s => s.Timestamp));

        CreateMap<MotionResponse, MotionCapture.Core.Models.MotionResult>()
            .ForMember(d => d.Joints, o => o.MapFrom(s => s.Joints.ToList()))
            .ForMember(d => d.FramesToDraw, o => o.MapFrom(s => s.FramesToDraw.ToList()));

        CreateMap<IEnumerable<MotionCapture.Core.Models.CameraFrame>, MotionRequest>()
            .ConvertUsing<CameraBatchToMotionRequestConverter>();

        CreateMap<RepeatedField<ProtoFrameLandmarks2D>, IEnumerable<MotionCapture.Core.Models.FrameLandmarks2D>>()
            .ConvertUsing<RepeatedFrameLandmarksConverter>();
    }

    private sealed class CameraBatchToMotionRequestConverter
        : ITypeConverter<IEnumerable<MotionCapture.Core.Models.CameraFrame>, MotionRequest>
    {
        public MotionRequest Convert(
            IEnumerable<MotionCapture.Core.Models.CameraFrame> source,
            MotionRequest destination,
            ResolutionContext context)
        {
            var request = new MotionRequest();
            foreach (var frame in source)
                request.Frames.Add(context.Mapper.Map<ProtoCameraFrame>(frame));
            return request;
        }
    }

    private sealed class RepeatedFrameLandmarksConverter
        : ITypeConverter<RepeatedField<ProtoFrameLandmarks2D>, IEnumerable<MotionCapture.Core.Models.FrameLandmarks2D>>
    {
        public IEnumerable<MotionCapture.Core.Models.FrameLandmarks2D> Convert(
            RepeatedField<ProtoFrameLandmarks2D> source,
            IEnumerable<MotionCapture.Core.Models.FrameLandmarks2D> destination,
            ResolutionContext context) =>
            source.Select(x => context.Mapper.Map<MotionCapture.Core.Models.FrameLandmarks2D>(x)).ToList();
    }
}
