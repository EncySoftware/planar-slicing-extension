// Copyright (c) 2022 Ultimaker B.V.
// CuraEngine is released under the terms of the AGPLv3 or higher

#ifdef ARCUS

#include <Arcus/Error.h> //To process error codes.
#include <spdlog/spdlog.h>
#include "communication/ArcusCommunication.h"
#include "communication/Listener.h"
#include <Arcus/Socket.h> //The socket to communicate to.
#include <iostream>
#include <fstream>
#include <comutil.h>

namespace cura
{

std::string SocketStateStr(Arcus::SocketState state)
{
    switch (state)
    {
    case Arcus::SocketState::Closed :
        return "Closed";
    case Arcus::SocketState::Closing:
        return "Closing";
    case Arcus::SocketState::Connected:
        return "Connected";
    case Arcus::SocketState::Connecting:
        return "Connecting";
    case Arcus::SocketState::Error:
        return "Error";
    case Arcus::SocketState::Initial:
        return "Initial";
    case Arcus::SocketState::Listening:
        return "Listening";
    case Arcus::SocketState::Opening:
        return "Opening";
    default:
        return "unknow";
    }
}

void Listener::stateChanged(Arcus::SocketState state)
{
    if (state == Arcus::SocketState::Closed || state == Arcus::SocketState::Closing)
    {
        *SlicedFinished = -1;
    }
    else
    {
        *SlicedFinished = static_cast<int>(state);
    }
    auto logMsg = "Socket state changed: " + SocketStateStr(state);
    CuraEngineControlProcess->OnLogMessage(2, _com_util::ConvertStringToBSTR(logMsg.c_str()));
    spdlog::info(logMsg);
}

void Listener::messageReceived()
{
    auto msg = this->getSocket()->takeNextMessage();
    auto TypeName = msg->GetTypeName();
    auto des = msg->GetDescriptor();
    auto ind = des->index();
    auto logMsg = std::to_string(ind);
    //CuraEngineControlProcess->OnLogMessage(2, _com_util::ConvertStringToBSTR(logMsg.c_str()));
    switch (ind)
    {
        case 5: // Progress
        {
            auto ProgressMsg = dynamic_cast<proto::Progress*>(msg.get());
            if (ProgressMsg != NULL && CuraEngineControlProcess != nullptr)
            {
                auto Amount = ProgressMsg->amount();
                CuraEngineControlProcess->OnProgress(Amount);
            }
            break;
        }
        case 8: // LayerOptimized
        {
            auto LayerOptimizedMsg = dynamic_cast<proto::LayerOptimized*>(msg.get());
            if (LayerOptimizedMsg != NULL && CuraEngineControlProcess != nullptr)
            {
                auto id = LayerOptimizedMsg->id();
                auto height = LayerOptimizedMsg->height()/1000;
                auto thickness = LayerOptimizedMsg->thickness()/1000;
                auto segmentsCount = LayerOptimizedMsg->path_segment_size();

                CuraLayer l;
                l.id = id;
                l.height = height;
                l.thickness = thickness;
                for (int i = 0; i < segmentsCount; i++)
                {
                    auto segment = LayerOptimizedMsg->path_segment(i);
                    auto extruder = segment.extruder();
                    auto point_type = static_cast<int>(segment.point_type());
                    auto points = segment.points();
                    auto line_type = segment.line_type();
                    auto line_width = segment.line_width();
                    auto line_thickness = segment.line_thickness();
                    auto line_feedrate = segment.line_feedrate();

                    CuraSegment seg;
                    seg.extruder = extruder;
                    seg.pointType = point_type;

                    UINT8* lineType = reinterpret_cast<UINT8*>(&line_type[0]);

                    float* lineWidth = reinterpret_cast<float*>(&line_width[0]);

                    float* lineThickness = reinterpret_cast<float*>(&line_thickness[0]);

                    float* lineFeedrate = reinterpret_cast<float*>(&line_feedrate[0]);

                    
                    auto byteCount = line_type.size();
                    auto Count = byteCount / sizeof(UINT8);
                    Count++;
                    if (Count > 0)
                    {
                        TCE2SPoint* points2S;
                        TCE3SPoint* points3S;
                        CuraSegmentPoint firstPoint;
                        if (point_type == 0)
                        {
                            points2S = reinterpret_cast<TCE2SPoint*>(&points[0]);
                            firstPoint.x = points2S[0].X;
                            firstPoint.y = points2S[0].Y;
                        }
                        if (point_type == 1)
                        {
                            points3S = reinterpret_cast<TCE3SPoint*>(&points[0]);
                            firstPoint.x = points3S[0].X;
                            firstPoint.y = points3S[0].Y;
                            firstPoint.z = points3S[0].Z;
                        }
                        firstPoint.lineFeedrate = lineFeedrate[0];
                        firstPoint.lineThickness = lineThickness[0];
                        firstPoint.lineType = lineType[0];
                        firstPoint.lineWidth = lineWidth[0];
                        seg.points.push_back(firstPoint);
                        for (int j = 1; j < Count; j++)
                        {
                            CuraSegmentPoint p;
                            if (point_type == 0)
                            {
                                p.x = points2S[j].X;
                                p.y = points2S[j].Y;
                            }
                            if (point_type == 1)
                            {
                                p.x = points3S[j].X;
                                p.y = points3S[j].Y;
                                p.z = points3S[j].Z;
                            }
                            p.lineFeedrate = lineFeedrate[j - 1];
                            p.lineThickness = lineThickness[j - 1];
                            p.lineType = lineType[j - 1];
                            p.lineWidth = lineWidth[j - 1];
                            seg.points.push_back(p);
                        }
                    }
                    
                    l.segments.push_back(seg);
                }
                layers.push_back(l);
            }
            break;
        }
        case 10: // GCodeLayer
        {
            auto GCodeLayerMsg = dynamic_cast<proto::GCodeLayer*>(msg.get());
            if (GCodeLayerMsg != NULL && CuraEngineControlProcess != nullptr)
            {
                auto GCodeData = GCodeLayerMsg->data();
                CuraEngineControlProcess->OnGCode(_com_util::ConvertStringToBSTR(GCodeData.c_str()));
            }
            break;
        }
        case 11: // PrintTimeMaterialEstimates
        {
            auto PrintTimeMaterialEstimatesMsg = dynamic_cast<proto::PrintTimeMaterialEstimates*>(msg.get());
            if (PrintTimeMaterialEstimatesMsg != NULL && CuraEngineControlProcess != nullptr)
            {
                auto time_none_val = PrintTimeMaterialEstimatesMsg->time_none();
                CuraEngineControlProcess->OnPrintTimeMaterialEstimates(
                    _com_util::ConvertStringToBSTR("time_none"), time_none_val);
                auto time_inset_0_val = PrintTimeMaterialEstimatesMsg->time_inset_0();
                CuraEngineControlProcess->OnPrintTimeMaterialEstimates(
                    _com_util::ConvertStringToBSTR("ime_inset_0"), time_inset_0_val);
                auto time_inset_x_val = PrintTimeMaterialEstimatesMsg->time_inset_x();
                CuraEngineControlProcess->OnPrintTimeMaterialEstimates(
                    _com_util::ConvertStringToBSTR("time_inset_x"), time_inset_x_val);
                auto time_skin_val = PrintTimeMaterialEstimatesMsg->time_skin();
                CuraEngineControlProcess->OnPrintTimeMaterialEstimates(
                    _com_util::ConvertStringToBSTR("time_skin"), time_skin_val);
                auto time_support_val = PrintTimeMaterialEstimatesMsg->time_support();
                CuraEngineControlProcess->OnPrintTimeMaterialEstimates(
                    _com_util::ConvertStringToBSTR("time_support"), time_support_val);
                auto time_skirt_val = PrintTimeMaterialEstimatesMsg->time_skirt();
                CuraEngineControlProcess->OnPrintTimeMaterialEstimates(
                    _com_util::ConvertStringToBSTR("time_skirt"), time_skirt_val);
                auto time_infill_val = PrintTimeMaterialEstimatesMsg->time_infill();
                CuraEngineControlProcess->OnPrintTimeMaterialEstimates(
                    _com_util::ConvertStringToBSTR("time_infill"), time_infill_val);
                auto time_support_infill_val = PrintTimeMaterialEstimatesMsg->time_support_infill();
                CuraEngineControlProcess->OnPrintTimeMaterialEstimates(
                    _com_util::ConvertStringToBSTR("time_support_infill"), time_support_infill_val);
                auto time_travel_val = PrintTimeMaterialEstimatesMsg->time_travel();
                CuraEngineControlProcess->OnPrintTimeMaterialEstimates(
                    _com_util::ConvertStringToBSTR("time_travel"), time_travel_val);
                auto time_retract_val = PrintTimeMaterialEstimatesMsg->time_retract();
                CuraEngineControlProcess->OnPrintTimeMaterialEstimates(
                    _com_util::ConvertStringToBSTR("time_retract"), time_retract_val);
                auto time_support_interface_val = PrintTimeMaterialEstimatesMsg->time_support_interface();
                CuraEngineControlProcess->OnPrintTimeMaterialEstimates(
                    _com_util::ConvertStringToBSTR("time_support_interface"), time_support_interface_val);
                auto time_prime_tower_val = PrintTimeMaterialEstimatesMsg->time_prime_tower();
                CuraEngineControlProcess->OnPrintTimeMaterialEstimates(
                    _com_util::ConvertStringToBSTR("time_prime_tower"), time_prime_tower_val);
                auto MaterialEstimatesMsg = PrintTimeMaterialEstimatesMsg->materialestimates();
                for (int i = 0; i < MaterialEstimatesMsg.size(); i++)
                {
                    auto MatEst = MaterialEstimatesMsg.Get(i);
                    std::wstring id = std::to_wstring(MatEst.id());
                    CuraEngineControlProcess->OnPrintTimeMaterialEstimates(
                        SysAllocString(id.c_str()), MatEst.material_amount());
                }
            }
            break;
        }
        case 16: // GCodePrefix
        {
            auto GCodePrefixMsg = dynamic_cast<proto::GCodePrefix*>(msg.get());
            if (GCodePrefixMsg != NULL && CuraEngineControlProcess != nullptr)
            {
                auto GCodePrefix = GCodePrefixMsg->data();
                CuraEngineControlProcess->OnGCodePrefix(_com_util::ConvertStringToBSTR(GCodePrefix.c_str()));
            }
            break;
        }
        case 17: // SliceUUID
        {
            auto SliceUUIDMsg = dynamic_cast<proto::SliceUUID*>(msg.get());
            if (SliceUUIDMsg != NULL && CuraEngineControlProcess != nullptr)
            {
                auto SliceUUID = SliceUUIDMsg->slice_uuid();
                CuraEngineControlProcess->OnSliceUUID(_com_util::ConvertStringToBSTR(SliceUUID.c_str()));
            }
            break;
        }
        case 18: // SlicingFinished
        {
            if (!layers.empty())
            {
                sortLayers();
                for (const auto& l : layers)
                {
                    CuraEngineControlProcess->OnStartLayersOptimized(l.id, l.height, l.thickness);
                    for (const auto& seg : l.segments)
                    {
                        CuraEngineControlProcess->OnStartPathSegment(seg.extruder, seg.pointType);
                        for (const auto& p : seg.points)
                        {
                            if (seg.pointType == 0)
                            {
                                TCE2SPoint p2s;
                                p2s.X = p.x;
                                p2s.Y = p.y;
                                CuraEngineControlProcess->Add2SPoint(p2s, p.lineType, p.lineWidth, p.lineThickness, p.lineFeedrate);
                            }
                            if (seg.pointType == 1)
                            {
                                TCE3SPoint p3s;
                                p3s.X = p.x;
                                p3s.Y = p.y;
                                p3s.Z = p.z;
                                CuraEngineControlProcess->Add3SPoint(p3s, p.lineType, p.lineWidth, p.lineThickness, p.lineFeedrate);

                            }
                        }
                        CuraEngineControlProcess->OnStopPathSegment();
                    }
                    CuraEngineControlProcess->OnStopLayersOptimized();
                }
            }
            
            auto msg = "Slicing Finished";
            *SlicedFinished = -1;
            CuraEngineControlProcess->OnFinish(_com_util::ConvertStringToBSTR("Finished"));
            break;
        }
        default:
        {
            break;
        }
    }
}
void Listener::error(const Arcus::Error& error)
{
    if (error.getErrorCode() == Arcus::ErrorCode::Debug)
    {
        auto logMsg = "Socket debug: " + error.getErrorMessage();
        CuraEngineControlProcess->OnLogMessage(2, _com_util::ConvertStringToBSTR(logMsg.c_str()));
        spdlog::debug("{}", error.getErrorMessage());
    }
    else
    {
        auto logMsg = "Socket error: " + error.getErrorMessage();
        CuraEngineControlProcess->OnLogMessage(5, _com_util::ConvertStringToBSTR(logMsg.c_str()));
        spdlog::error("{}", error.getErrorMessage());
    }
    *SlicedFinished = -1;
}

} // namespace cura

#endif // ARCUS