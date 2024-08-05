// Copyright (c) 2022 Ultimaker B.V.
// CuraEngine is released under the terms of the AGPLv3 or higher

#ifdef ARCUS

#include "communication/ArcusCommunicationPrivate.h"

namespace cura
{

ArcusCommunication::Private::Private()
    : socket(nullptr)
    , object_count(0)
    , last_sent_progress(-1)
    , slice_count(0)
    , millisecUntilNextTry(100)
{
}

} // namespace cura

#endif // ARCUS