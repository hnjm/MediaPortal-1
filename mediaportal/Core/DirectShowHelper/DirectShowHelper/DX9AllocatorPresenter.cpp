#include "StdAfx.h"
#include ".\dx9allocatorpresenter.h"
#include ".\Vector.h"


void CVMR9AllocatorPresenter::Log(const char *fmt, ...) 
{
	va_list ap;
	va_start(ap,fmt);

	char buffer[1000]; 
	int tmp;
	va_start(ap,fmt);
	tmp=vsprintf(buffer, fmt, ap);
	va_end(ap); 

	FILE* fp = fopen("log/vmr9.log","a+");
	if (fp!=NULL)
	{
		fprintf(fp,"%s\n",buffer);
		fclose(fp);
	}
};

CVMR9AllocatorPresenter::CVMR9AllocatorPresenter(IDirect3DDevice9* direct3dDevice, IVMR9Callback* callback, HMONITOR monitor)
: m_refCount(1)
{
	Log("-------------------------------------");
	m_hMonitor=monitor;
	m_pD3DDev.Attach(direct3dDevice);
	m_pCallback=callback;
	m_surfaceCount=0;

}

CVMR9AllocatorPresenter::~CVMR9AllocatorPresenter()
{
	Deinit();
}


// IUnknown
HRESULT CVMR9AllocatorPresenter::QueryInterface( 
        REFIID riid,
        void** ppvObject)
{
    HRESULT hr = E_NOINTERFACE;

    if( ppvObject == NULL ) {
        hr = E_POINTER;
    } 
    else if( riid == IID_IVMRSurfaceAllocator9 ) {
        *ppvObject = static_cast<IVMRSurfaceAllocator9*>( this );
        AddRef();
        hr = S_OK;
    } 
    else if( riid == IID_IVMRImagePresenter9 ) {
        *ppvObject = static_cast<IVMRImagePresenter9*>( this );
        AddRef();
        hr = S_OK;
    } 
    else if( riid == IID_IUnknown ) {
        *ppvObject = 
            static_cast<IUnknown*>( 
            static_cast<IVMRSurfaceAllocator9*>( this ) );
        AddRef();
        hr = S_OK;    
    }

    return hr;
}

ULONG CVMR9AllocatorPresenter::AddRef()
{
    return InterlockedIncrement(& m_refCount);
}

ULONG CVMR9AllocatorPresenter::Release()
{
    ULONG ret = InterlockedDecrement(& m_refCount);
    if( ret == 0 )
    {
        delete this;
    }

    return ret;
}

STDMETHODIMP CVMR9AllocatorPresenter::InitializeDevice(DWORD_PTR dwUserID, VMR9AllocationInfo* lpAllocInfo, DWORD* lpNumBuffers)
{
	if(!lpAllocInfo || !lpNumBuffers)
		return E_POINTER;

	if(!m_pIVMRSurfAllocNotify)
		return E_FAIL;

	Log("InitializeDevice()");
	Log("InitializeDevice() %dx%d AR %d:%d flags:%d buffers:%d  fmt:(%x) %c%c%c%c", 
			lpAllocInfo->dwWidth,lpAllocInfo->dwHeight, 
			lpAllocInfo->szAspectRatio.cx,lpAllocInfo->szAspectRatio.cy,
			lpAllocInfo->dwFlags,
			*lpNumBuffers,
			lpAllocInfo->Format,
			((char)lpAllocInfo->Format&0xff),
			((char)(lpAllocInfo->Format>>8)&0xff),
			((char)(lpAllocInfo->Format>>16)&0xff),
			((char)(lpAllocInfo->Format>>24)&0xff));
	// StretchRect's yv12 -> rgb conversion looks horribly bright compared to the result of yuy2 -> rgb
	if(lpAllocInfo->Format == '21VY' || lpAllocInfo->Format == '024Y')
	{
		Log("InitializeDevice()   invalid format");
		return E_FAIL;
	}
	if (lpAllocInfo->Format!=D3DFMT_A8R8G8B8 &&
		lpAllocInfo->Format!=D3DFMT_R5G6B5 &&
		lpAllocInfo->Format!=D3DFMT_R8G8B8 &&
		lpAllocInfo->Format!=D3DFMT_X8R8G8B8 &&
		lpAllocInfo->Format!=D3DFMT_R5G6B5 &&
		lpAllocInfo->Format!=D3DFMT_X1R5G5B5) return E_FAIL;
	DeleteSurfaces();
	
	m_surfaceCount=*lpNumBuffers;
	
    HRESULT hr;

	m_pSurfaces.resize(*lpNumBuffers);
	hr = m_pIVMRSurfAllocNotify->AllocateSurfaceHelper(lpAllocInfo, lpNumBuffers, & m_pSurfaces.at(0) );
	if(FAILED(hr))
	{
		
		Log("InitializeDevice()   AllocateSurfaceHelper returned:0x%x",hr);
		return hr;
	}
	m_NativeVideoSize = CSize(lpAllocInfo->dwWidth, abs((int)lpAllocInfo->dwHeight));
	m_AspectRatio = m_NativeVideoSize;
	int arx = lpAllocInfo->szAspectRatio.cx, ary = lpAllocInfo->szAspectRatio.cy;
	if(arx > 0 && ary > 0) m_AspectRatio.SetSize(arx, ary);

	if(FAILED(hr = AllocSurfaces()))
	{
		Log("InitializeDevice()   AllocSurfaces returned:0x%x",hr);
		return hr;
	}
	// test if the colorspace is acceptable
	if(FAILED(hr = m_pD3DDev->StretchRect(m_pSurfaces[0], NULL, m_pVideoSurface, NULL, D3DTEXF_NONE)))
	{
		Log("InitializeDevice()   StretchRect failed with hr:0x%x",hr);
		DeleteSurfaces();
		return E_FAIL;
	}

	hr = m_pD3DDev->ColorFill(m_pVideoSurface, NULL, 0);

	Log("InitializeDevice() done()");
	return hr;
}

