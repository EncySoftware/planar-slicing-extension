#pragma once
#ifndef EXPORTS_H
#define EXPORTS_H

extern "C" __declspec(dllexport) void* GetCuraEngineConnectionLibPointer();
extern "C" __declspec(dllexport) void FinalizeCuraEngineConnectionLib();
#endif // EXPORTS_H