

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 6.00.0366 */
/* at Thu Aug 01 17:31:45 2024
 */
/* Compiler settings
    Oicf, W1, Zp8, env=Win32 (32b run)
    protocol : dce , ms_ext, c_ext, robust
    error checks: allocation ref bounds_check enum stub_data 
    VC __declspec() decoration level: 
         __declspec(uuid()), __declspec(selectany), __declspec(novtable)
         DECLSPEC_UUID(), MIDL_INTERFACE()
*/
//@@MIDL_FILE_HEADING(  )

#pragma warning( disable: 4049 )  /* more than 64k source lines */


/* verify that the <rpcndr.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCNDR_H_VERSION__
#define __REQUIRED_RPCNDR_H_VERSION__ 475
#endif

#include "rpc.h"
#include "rpcndr.h"

#ifndef __RPCNDR_H_VERSION__
#error this stub requires an updated version of <rpcndr.h>
#endif // __RPCNDR_H_VERSION__


#ifndef __CuraConnectionInterface_h__
#define __CuraConnectionInterface_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

/* Forward Declarations */ 

#ifndef __ICuraEngineControlProcess_FWD_DEFINED__
#define __ICuraEngineControlProcess_FWD_DEFINED__
typedef interface ICuraEngineControlProcess ICuraEngineControlProcess;
#endif 	/* __ICuraEngineControlProcess_FWD_DEFINED__ */


#ifndef __ITrianglesReciever_FWD_DEFINED__
#define __ITrianglesReciever_FWD_DEFINED__
typedef interface ITrianglesReciever ITrianglesReciever;
#endif 	/* __ITrianglesReciever_FWD_DEFINED__ */


#ifndef __IParamsReceiver_FWD_DEFINED__
#define __IParamsReceiver_FWD_DEFINED__
typedef interface IParamsReceiver IParamsReceiver;
#endif 	/* __IParamsReceiver_FWD_DEFINED__ */


#ifndef __ICuraConnectionLibrary_FWD_DEFINED__
#define __ICuraConnectionLibrary_FWD_DEFINED__
typedef interface ICuraConnectionLibrary ICuraConnectionLibrary;
#endif 	/* __ICuraConnectionLibrary_FWD_DEFINED__ */