STDMETHODIMP CVMR9AllocatorPresenter::TerminateDevice(DWORD_PTR dwUserID)
{
    DeleteSurfaces();
	
	Log("TerminateDevice()");
    return S_OK;
}

STDMETHODIMP CVMR9AllocatorPresenter::GetSurface(DWORD_PTR dwUserID, DWORD SurfaceIndex, DWORD SurfaceFlags, IDirect3DSurface9** lplpSurface)
{
    if(!lplpSurface)
		return E_POINTER;



    if (SurfaceIndex >= m_pSurfaces.size() ) 
    {
        return E_FAIL;
    }
    CAutoLock cAutoLock(this);

	return m_pSurfaces[SurfaceIndex].CopyTo(lplpSurface) ;
}

STDMETHODIMP CVMR9AllocatorPresenter::AdviseNotify(IVMRSurfaceAllocatorNotify9* lpIVMRSurfAllocNotify)
{
    CAutoLock cAutoLock(this);
	
	m_pIVMRSurfAllocNotify = lpIVMRSurfAllocNotify;

	HRESULT hr;
    if(FAILED(hr = m_pIVMRSurfAllocNotify->SetD3DDevice(m_pD3DDev, m_hMonitor)))
		return hr;

    return S_OK;
}

// IVMRImagePresenter9

STDMETHODIMP CVMR9AllocatorPresenter::StartPresenting(DWORD_PTR dwUserID)
{
    CAutoLock cAutoLock(this);

    ASSERT(m_pD3DDev);

	
	Log("StartPresenting()");
	return m_pD3DDev ? S_OK : E_FAIL;
}

STDMETHODIMP CVMR9AllocatorPresenter::StopPresenting(DWORD_PTR dwUserID)
{
	Log("StopPresenting()");
	return S_OK;
}

STDMETHODIMP CVMR9AllocatorPresenter::PresentImage(DWORD_PTR dwUserID, VMR9PresentationInfo* lpPresInfo)
{
    HRESULT hr;

	{
		if(!m_pIVMRSurfAllocNotify)
			return E_FAIL;
		if(!lpPresInfo || !lpPresInfo->lpSurf)
			return E_POINTER;
	}
	{

		CAutoLock cAutoLock(this);

		hr = m_pD3DDev->StretchRect(lpPresInfo->lpSurf, NULL, m_pVideoSurface, NULL, D3DTEXF_NONE);

		m_fps = 10000000.0 / (lpPresInfo->rtEnd - lpPresInfo->rtStart);

		CSize VideoSize = m_NativeVideoSize;
		int arx = lpPresInfo->szAspectRatio.cx, ary = lpPresInfo->szAspectRatio.cy;
		if(arx > 0 && ary > 0) VideoSize.cx = VideoSize.cy*arx/ary;
		if(VideoSize != GetVideoSize())
		{
			m_AspectRatio.SetSize(arx, ary);
		}

		Paint(true);
		hr = S_OK;
	}

    return hr;
}

void CVMR9AllocatorPresenter::DeleteSurfaces()
{
    CAutoLock cAutoLock(this);

	m_pVideoSurfaceOff = NULL;
	m_pVideoSurfaceYUY2 = NULL;

	m_pVideoTexture = NULL;
	m_pVideoSurface = NULL;

	for( size_t i = 0; i < m_pSurfaces.size(); ++i ) 
    {
        m_pSurfaces[i] = NULL;
    }

	
}

