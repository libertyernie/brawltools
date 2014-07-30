using System;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.MemoryMappedFiles;

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

            return new cFileMap(stream, prot, offset, length) { _baseStream = stream, _path = stream.Name };

//#if DEBUG
//            Console.WriteLine("Opening file map: {0}", stream.Name);
//#endif
        }

        public static FileMap FromStreamInternal(FileStream stream, FileMapProtect prot, int offset, int length)
        {
            if (length == 0)
                length = (int)stream.Length;
            
//#if DEBUG
//            Console.WriteLine("Opening file map: {0}", stream.Name);
//#endif
            
            return new cFileMap(stream, prot, offset, length) { _baseStream = stream, _path = stream.Name };
        }

    }

    public enum FileMapProtect : uint
    {
        Read = 0x01,
        ReadWrite = 0x02
    }

    public unsafe class cFileMap : FileMap
    {
        protected MemoryMappedFile _mappedFile;
        protected MemoryMappedViewAccessor _mappedFileAccessor;

        public cFileMap(FileStream stream, FileMapProtect protect, int offset, int length)
        {
            MemoryMappedFileAccess cProtect = (protect == FileMapProtect.ReadWrite) ? MemoryMappedFileAccess.ReadWrite : MemoryMappedFileAccess.Read;
            _length = length;
            _mappedFile = MemoryMappedFile.CreateFromFile(stream, stream.Name, _length, cProtect, null, HandleInheritability.None, true);
            _mappedFileAccessor = _mappedFile.CreateViewAccessor(offset, _length, cProtect);
            _addr = _mappedFileAccessor.SafeMemoryMappedViewHandle.DangerousGetHandle();
        }

        public override void Dispose()
        {
            if (_mappedFile != null) 
                _mappedFile.Dispose();
            if (_mappedFileAccessor != null) 
                _mappedFileAccessor.Dispose();
            base.Dispose();
        }
    }
}