#ifdef __cplusplus
extern "C"{
#endif 

void * __RPC_USER MIDL_user_allocate(size_t);
void __RPC_USER MIDL_user_free( void * ); 

/* interface __MIDL_itf_CuraConnectionInterface_0000 */
/* [local] */ 


#pragma pack(push, 1)


extern RPC_IF_HANDLE __MIDL_itf_CuraConnectionInterface_0000_v0_0_c_ifspec;
extern RPC_IF_HANDLE __MIDL_itf_CuraConnectionInterface_0000_v0_0_s_ifspec;


#ifndef __CuraConnectionInterface_LIBRARY_DEFINED__
#define __CuraConnectionInterface_LIBRARY_DEFINED__

/* library CuraConnectionInterface */
/* [helpstring][version][uuid] */ 

typedef /* [version] */ struct tagTCE3SPoint
    {
    float X;
    float Y;
    float Z;
    } 	TCE3SPoint;

typedef /* [version] */ struct tagTCE2SPoint
    {
    float X;
    float Y;
    } 	TCE2SPoint;


EXTERN_C const IID LIBID_CuraConnectionInterface;

#ifndef __ICuraEngineControlProcess_INTERFACE_DEFINED__
#define __ICuraEngineControlProcess_INTERFACE_DEFINED__

/* interface ICuraEngineControlProcess */
/* [object][version][uuid][helpstring] */ 


EXTERN_C const IID IID_ICuraEngineControlProcess;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("53DD0B9D-A10F-4A95-9A91-7EE5F1EF6512")
    ICuraEngineControlProcess : public IUnknown
    {
    public:
        virtual /* [helpstring] */ HRESULT __stdcall OnError( 
            /* [in] */ BSTR msg) = 0;
        
        virtual /* [helpstring] */ HRESULT __stdcall OnFinish( 
            /* [in] */ BSTR msg) = 0;
        
        virtual /* [helpstring] */ HRESULT __stdcall OnPrintTimeMaterialEstimates( 
            /* [in] */ BSTR name,
            /* [in] */ double value) = 0;
        
        virtual /* [helpstring] */ HRESULT __stdcall OnGCode( 
            /* [in] */ BSTR GCode) = 0;
        
        virtual /* [helpstring] */ HRESULT __stdcall OnProgress( 
            /* [in] */ double progress) = 0;
        
        virtual HRESULT __stdcall OnStartLayersOptimized( 
            /* [in] */ long id,
            /* [in] */ double height,
            /* [in] */ double thickness) = 0;
        
        virtual HRESULT __stdcall OnStopLayersOptimized( void) = 0;
        
        virtual HRESULT __stdcall OnStartPathSegment( 
            /* [in] */ long extruderID,
            /* [in] */ long point_type) = 0;
        
        virtual HRESULT __stdcall OnStopPathSegment( void) = 0;
        
        virtual HRESULT __stdcall Add2SPoint( 
            /* [in] */ TCE2SPoint p,
            /* [in] */ long lineType,
            /* [in] */ float lineWidth,
            /* [in] */ float lineThickness,
            /* [in] */ float lineFeedrate) = 0;
        
        virtual HRESULT __stdcall Add3SPoint( 
            /* [in] */ TCE3SPoint p,
            /* [in] */ long lineType,
            /* [in] */ float lineWidth,
            /* [in] */ float lineThickness,
            /* [in] */ float lineFeedrate) = 0;
        
        virtual /* [helpstring] */ HRESULT __stdcall OnGCodePrefix( 
            /* [in] */ BSTR GCodePrefix) = 0;
        
        virtual /* [helpstring] */ HRESULT __stdcall OnSliceUUID( 
            /* [in] */ BSTR SliceUUID) = 0;
        
        virtual HRESULT __stdcall OnLogMessage( 
            /* [in] */ long messageType,
            /* [in] */ BSTR messageString) = 0;
        
        virtual HRESULT __stdcall IsProcessCancelled( 
            /* [retval][out] */ VARIANT_BOOL *result) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct ICuraEngineControlProcessVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ICuraEngineControlProcess * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ICuraEngineControlProcess * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ICuraEngineControlProcess * This);
        
        /* [helpstring] */ HRESULT ( __stdcall *OnError )( 
            ICuraEngineControlProcess * This,
            /* [in] */ BSTR msg);
        
        /* [helpstring] */ HRESULT ( __stdcall *OnFinish )( 
            ICuraEngineControlProcess * This,
            /* [in] */ BSTR msg);
        
        /* [helpstring] */ HRESULT ( __stdcall *OnPrintTimeMaterialEstimates )( 
            ICuraEngineControlProcess * This,
            /* [in] */ BSTR name,
            /* [in] */ double value);
        
        /* [helpstring] */ HRESULT ( __stdcall *OnGCode )( 
            ICuraEngineControlProcess * This,
            /* [in] */ BSTR GCode);
        
        /* [helpstring] */ HRESULT ( __stdcall *OnProgress )( 
            ICuraEngineControlProcess * This,
            /* [in] */ double progress);
        
        HRESULT ( __stdcall *OnStartLayersOptimized )( 
            ICuraEngineControlProcess * This,
            /* [in] */ long id,
            /* [in] */ double height,
            /* [in] */ double thickness);
        
        HRESULT ( __stdcall *OnStopLayersOptimized )( 
            ICuraEngineControlProcess * This);
        
        HRESULT ( __stdcall *OnStartPathSegment )( 
            ICuraEngineControlProcess * This,
            /* [in] */ long extruderID,
            /* [in] */ long point_type);
        
        HRESULT ( __stdcall *OnStopPathSegment )( 
            ICuraEngineControlProcess * This);
        
        HRESULT ( __stdcall *Add2SPoint )( 
            ICuraEngineControlProcess * This,
            /* [in] */ TCE2SPoint p,
            /* [in] */ long lineType,
            /* [in] */ float lineWidth,
            /* [in] */ float lineThickness,
            /* [in] */ float lineFeedrate);
        
        HRESULT ( __stdcall *Add3SPoint )( 
            ICuraEngineControlProcess * This,
            /* [in] */ TCE3SPoint p,
            /* [in] */ long lineType,
            /* [in] */ float lineWidth,
            /* [in] */ float lineThickness,
            /* [in] */ float lineFeedrate);
        
        /* [helpstring] */ HRESULT ( __stdcall *OnGCodePrefix )( 
            ICuraEngineControlProcess * This,
            /* [in] */ BSTR GCodePrefix);
        
        /* [helpstring] */ HRESULT ( __stdcall *OnSliceUUID )( 
            ICuraEngineControlProcess * This,
            /* [in] */ BSTR SliceUUID);
        
        HRESULT ( __stdcall *OnLogMessage )( 
            ICuraEngineControlProcess * This,
            /* [in] */ long messageType,
            /* [in] */ BSTR messageString);
        
        HRESULT ( __stdcall *IsProcessCancelled )( 
            ICuraEngineControlProcess * This,
            /* [retval][out] */ VARIANT_BOOL *result);
        
        END_INTERFACE
    } ICuraEngineControlProcessVtbl;

    interface ICuraEngineControlProcess
    {
        CONST_VTBL struct ICuraEngineControlProcessVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ICuraEngineControlProcess_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define ICuraEngineControlProcess_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define ICuraEngineControlProcess_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define ICuraEngineControlProcess_OnError(This,msg)	\
    (This)->lpVtbl -> OnError(This,msg)

#define ICuraEngineControlProcess_OnFinish(This,msg)	\
    (This)->lpVtbl -> OnFinish(This,msg)

#define ICuraEngineControlProcess_OnPrintTimeMaterialEstimates(This,name,value)	\
    (This)->lpVtbl -> OnPrintTimeMaterialEstimates(This,name,value)

#define ICuraEngineControlProcess_OnGCode(This,GCode)	\
    (This)->lpVtbl -> OnGCode(This,GCode)

#define ICuraEngineControlProcess_OnProgress(This,progress)	\
    (This)->lpVtbl -> OnProgress(This,progress)

#define ICuraEngineControlProcess_OnStartLayersOptimized(This,id,height,thickness)	\
    (This)->lpVtbl -> OnStartLayersOptimized(This,id,height,thickness)

#define ICuraEngineControlProcess_OnStopLayersOptimized(This)	\
    (This)->lpVtbl -> OnStopLayersOptimized(This)

#define ICuraEngineControlProcess_OnStartPathSegment(This,extruderID,point_type)	\
    (This)->lpVtbl -> OnStartPathSegment(This,extruderID,point_type)

#define ICuraEngineControlProcess_OnStopPathSegment(This)	\
    (This)->lpVtbl -> OnStopPathSegment(This)

#define ICuraEngineControlProcess_Add2SPoint(This,p,lineType,lineWidth,lineThickness,lineFeedrate)	\
    (This)->lpVtbl -> Add2SPoint(This,p,lineType,lineWidth,lineThickness,lineFeedrate)

#define ICuraEngineControlProcess_Add3SPoint(This,p,lineType,lineWidth,lineThickness,lineFeedrate)	\
    (This)->lpVtbl -> Add3SPoint(This,p,lineType,lineWidth,lineThickness,lineFeedrate)

#define ICuraEngineControlProcess_OnGCodePrefix(This,GCodePrefix)	\
    (This)->lpVtbl -> OnGCodePrefix(This,GCodePrefix)

#define ICuraEngineControlProcess_OnSliceUUID(This,SliceUUID)	\
    (This)->lpVtbl -> OnSliceUUID(This,SliceUUID)

#define ICuraEngineControlProcess_OnLogMessage(This,messageType,messageString)	\
    (This)->lpVtbl -> OnLogMessage(This,messageType,messageString)

#define ICuraEngineControlProcess_IsProcessCancelled(This,result)	\
    (This)->lpVtbl -> IsProcessCancelled(This,result)

#endif /* COBJMACROS */


#endif 	/* C style interface */



/* [helpstring] */ HRESULT __stdcall ICuraEngineControlProcess_OnError_Proxy( 
    ICuraEngineControlProcess * This,
    /* [in] */ BSTR msg);


void __RPC_STUB ICuraEngineControlProcess_OnError_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring] */ HRESULT __stdcall ICuraEngineControlProcess_OnFinish_Proxy( 
    ICuraEngineControlProcess * This,
    /* [in] */ BSTR msg);