HRESULT CVMR9AllocatorPresenter::AllocSurfaces()
{
    CAutoLock cAutoLock(this);

	m_pVideoSurfaceOff = NULL;
	m_pVideoSurfaceYUY2 = NULL;
	m_pVideoTexture = NULL;
	m_pVideoSurface = NULL;

	HRESULT hr;

	if(FAILED(hr = m_pD3DDev->CreateOffscreenPlainSurface(
		m_NativeVideoSize.cx, m_NativeVideoSize.cy, D3DFMT_X8R8G8B8, 
		D3DPOOL_DEFAULT, &m_pVideoSurfaceOff, NULL)))
	{
		Log("AllocSurfaces()   cannot create offscreen surface for X8R8G8B8 :0x%x",hr);
		return hr;
	}
	else Log("AllocSurfaces()   created offscreen surface X8R8G8B8");

	m_pD3DDev->ColorFill(m_pVideoSurfaceOff, NULL, 0);

	if(FAILED(hr = m_pD3DDev->CreateOffscreenPlainSurface(
		m_NativeVideoSize.cx, m_NativeVideoSize.cy, D3DFMT_YUY2, 
		D3DPOOL_DEFAULT, &m_pVideoSurfaceYUY2, NULL)))
	{
		Log("AllocSurfaces()   cannot create offscreen surface for YUY2 :0x%x",hr);
		m_pVideoSurfaceYUY2 = NULL;
	}
	else Log("AllocSurfaces()   created offscreen surface for YUY2");
	if(m_pVideoSurfaceYUY2)
	{
		m_pD3DDev->ColorFill(m_pVideoSurfaceOff, NULL, 0x80108010);
	}


//	AppSettings& s = AfxGetAppSettings();

	m_pVideoTexture = NULL;
	m_pVideoSurface = NULL;


//	if(s.iAPSurfaceUsage == VIDRNDT_AP_TEXTURE2D || s.iAPSurfaceUsage == VIDRNDT_AP_TEXTURE3D)
	{
		if(FAILED(hr = m_pD3DDev->CreateTexture(
			m_NativeVideoSize.cx, m_NativeVideoSize.cy, 1, D3DUSAGE_RENDERTARGET, /*D3DFMT_X8R8G8B8*/D3DFMT_A8R8G8B8, 
			D3DPOOL_DEFAULT, &m_pVideoTexture, NULL)))
		{
			Log("AllocSurfaces()   cannot create texture for A8R8G8B8 :0x%x",hr);

			if(FAILED(hr = m_pD3DDev->CreateTexture(
				m_NativeVideoSize.cx, m_NativeVideoSize.cy, 1, D3DUSAGE_RENDERTARGET, /*D3DFMT_X8R8G8B8*/D3DFMT_X8R8G8B8, 
				D3DPOOL_DEFAULT, &m_pVideoTexture, NULL)))
			{	
				Log("AllocSurfaces()   cannot create texture for X8R8G8B8 :0x%x",hr);
				return hr;
			}
			else
				Log("AllocSurfaces()   created texture X8R8G8B8");
		}
		else
			Log("AllocSurfaces()   created texture A8R8G8B8");

		if(FAILED(hr = m_pVideoTexture->GetSurfaceLevel(0, &m_pVideoSurface)))
		{
			Log("AllocSurfaces()   cannot get surface level 0x:%x",hr);
			return hr;
		}
	//	if(s.iAPSurfaceUsage == VIDRNDT_AP_TEXTURE2D) 
	//		m_pVideoTexture = NULL;
	}
/*	else
	{
		if(FAILED(hr = m_pD3DDev->CreateOffscreenPlainSurface(
			m_NativeVideoSize.cx, m_NativeVideoSize.cy, D3DFMT_X8R8G8B8, 
			D3DPOOL_DEFAULT, &m_pVideoSurface, NULL)))
			return hr;
	}
*/
	hr = m_pD3DDev->ColorFill(m_pVideoSurface, NULL, 0);

	return S_OK;
}

CSize CVMR9AllocatorPresenter::GetVideoSize(bool fCorrectAR)
{
	CSize VideoSize(m_NativeVideoSize);

	if(fCorrectAR && m_AspectRatio.cx > 0 && m_AspectRatio.cy > 0)
		VideoSize.cx = VideoSize.cy*m_AspectRatio.cx/m_AspectRatio.cy;

	return(VideoSize);
}

void CVMR9AllocatorPresenter::Paint(bool fAll)
{
	if (m_pCallback!=NULL)
	{
		CSize videoSize = GetVideoSize(false);
		if (m_pVideoTexture!=NULL)
		{
			IDirect3DTexture9* tex=m_pVideoTexture;
			DWORD dwPtr=(DWORD)(tex);
			m_pCallback->PresentImage(videoSize.cx, videoSize.cy, dwPtr);
		}
		else
		{
			IDirect3DSurface9* tex=m_pVideoSurface;
			DWORD dwPtr=(DWORD)(tex);
			m_pCallback->PresentSurface(videoSize.cx, videoSize.cy, dwPtr);
		}
		//tex->Release();
	}
}

void CVMR9AllocatorPresenter::Deinit()
{
	DeleteSurfaces();
	m_surfaceCount=NULL;
	m_pVideoSurfaceOff = NULL;
	m_pVideoSurfaceYUY2 = NULL;

	m_pVideoTexture = NULL;
	m_pVideoSurface = NULL;
	m_pD3DDev=NULL;
	m_pIVMRSurfAllocNotify=NULL;
}