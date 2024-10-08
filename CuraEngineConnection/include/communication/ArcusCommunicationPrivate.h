// Copyright (c) 2018 Ultimaker B.V.
// CuraEngine is released under the terms of the AGPLv3 or higher.

#ifndef ARCUSCOMMUNICATIONPRIVATE_H
#define ARCUSCOMMUNICATIONPRIVATE_H
#ifdef ARCUS

#include "ArcusCommunication.h" //We're adding a subclass to this.

#include <sstream> //For ostringstream.

namespace cura
{


class ArcusCommunication::Private
{
    friend class ArcusCommunicationPrivateTest;

public:
    Private();

    Arcus::Socket* socket; //!< Socket to send data to.
    size_t object_count; //!< Number of objects that need to be sliced.
    std::string temp_gcode_file; //!< Temporary buffer for the g-code.
    std::ostringstream gcode_output_stream; //!< The stream to write g-code to.

    int last_sent_progress; //!< Last sent progress promille (1/1000th). Used to not send duplicate messages with the same promille.

    /*
     * \brief How often we've sliced so far during this run of CuraEngine.
     *
     * This is currently used to limit the number of slices per run to 1,
     * because CuraEngine produced different output for each slice. The fix was
     * to restart CuraEngine every time you make a slice.
     *
     * Once this bug is resolved, we can allow multiple slices for each run. Our
     * intuition says that there might be some differences if we let stuff
     * depend on the order of iteration in unordered_map or unordered_set,
     * because those data structures will give a different order if more memory
     * has already been reserved for them.
     */
    size_t slice_count; //!< How often we've sliced so far during this run of CuraEngine.

    const size_t millisecUntilNextTry; // How long we wait until we try to connect again.
};

} // namespace cura

#endif // ARCUS
#endif // ARCUSCOMMUNICATIONPRIVATE_H