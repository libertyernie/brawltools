using System;
using System.Runtime.InteropServices;
using System.IO;

namespace BrawlLib.IO
{
    public abstract class FileMap : IDisposable
    {
        protected VoidPtr _addr;
        protected int _length;
        protected string _path;
        protected FileStream _baseStream;

        public VoidPtr Address { get { return _addr; } }
        public int Length { get { return _length; } set { _length = value; } }
        public string FilePath { get { return _path; } }

        ~FileMap() { Dispose(); }
        public virtual void Dispose() 
        {
            if (_baseStream != null)
            {
                _baseStream.Close();
                _baseStream.Dispose();
                _baseStream = null;
            }
//#if DEBUG
//            Console.WriteLine("Closing file map: {0}", _path);
//#endif
            GC.SuppressFinalize(this); 
        }

        public static FileMap FromFile(string path) { return FromFile(path, FileMapProtect.ReadWrite, 0, 0); }
        public static FileMap FromFile(string path, FileMapProtect prot) { return FromFile(path, prot, 0, 0); }
        public static FileMap FromFile(string path, FileMapProtect prot, int offset, int length) { return FromFile(path, prot, 0, 0, FileOptions.RandomAccess); }
        public static FileMap FromFile(string path, FileMapProtect prot, int offset, int length, FileOptions options)
        {
            FileStream stream;
            FileMap map;
            try { stream = new FileStream(path, FileMode.Open, (prot == FileMapProtect.ReadWrite) ? FileAccess.ReadWrite : FileAccess.Read, FileShare.Read, 8, options); }
            catch //File is currently in use, but we can copy it to a temp location and read that
            {
                string tempPath = Path.GetTempFileName();
                File.Copy(path, tempPath, true);
                stream = new FileStream(tempPath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, 8, options | FileOptions.DeleteOnClose);
            }
            try { map = FromStreamInternal(stream, prot, offset, length); }
            catch (Exception x) { stream.Dispose(); throw x; }
            map._path = path; //In case we're using a temp file
            return map;
        }
        public static FileMap FromTempFile(int length)
        {
            FileStream stream = new FileStream(Path.GetTempFileName(), FileMode.Open, FileAccess.ReadWrite, FileShare.Read, 8, FileOptions.RandomAccess | FileOptions.DeleteOnClose);
            try { return FromStreamInternal(stream, FileMapProtect.ReadWrite, 0, length); }
            catch (Exception x) { stream.Dispose(); throw x; }
        }

        public static FileMap FromStream(FileStream stream) { return FromStream(stream, FileMapProtect.ReadWrite, 0, 0); }
        public static FileMap FromStream(FileStream stream, FileMapProtect prot) { return FromStream(stream, prot, 0, 0); }
        public static FileMap FromStream(FileStream stream, FileMapProtect prot, int offset, int length)
        {
            //FileStream newStream = new FileStream(stream.Name, FileMode.Open, prot == FileMapProtect.Read ? FileAccess.Read : FileAccess.ReadWrite, FileShare.Read, 8, FileOptions.RandomAccess);
            //try { return FromStreamInternal(newStream, prot, offset, length); }
            //catch (Exception x) { newStream.Dispose(); throw x; }

            if (length == 0)
                length = (int)stream.Length;

//#if DEBUG
//            Console.WriteLine("Opening file map: {0}", stream.Name);
//#endif
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    return new wFileMap(stream.SafeFileHandle.DangerousGetHandle(), prot, offset, (uint)length) {_path = stream.Name };
                case PlatformID.Unix:
                    return new lFileMap(stream.SafeFileHandle.DangerousGetHandle(), prot, (uint)offset, (uint)length) { _path = stream.Name };
            }
            return null;
        }

        public static FileMap FromStreamInternal(FileStream stream, FileMapProtect prot, int offset, int length)
        {
            if (length == 0)
                length = (int)stream.Length;
            
//#if DEBUG
//            Console.WriteLine("Opening file map: {0}", stream.Name);
//#endif
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    return new wFileMap(stream.SafeFileHandle.DangerousGetHandle(), prot, offset, (uint)length) { _baseStream = stream, _path = stream.Name };
                case PlatformID.Unix:
                    return new lFileMap(stream.SafeFileHandle.DangerousGetHandle(), prot, (uint)offset, (uint)length) { _baseStream = stream, _path = stream.Name };
            }
            return null;
        }

    }

    public enum FileMapProtect : uint
    {
        Read = 0x01,
        ReadWrite = 0x02
    }

    public class wFileMap : FileMap
    {
        internal wFileMap(VoidPtr hFile, FileMapProtect protect, long offset, uint length)
        {
            long maxSize = offset + length;
            uint maxHigh = (uint)(maxSize >> 32);
            uint maxLow = (uint)maxSize;
            Win32._FileMapProtect mProtect; Win32._FileMapAccess mAccess;
            if (protect == FileMapProtect.ReadWrite)
            {
                mProtect = Win32._FileMapProtect.ReadWrite;
                mAccess = Win32._FileMapAccess.Write;
            }
            else
            {
                mProtect = Win32._FileMapProtect.ReadOnly;
                mAccess = Win32._FileMapAccess.Read;
            }

            using (Win32.SafeHandle h = Win32.CreateFileMapping(hFile, null, mProtect, maxHigh, maxLow, null))
            {
                h.ErrorCheck();
                _addr = Win32.MapViewOfFile(h.Handle, mAccess, (uint)(offset >> 32), (uint)offset, length);
                if (!_addr) Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                _length = (int)length;
            }
        }

        public override void Dispose()
        {
            if (_addr) 
            {
                Win32.FlushViewOfFile(_addr, 0);
                Win32.UnmapViewOfFile(_addr);
                _addr = null;
            }
            base.Dispose();
        }
    }

    public unsafe class lFileMap : FileMap
    {
        public lFileMap(VoidPtr hFile, FileMapProtect protect, uint offset, uint length)
        {
            Linux.MMapProtect mProtect = (protect == FileMapProtect.ReadWrite) ? Linux.MMapProtect.Read | Linux.MMapProtect.Write : Linux.MMapProtect.Read;
            _addr = Linux.mmap(null, length, mProtect, Linux.MMapFlags.Shared, hFile, offset);
            _length = (int)length;
        }

        public override void Dispose()
        {
            if (_addr) { Linux.munmap(_addr, (uint)_length); _addr = null; }
            base.Dispose();
        }
    }
}
