using System;
using BrawlLib.Imaging;
using BrawlLib.SSBBTypes;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

namespace System
{
    [Serializable]
    public class ModelEditorViewportEntry : ISerializable
    {
        public bool Bones;
        public bool Polys;
        public bool Wireframe;
        public bool Floor;
        public bool Vertices;
        public bool Normals;
        public bool ShowCamCoords;
        public bool ModelBoundingBox;
        public bool ObjectBoundingBox;
        public bool BoneBoundingBox;
        public bool HideOffscreen;
        public bool CameraSet;
        public bool EnableSmoothing;
        public bool EnableText;

        public ViewportType Type;
        public ViewportDock Dock;

        public float _tScale, _rScale, _zScale, _nearZ, _farz, _yFov;
        public Vector4 _amb, _pos, _diff, _spec, _emis;
        public Vector3 _defaultCam;
        public Vector3 _defaultRot;

        public ModelEditorViewportEntry() { }
        public ModelEditorViewportEntry(SerializationInfo info, StreamingContext ctxt)
        {
            FieldInfo[] fields = GetType().GetFields(); //Gets public fields only
            foreach (FieldInfo f in fields)
            {
                Type t = f.FieldType;
                f.SetValue(this, info.GetValue(f.Name, t));
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            FieldInfo[] fields = GetType().GetFields(); //Gets public fields only
            foreach (FieldInfo f in fields)
            {
                Type t = f.FieldType;
                info.AddValue(f.Name, f.GetValue(this));
            }
        }

        public enum ViewportType : uint
        {
            Perspective,
            Orthographic,
            Front,
            Back,
            Left,
            Right,
            Top,
            Bottom
        }

        public enum ViewportDock : uint
        {
            Top,
            Bottom,
            Left,
            Right,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
            Fill
        }

        public static unsafe readonly ModelEditorViewportEntry Default = new ModelEditorViewportEntry()
        {
            _amb = new Vector4(100.0f / 255.0f, 100.0f / 255.0f, 100.0f / 255.0f, 1.0f),
            _diff = new Vector4(100.0f / 255.0f, 100.0f / 255.0f, 100.0f / 255.0f, 1.0f),
            _pos = new Vector4(100.0f, 45.0f, 45.0f, 1.0f),
            _spec = new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
            _emis = new Vector4(100.0f / 255.0f, 100.0f / 255.0f, 100.0f / 255.0f, 1.0f),
            _yFov = 45.0f,
            _nearZ = 1.0f,
            _farz = 200000.0f,
            _zScale = 2.5f,
            _tScale = 0.05f,
            _rScale = 0.4f,
            _defaultCam = new Vector3(),
            _defaultRot = new Vector3(),

            Bones = true,
            Wireframe = false,
            Polys = true,
            Vertices = false,
            ModelBoundingBox = false,
            ObjectBoundingBox = false,
            BoneBoundingBox = false,
            Normals = false,
            HideOffscreen = false,
            ShowCamCoords = false,
            Floor = false,
            EnableText = false,
            EnableSmoothing = false,
            CameraSet = false,

            Dock = ViewportDock.Fill,
            Type = ViewportType.Perspective
        };
    }

    [Serializable]
    public class ModelEditorSettings : ISerializable
    {
        public bool RetrieveCorrAnims;
        public bool DisplayExternalAnims;
        public bool DisplayNonBRRESAnims;
        public bool UseBindStateBox;
        public bool SyncTexToObj;
        public bool SyncObjToVIS0;
        public bool DisableBonesOnPlay;
        public bool Maximize;
        public bool GenTansCHR;
        public bool GenTansSRT;
        public bool GenTansSHP;
        public bool GenTansLight;
        public bool GenTansFog;
        public bool GenTansCam;
        public bool DisplayBRRESAnims;
        public bool SnapToColl;
        public bool FlatBoneList;
        public bool BoneListContains;

        public uint _rightPanelWidth;
        public uint _undoCount;
        public int _imageCapFmt;

        public ARGBPixel _orbColor;
        public ARGBPixel _lineColor;
        public ARGBPixel _lineDeselectedColor;
        public ARGBPixel _floorColor;

        public string _screenCapPath;
        public string _liveTexFolderPath;

        public List<ModelEditorViewportEntry> _viewports;

        public static readonly ModelEditorSettings Default = new ModelEditorSettings()
        {
            RetrieveCorrAnims = true,
            SyncTexToObj = false,
            SyncObjToVIS0 = false,
            DisableBonesOnPlay = true,
            Maximize = false,
            GenTansCHR = true,
            GenTansSRT = true,
            GenTansSHP = true,
            GenTansLight = true,
            GenTansFog = true,
            GenTansCam = true,
            DisplayNonBRRESAnims = true,
            DisplayExternalAnims = true,
            DisplayBRRESAnims = true,
            SnapToColl = false,
            FlatBoneList = true,
            BoneListContains = false,
            UseBindStateBox = true,

            _imageCapFmt = 0,
            _undoCount = 50,
            _orbColor = new ARGBPixel(255, 0, 128, 0),
            _lineColor = new ARGBPixel(255, 0, 0, 128),
            _lineDeselectedColor = new ARGBPixel(255, 128, 0, 0),
            _floorColor = new ARGBPixel(255, 128, 128, 191),

            _viewports = new List<ModelEditorViewportEntry>()
            {
                ModelEditorViewportEntry.Default
            },
        };

        public ModelEditorSettings() { }
        public ModelEditorSettings(SerializationInfo info, StreamingContext ctxt)
        {
            FieldInfo[] fields = GetType().GetFields(); //Gets public fields only
            foreach (FieldInfo f in fields)
            {
                Type t = f.FieldType;
                f.SetValue(this, info.GetValue(f.Name, t));
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            FieldInfo[] fields = GetType().GetFields(); //Gets public fields only
            foreach (FieldInfo f in fields)
            {
                Type t = f.FieldType;
                info.AddValue(f.Name, f.GetValue(this));
            }
        }
    }

    public static class Serializer
    {
        public static void SerializeObject(string filename, ISerializable obj)
        {
            Stream stream = File.Open(filename, FileMode.Create);
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Serialize(stream, obj);
            stream.Close();
        }

        public static ISerializable DeserializeObject(string filename)
        {
            Stream stream = File.Open(filename, FileMode.Open);
            BinaryFormatter bFormatter = new BinaryFormatter();
            ISerializable obj = (ISerializable)bFormatter.Deserialize(stream);
            stream.Close();
            return obj;
        }
    }
}