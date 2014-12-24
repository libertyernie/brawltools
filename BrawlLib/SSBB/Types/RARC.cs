using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using BrawlLib.Wii.Graphics;

namespace BrawlLib.SSBBTypes
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RARC
    {
        public const int Size = 0x40;
        public const string Tag = "RARC";

        public BinTag _tag;
        public buint _totalSize; //Size of the entire file
        public buint _headerOffset; //Offset to where offsets begin
        public buint _fileDataOffset; //Relative to header offset

        public buint _fileDataSize0; //Size of only internal file data 
        public buint _fileDataSize1; //Size of only internal file data, copy
        public buint _unknown0;
        public buint _unknown1;
        
        //This is the 'header', where offsets begin
        public buint _folderCount;
        public buint _folderEntriesOffset;
        public buint _unknown2;
        public buint _fileEntriesOffset;
        public buint _unknown3;
        public buint _stringTableOffset;
        public buint _unknown4;
        public buint _unknown5;
        
        private VoidPtr Address { get { fixed (void* ptr = &this)return ptr; } }

        public RARCFolderEntry* FolderEntries { get { return (RARCFolderEntry*)(Address + _headerOffset + _folderEntriesOffset); } }
        public RARCFileEntry* FileEntries { get { return (RARCFileEntry*)(Address + _headerOffset + _fileEntriesOffset); } }

        public string GetString(uint offset)
        {
            return new String((sbyte*)Address + _headerOffset + _stringTableOffset + offset);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RARCFolderEntry
    {
        public const int Size = 0x10;

        public BinTag _tag;
        public buint _stringOffset; //directory name, offset into string table
        public bushort _unknown;
        public bushort _numFileEntries;
        public buint _firstFileEntryIndex;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RARCFileEntry
    {
        public const int Size = 0x14;

        public bushort _id; //file id. If this is 0xFFFF, then this entry is a subdirectory link
        public bushort _unknown0;
        public bushort _unknown1;
        public bushort _stringOffset; //file/subdir name, offset into string table
        public buint _dataOffset; //offset to file data (for subdirs: index of Node representing the subdir)
        public buint _dataSize; //size of data
        public buint _unknown2; //seems to be always '0'
    }
}