void __RPC_STUB ICuraEngineControlProcess_OnFinish_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring] */ HRESULT __stdcall ICuraEngineControlProcess_OnPrintTimeMaterialEstimates_Proxy( 
    ICuraEngineControlProcess * This,
    /* [in] */ BSTR name,
    /* [in] */ double value);


void __RPC_STUB ICuraEngineControlProcess_OnPrintTimeMaterialEstimates_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring] */ HRESULT __stdcall ICuraEngineControlProcess_OnGCode_Proxy( 
    ICuraEngineControlProcess * This,
    /* [in] */ BSTR GCode);


void __RPC_STUB ICuraEngineControlProcess_OnGCode_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring] */ HRESULT __stdcall ICuraEngineControlProcess_OnProgress_Proxy( 
    ICuraEngineControlProcess * This,
    /* [in] */ double progress);


void __RPC_STUB ICuraEngineControlProcess_OnProgress_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT __stdcall ICuraEngineControlProcess_OnStartLayersOptimized_Proxy( 
    ICuraEngineControlProcess * This,
    /* [in] */ long id,
    /* [in] */ double height,
    /* [in] */ double thickness);


void __RPC_STUB ICuraEngineControlProcess_OnStartLayersOptimized_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT __stdcall ICuraEngineControlProcess_OnStopLayersOptimized_Proxy( 
    ICuraEngineControlProcess * This);


