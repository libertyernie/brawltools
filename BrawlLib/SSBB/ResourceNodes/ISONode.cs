using System;
using BrawlLib.SSBBTypes;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class ISONode : ISOEntryNode
    {
        internal static byte[] LoadedKey = null;

        internal ISO* Header { get { return (ISO*)WorkingUncompressed.Address; } }
        public override ResourceType ResourceType { get { return ResourceType.Unknown; } }
        public override string DataSize { get { return "0x" + WorkingUncompressed.Map.BaseStream.Length.ToString("X"); } }

        string _gameName;
        public bool _isGC = false;

        [Category("ISO Disc Image")]
        public string GameName
        {
            get { return _gameName; }
            set { _gameName = value.Substring(0, Math.Max(0x60, value.Length)); }
        }
        public override bool OnInitialize()
        {
            _name = Header->GameID;
            _gameName = Header->GameName;
            PartitionHeader p = Get<PartitionHeader>(0x40000);
            return p._partitionCount > 0 || p._channelCount > 0;
        }
        public override void OnPopulate()
        {
            PartitionHeader p = Get<PartitionHeader>(0x40000);
            int pCount = p._partitionCount;
            int total = p._channelCount + pCount;
            
            for (int i = 0; i < total; ++i)
            {
                long offset = i < pCount ? (p.PartitionOffset + i * 8L) : (p.ChannelOffset + (i - pCount) * 8L);
                PartitionTableEntry e = Get<PartitionTableEntry>(offset);
                new ISOPartitionNode(e, i >= pCount).Create(this, e._offset * OffMult, 0x8000, false);
            }
        }
        internal static ResourceNode TryParse(DataSource source)
        {
            ISO* header = (ISO*)source.Address;
            bool GCMatch = header->_tagGC == ISO.GCTag;
            bool WiiMatch = header->_tagWii == ISO.WiiTag;
            if ((GCMatch || WiiMatch) && (LoadedKey != null || LoadKey()))
                return new ISONode() { _isGC = GCMatch };
            return null;
        }
        private static bool LoadKey()
        {
            if (File.Exists("key.bin"))
            {
                FileStream s = File.OpenRead("key.bin");
                if (s.Length == 16)
                {
                    LoadedKey = new byte[16];
                    s.Read(LoadedKey, 0, 16);
                    return true;
                }
            }
            MessageBox.Show("Unable to load key.bin");
            return false;
        }
    }
    public unsafe class ISOPartitionNode : ISOEntryNode, IBufferNode
    {
        internal VoidPtr Header { get { return WorkingUncompressed.Address; } }

        PartitionTableEntry.Type _type;
        string _vcID;
        RSAType _rsaType;
        byte[] _rsaSig;
        TMDInfo _tmd;
        List<TMDEntry> _tmdEntries;
        bool _encrypted = false;
        uint _cachedBlock = uint.MaxValue;

        [Category("TMD")]
        public RSAType RSA { get { return _rsaType; } }
        [Category("TMD")]
        public byte[] RSASig { get { return _rsaSig; } }
        [Category("TMD")]
        public byte Version { get { return _tmd._version; } }
        [Category("TMD")]
        public byte CaCrlVersion { get { return _tmd._caCrlVersion; } }
        [Category("TMD")]
        public byte SignerCrlVersion { get { return _tmd._signerCrlVersion; } }
        [Category("TMD")]
        public int SysVersionLo { get { return _tmd._sysVersionLo; } }
        [Category("TMD")]
        public int SysVersionHi { get { return _tmd._sysVersionHi; } }
        [Category("TMD")]
        public short TitleID0 { get { return _tmd._titleID0; } }
        [Category("TMD")]
        public short TitleID1 { get { return _tmd._titleID1; } }
        [Category("TMD")]
        public string TitleTag { get { return _tmd._titleTag; } }
        [Category("TMD")]
        public int TitleType { get { return _tmd._titleType; } }
        [Category("TMD")]
        public short GroupID { get { return _tmd._groupID; } }
        [Category("TMD")]
        public int AccessRights { get { return _tmd._accessRights; } }
        [Category("TMD")]
        public short TitleVersion { get { return _tmd._titleVersion; } }
        [Category("TMD")]
        public short NumContents { get { return _tmd._numContents; } }
        [Category("TMD")]
        public short BootIndex { get { return _tmd._bootIndex; } }
        [Category("TMD")]
        public TMDEntry[] Entries { get { return _tmdEntries != null ? _tmdEntries.ToArray() : null; } }
        [Category("TMD")]
        public byte[] TitleKey { get { return _titleKey; } }
        [Category("TMD")]
        public byte[] IV { get { return _iv; } }

        byte[] _titleKey, _iv;

        [Category("ISO Partition")]
        public PartitionTableEntry.Type PartitionType
        {
            get { return _type; }
            set { _type = value; SignalPropertyChange(); }
        }
        [Category("ISO Partition")]
        public string VirtualConsoleID
        {
            get { return _vcID; }
            set
            {
                if (PartitionType == PartitionTableEntry.Type.VirtualConsole)
                {
                    _vcID = value.PadLeft(4, ' ').Substring(value.Length - 4, value.Length);
                    SignalPropertyChange();
                }
                else
                    _vcID = "N/A";
            }
        }
        public ISOPartitionNode(PartitionTableEntry entry, bool VC)
        {
            if (VC)
            {
                _vcID = entry.GameID;
                _type = PartitionTableEntry.Type.VirtualConsole;
            }
            else
            {
                _vcID = "N/A";
                _type = entry.PartitionType;
            }
        }
        public enum RSAType : uint
        {
            None = 0,
            RSA2048 = 0x00010001,
            RSA4096 = 0x00010000,
        }
        public override bool OnInitialize()
        {
            base.OnInitialize();

            const int apploaderOffset = 0x2440;

            _name = String.Format("[{0}] {1}", Index, PartitionType.ToString());
            PartitionInfo info = Get<PartitionInfo>(0x2A4, true);
            
            long tmdOffset = info._tmdOffset * OffMult;
            uint sigType = Get<buint>(tmdOffset, true);
            _rsaType = (RSAType)sigType;
            if (sigType != 0)
            {
                tmdOffset += 4L;

                int size = 0x200 - (((int)sigType & 1) * 0x100);

                _rsaSig = GetBytes(tmdOffset, size, true);

                tmdOffset += size;
                tmdOffset = ((tmdOffset + 64 - 1) & ~(64 - 1));

                _tmd = Get<TMDInfo>(tmdOffset, true);
                tmdOffset += TMDInfo.Size;

                _tmdEntries = new List<TMDEntry>();
                for (int i = 0; i < _tmd._numContents; ++i)
                    _tmdEntries.Add(Get<TMDEntry>(tmdOffset + i * TMDEntry.Size, true));

                byte[] encryptedKey = GetBytes(0x1BF, 16, true);
                _iv = GetBytes(0x1DC, 8, true);
                Array.Resize(ref _iv, 16);

                using (AesManaged aesAlg = new AesManaged())
                {
                    aesAlg.Key = ISONode.LoadedKey;
                    aesAlg.IV = _iv;
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.Zeros;

                    _titleKey = new byte[16];
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (MemoryStream msDecrypt = new MemoryStream(encryptedKey))
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                            csDecrypt.Read(_titleKey, 0, 16);
                }
            }

            return true;
        }

        public bool IsValid()
        {
            return GetLength() > 0;
        }

        public VoidPtr GetAddress()
        {
            return WorkingUncompressed.Address;
        }

        public int GetLength()
        {
            return WorkingUncompressed.Length;
        }
    }
    public unsafe abstract class ISOEntryNode : ResourceNode
    {
        //ISOs are too big for file mapping,
        //so we need to use some special functions to get data from the base stream.

        public long _rootOffset;

        //Store the buffers the children are initialized on so that they are not disposed of before the node is
        List<UnsafeBuffer> _childBuffers = new List<UnsafeBuffer>();

        public virtual string DataSize { get { return "0x" + WorkingUncompressed.Length.ToString("X"); } }
        protected ISONode ISORoot { get { return RootNode as ISONode; } }
        protected long OffMult { get { return ISORoot._isGC ? 0L : 4L; } }

        public T Get<T>(long offset, bool relative = false) where T : struct
        {
            FileStream s = RootNode.WorkingUncompressed.Map.BaseStream;
            s.Seek(relative ? _rootOffset + offset : offset, SeekOrigin.Begin);

            int size = Marshal.SizeOf(default(T));
            byte[] data = new byte[size];
            s.Read(data, 0, size);

            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            T dataStruct = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();

            return dataStruct;
        }
        public UnsafeBuffer GetData(long offset, int size, bool relative = false)
        {
            byte[] data = GetBytes(offset, size, relative);
            UnsafeBuffer buffer = new UnsafeBuffer(size);
            fixed (byte* b = &data[0])
                Memory.Move(buffer.Address, b, (uint)size);
            return buffer;
        }
        public byte[] GetBytes(long offset, int size, bool relative = false)
        {
            FileStream s = RootNode.WorkingUncompressed.Map.BaseStream;
            s.Seek(relative ? _rootOffset + offset : offset, SeekOrigin.Begin);
            byte[] data = new byte[size];
            s.Read(data, 0, size);
            return data;
        }
        public void Create(ISOEntryNode parent, long offset, int size, bool relativeOffset)
        {
            _rootOffset = relativeOffset ? parent._rootOffset + offset : offset;

            _parent = parent;
            UnsafeBuffer buffer = GetData(_rootOffset, size);
            _parent = null;

            Initialize(parent, buffer.Address, buffer.Length);

            parent._childBuffers.Add(buffer);
        }
    }
}
