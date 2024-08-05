//Copyright (c) 2018 Ultimaker B.V.
//CuraEngine is released under the terms of the AGPLv3 or higher.

#ifndef LISTENER_H
#define LISTENER_H
#ifdef ARCUS //Extends from Arcus::SocketListener, so only compile if we're using libArcus.

#include <Arcus/SocketListener.h> //The class we're extending from.
#include "utils/CuraConnectionInterface.h"
#include <iostream>
#include <list>

namespace cura
{

/*
 * Extension of Arcus' ``SocketListener`` class to specialise the message
 * handling for CuraEngine.
 */
struct CuraSegmentPoint
{
public:
    float x;
    float y;
    float z;
    UINT8 lineType;
    float lineWidth;
    float lineThickness;
    float lineFeedrate;
};
struct CuraSegmentPoint2S : CuraSegmentPoint
{
public:
    TCE2SPoint point;
};
struct CuraSegmentPoint3S : CuraSegmentPoint
{
public:
    TCE3SPoint point;
};
struct CuraSegment
{
public:
    int32_t extruder;
    int pointType;
    std::list<CuraSegmentPoint> points;
};
struct CuraLayer
{
public:
   int32_t id;
   float height;
   float thickness;
   std::list<CuraSegment> segments;
};

class Listener : public Arcus::SocketListener
{
public:
    /*
     * Changes the ``stateChanged`` signal to do nothing.
     */
    void stateChanged(Arcus::SocketState state) override;

    /*
     * Changes the ``messageReceived`` signal to do nothing.
     */
    void messageReceived() override;

    /*
     * Log an error when we get one from libArcus.
     */
    void error(const Arcus::Error& error) override;

    int* SlicedFinished;

    ICuraEngineControlProcess* CuraEngineControlProcess = nullptr;

    ~Listener()
    {
        CuraEngineControlProcess = nullptr;
    }

private:
    std::list<CuraLayer> layers;
    static bool compareLayers(const CuraLayer& a, const CuraLayer& b)
    {
        return a.id < b.id;
    }
    void sortLayers()
    {
        layers.sort(compareLayers);
    }

};

} //namespace cura

#endif //ARCUS
#endif //LISTENER_H