void __RPC_STUB ICuraEngineControlProcess_OnStopLayersOptimized_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT __stdcall ICuraEngineControlProcess_OnStartPathSegment_Proxy( 
    ICuraEngineControlProcess * This,
    /* [in] */ long extruderID,
    /* [in] */ long point_type);


void __RPC_STUB ICuraEngineControlProcess_OnStartPathSegment_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT __stdcall ICuraEngineControlProcess_OnStopPathSegment_Proxy( 
    ICuraEngineControlProcess * This);


void __RPC_STUB ICuraEngineControlProcess_OnStopPathSegment_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT __stdcall ICuraEngineControlProcess_Add2SPoint_Proxy( 
    ICuraEngineControlProcess * This,
    /* [in] */ TCE2SPoint p,
    /* [in] */ long lineType,
    /* [in] */ float lineWidth,
    /* [in] */ float lineThickness,
    /* [in] */ float lineFeedrate);


void __RPC_STUB ICuraEngineControlProcess_Add2SPoint_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT __stdcall ICuraEngineControlProcess_Add3SPoint_Proxy( 
    ICuraEngineControlProcess * This,
    /* [in] */ TCE3SPoint p,
    /* [in] */ long lineType,
    /* [in] */ float lineWidth,
    /* [in] */ float lineThickness,
    /* [in] */ float lineFeedrate);


void __RPC_STUB ICuraEngineControlProcess_Add3SPoint_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring] */ HRESULT __stdcall ICuraEngineControlProcess_OnGCodePrefix_Proxy( 
    ICuraEngineControlProcess * This,
    /* [in] */ BSTR GCodePrefix);


void __RPC_STUB ICuraEngineControlProcess_OnGCodePrefix_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


/* [helpstring] */ HRESULT __stdcall ICuraEngineControlProcess_OnSliceUUID_Proxy( 
    ICuraEngineControlProcess * This,
    /* [in] */ BSTR SliceUUID);


void __RPC_STUB ICuraEngineControlProcess_OnSliceUUID_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT __stdcall ICuraEngineControlProcess_OnLogMessage_Proxy( 
    ICuraEngineControlProcess * This,
    /* [in] */ long messageType,
    /* [in] */ BSTR messageString);


void __RPC_STUB ICuraEngineControlProcess_OnLogMessage_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT __stdcall ICuraEngineControlProcess_IsProcessCancelled_Proxy( 
    ICuraEngineControlProcess * This,
    /* [retval][out] */ VARIANT_BOOL *result);


void __RPC_STUB ICuraEngineControlProcess_IsProcessCancelled_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __ICuraEngineControlProcess_INTERFACE_DEFINED__ */


#ifndef __ITrianglesReciever_INTERFACE_DEFINED__
#define __ITrianglesReciever_INTERFACE_DEFINED__

/* interface ITrianglesReciever */
/* [object][version][uuid][helpstring] */ 


