using System;
using System.Runtime.InteropServices;

public class BusyScope : IDisposable
{
	private bool _isDisposed;
	private IntPtr _nativeResource = Marshal.AllocHGlobal(100);

	private IBusyResource _resource;

	public BusyScope(IBusyResource resource)
	{
		_resource = resource;

		_resource.OnLockApplied();
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_isDisposed) return;

		if (disposing)
		{
			_resource.OnLockReleased();

			_resource = null;
		}

		if (_nativeResource != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(_nativeResource);
			_nativeResource = IntPtr.Zero;
		}

		_isDisposed = true;
	}

	~BusyScope() => Dispose(false);
}