EXTERN_C const IID IID_ITrianglesReciever;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("2BF945B3-1166-4152-B9B9-75C5D588CA8E")
    ITrianglesReciever : public IUnknown
    {
    public:
        virtual HRESULT __stdcall BeginTransfer( void) = 0;
        
        virtual HRESULT __stdcall EndTransfer( void) = 0;
        
        virtual HRESULT __stdcall BeginModel( void) = 0;
        
        virtual HRESULT __stdcall EndModel( void) = 0;
        
        virtual HRESULT __stdcall BeginMesh( 
            /* [in] */ BSTR meshName,
            /* [in] */ long triangleCount) = 0;
        
        virtual HRESULT __stdcall EndMesh( void) = 0;
        
        virtual HRESULT __stdcall AddTriangle( 
            /* [in] */ TCE3SPoint p1,
            /* [in] */ TCE3SPoint p2,
            /* [in] */ TCE3SPoint p3) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct ITrianglesRecieverVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ITrianglesReciever * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ITrianglesReciever * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ITrianglesReciever * This);
        
        HRESULT ( __stdcall *BeginTransfer )( 
            ITrianglesReciever * This);
        
        HRESULT ( __stdcall *EndTransfer )( 
            ITrianglesReciever * This);
        
        HRESULT ( __stdcall *BeginModel )( 
            ITrianglesReciever * This);
        
        HRESULT ( __stdcall *EndModel )( 
            ITrianglesReciever * This);
        
        HRESULT ( __stdcall *BeginMesh )( 
            ITrianglesReciever * This,
            /* [in] */ BSTR meshName,
            /* [in] */ long triangleCount);
        
        HRESULT ( __stdcall *EndMesh )( 
            ITrianglesReciever * This);
        
        HRESULT ( __stdcall *AddTriangle )( 
            ITrianglesReciever * This,
            /* [in] */ TCE3SPoint p1,
            /* [in] */ TCE3SPoint p2,
            /* [in] */ TCE3SPoint p3);
        
        END_INTERFACE
    } ITrianglesRecieverVtbl;

    interface ITrianglesReciever
    {
        CONST_VTBL struct ITrianglesRecieverVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ITrianglesReciever_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define ITrianglesReciever_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define ITrianglesReciever_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define ITrianglesReciever_BeginTransfer(This)	\
    (This)->lpVtbl -> BeginTransfer(This)

#define ITrianglesReciever_EndTransfer(This)	\
    (This)->lpVtbl -> EndTransfer(This)

#define ITrianglesReciever_BeginModel(This)	\
    (This)->lpVtbl -> BeginModel(This)

#define ITrianglesReciever_EndModel(This)	\
    (This)->lpVtbl -> EndModel(This)

#define ITrianglesReciever_BeginMesh(This,meshName,triangleCount)	\
    (This)->lpVtbl -> BeginMesh(This,meshName,triangleCount)

#define ITrianglesReciever_EndMesh(This)	\
    (This)->lpVtbl -> EndMesh(This)

#define ITrianglesReciever_AddTriangle(This,p1,p2,p3)	\
    (This)->lpVtbl -> AddTriangle(This,p1,p2,p3)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT __stdcall ITrianglesReciever_BeginTransfer_Proxy( 
    ITrianglesReciever * This);


void __RPC_STUB ITrianglesReciever_BeginTransfer_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT __stdcall ITrianglesReciever_EndTransfer_Proxy( 
    ITrianglesReciever * This);


void __RPC_STUB ITrianglesReciever_EndTransfer_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT __stdcall ITrianglesReciever_BeginModel_Proxy( 
    ITrianglesReciever * This);


void __RPC_STUB ITrianglesReciever_BeginModel_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT __stdcall ITrianglesReciever_EndModel_Proxy( 
    ITrianglesReciever * This);


void __RPC_STUB ITrianglesReciever_EndModel_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT __stdcall ITrianglesReciever_BeginMesh_Proxy( 
    ITrianglesReciever * This,
    /* [in] */ BSTR meshName,
    /* [in] */ long triangleCount);


void __RPC_STUB ITrianglesReciever_BeginMesh_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT __stdcall ITrianglesReciever_EndMesh_Proxy( 
    ITrianglesReciever * This);


void __RPC_STUB ITrianglesReciever_EndMesh_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT __stdcall ITrianglesReciever_AddTriangle_Proxy( 
    ITrianglesReciever * This,
    /* [in] */ TCE3SPoint p1,
    /* [in] */ TCE3SPoint p2,
    /* [in] */ TCE3SPoint p3);


void __RPC_STUB ITrianglesReciever_AddTriangle_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __ITrianglesReciever_INTERFACE_DEFINED__ */


#ifndef __IParamsReceiver_INTERFACE_DEFINED__
#define __IParamsReceiver_INTERFACE_DEFINED__

/* interface IParamsReceiver */
/* [object][version][uuid][helpstring] */ 


EXTERN_C const IID IID_IParamsReceiver;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("B82C95B6-9015-4AAC-8CCB-FA92525C9C0D")
    IParamsReceiver : public IUnknown
    {
    public:
        virtual HRESULT __stdcall GlobalParamsSize( 
            /* [retval][out] */ long *result) = 0;
        
        virtual HRESULT __stdcall GlobalParamName( 
            /* [in] */ long index,
            /* [retval][out] */ BSTR *result) = 0;
        
        virtual HRESULT __stdcall GlobalParamValue( 
            /* [in] */ long index,
            /* [retval][out] */ BSTR *result) = 0;
        
        virtual HRESULT __stdcall ExtruderParamsSize( 
            /* [retval][out] */ long *result) = 0;
        
        virtual HRESULT __stdcall ExtruderParamName( 
            /* [in] */ long index,
            /* [retval][out] */ BSTR *result) = 0;
        
        virtual HRESULT __stdcall ExtruderParamValue( 
            /* [in] */ long index,
            /* [retval][out] */ BSTR *result) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct IParamsReceiverVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IParamsReceiver * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IParamsReceiver * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IParamsReceiver * This);
        
        HRESULT ( __stdcall *GlobalParamsSize )( 
            IParamsReceiver * This,
            /* [retval][out] */ long *result);
        
        HRESULT ( __stdcall *GlobalParamName )( 
            IParamsReceiver * This,
            /* [in] */ long index,
            /* [retval][out] */ BSTR *result);
        
        HRESULT ( __stdcall *GlobalParamValue )( 
            IParamsReceiver * This,
            /* [in] */ long index,
            /* [retval][out] */ BSTR *result);
        
        HRESULT ( __stdcall *ExtruderParamsSize )( 
            IParamsReceiver * This,
            /* [retval][out] */ long *result);
        
        HRESULT ( __stdcall *ExtruderParamName )( 
            IParamsReceiver * This,
            /* [in] */ long index,
            /* [retval][out] */ BSTR *result);
        
        HRESULT ( __stdcall *ExtruderParamValue )( 
            IParamsReceiver * This,
            /* [in] */ long index,
            /* [retval][out] */ BSTR *result);
        
        END_INTERFACE
    } IParamsReceiverVtbl;

    interface IParamsReceiver
    {
        CONST_VTBL struct IParamsReceiverVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IParamsReceiver_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define IParamsReceiver_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define IParamsReceiver_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define IParamsReceiver_GlobalParamsSize(This,result)	\
    (This)->lpVtbl -> GlobalParamsSize(This,result)

#define IParamsReceiver_GlobalParamName(This,index,result)	\
    (This)->lpVtbl -> GlobalParamName(This,index,result)

#define IParamsReceiver_GlobalParamValue(This,index,result)	\
    (This)->lpVtbl -> GlobalParamValue(This,index,result)

#define IParamsReceiver_ExtruderParamsSize(This,result)	\
    (This)->lpVtbl -> ExtruderParamsSize(This,result)

#define IParamsReceiver_ExtruderParamName(This,index,result)	\
    (This)->lpVtbl -> ExtruderParamName(This,index,result)

#define IParamsReceiver_ExtruderParamValue(This,index,result)	\
    (This)->lpVtbl -> ExtruderParamValue(This,index,result)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT __stdcall IParamsReceiver_GlobalParamsSize_Proxy( 
    IParamsReceiver * This,
    /* [retval][out] */ long *result);


void __RPC_STUB IParamsReceiver_GlobalParamsSize_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT __stdcall IParamsReceiver_GlobalParamName_Proxy( 
    IParamsReceiver * This,
    /* [in] */ long index,
    /* [retval][out] */ BSTR *result);


void __RPC_STUB IParamsReceiver_GlobalParamName_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT __stdcall IParamsReceiver_GlobalParamValue_Proxy( 
    IParamsReceiver * This,
    /* [in] */ long index,
    /* [retval][out] */ BSTR *result);


void __RPC_STUB IParamsReceiver_GlobalParamValue_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT __stdcall IParamsReceiver_ExtruderParamsSize_Proxy( 
    IParamsReceiver * This,
    /* [retval][out] */ long *result);


void __RPC_STUB IParamsReceiver_ExtruderParamsSize_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT __stdcall IParamsReceiver_ExtruderParamName_Proxy( 
    IParamsReceiver * This,
    /* [in] */ long index,
    /* [retval][out] */ BSTR *result);


void __RPC_STUB IParamsReceiver_ExtruderParamName_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT __stdcall IParamsReceiver_ExtruderParamValue_Proxy( 
    IParamsReceiver * This,
    /* [in] */ long index,
    /* [retval][out] */ BSTR *result);


void __RPC_STUB IParamsReceiver_ExtruderParamValue_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __IParamsReceiver_INTERFACE_DEFINED__ */


#ifndef __ICuraConnectionLibrary_INTERFACE_DEFINED__
#define __ICuraConnectionLibrary_INTERFACE_DEFINED__

/* interface ICuraConnectionLibrary */
/* [object][version][uuid][helpstring] */ 


EXTERN_C const IID IID_ICuraConnectionLibrary;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("52DB7F0D-4D16-4417-AC24-21DA256382B9")
    ICuraConnectionLibrary : public IUnknown
    {
    public:
        virtual /* [propget][helpstring] */ HRESULT __stdcall get_TrianglesReciever( 
            /* [retval][out] */ ITrianglesReciever **Value) = 0;
        
        virtual HRESULT __stdcall Slice( 
            /* [in] */ ICuraEngineControlProcess *CuraEngineControlProcess,
            /* [in] */ IParamsReceiver *ParamsReceiver,
            /* [in] */ BSTR CuraPath,
            /* [retval][out] */ VARIANT_BOOL *result) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct ICuraConnectionLibraryVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ICuraConnectionLibrary * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ICuraConnectionLibrary * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ICuraConnectionLibrary * This);
        
        /* [propget][helpstring] */ HRESULT ( __stdcall *get_TrianglesReciever )( 
            ICuraConnectionLibrary * This,
            /* [retval][out] */ ITrianglesReciever **Value);
        
        HRESULT ( __stdcall *Slice )( 
            ICuraConnectionLibrary * This,
            /* [in] */ ICuraEngineControlProcess *CuraEngineControlProcess,
            /* [in] */ IParamsReceiver *ParamsReceiver,
            /* [in] */ BSTR CuraPath,
            /* [retval][out] */ VARIANT_BOOL *result);
        
        END_INTERFACE
    } ICuraConnectionLibraryVtbl;

    interface ICuraConnectionLibrary
    {
        CONST_VTBL struct ICuraConnectionLibraryVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ICuraConnectionLibrary_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define ICuraConnectionLibrary_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define ICuraConnectionLibrary_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define ICuraConnectionLibrary_get_TrianglesReciever(This,Value)	\
    (This)->lpVtbl -> get_TrianglesReciever(This,Value)

#define ICuraConnectionLibrary_Slice(This,CuraEngineControlProcess,ParamsReceiver,CuraPath,result)	\
    (This)->lpVtbl -> Slice(This,CuraEngineControlProcess,ParamsReceiver,CuraPath,result)

#endif /* COBJMACROS */


#endif 	/* C style interface */



/* [propget][helpstring] */ HRESULT __stdcall ICuraConnectionLibrary_get_TrianglesReciever_Proxy( 
    ICuraConnectionLibrary * This,
    /* [retval][out] */ ITrianglesReciever **Value);


void __RPC_STUB ICuraConnectionLibrary_get_TrianglesReciever_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT __stdcall ICuraConnectionLibrary_Slice_Proxy( 
    ICuraConnectionLibrary * This,
    /* [in] */ ICuraEngineControlProcess *CuraEngineControlProcess,
    /* [in] */ IParamsReceiver *ParamsReceiver,
    /* [in] */ BSTR CuraPath,
    /* [retval][out] */ VARIANT_BOOL *result);


void __RPC_STUB ICuraConnectionLibrary_Slice_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __ICuraConnectionLibrary_INTERFACE_DEFINED__ */

#endif /* __CuraConnectionInterface_LIBRARY_DEFINED__ */

/* Additional Prototypes for ALL interfaces */

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


