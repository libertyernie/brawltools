using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.PowerPcAssembly;
using System.Text;

namespace BrawlLib.SSBB.ResourceNodes
{
    public unsafe class RelocationManager
    {
        public ModuleNode _module;
        public ModuleDataNode _section;

        private Dictionary<int, List<RelocationTarget>> _linkedCommands;
        private Dictionary<int, List<RelocationTarget>> _linkedBranches;
        private Dictionary<int, RelocationTarget> _targetRelocations;
        private Dictionary<int, List<string>> _tags;
        internal Dictionary<int, RelCommand> _commands;

        public KeyValuePair<int, RelCommand>[] GetCommands() { return _commands.OrderBy(x => x.Key).ToArray(); }

        public int _constructorIndex, _destructorIndex, _unresolvedIndex;

        public uint BaseOffset { get { return _section.Data - ((ResourceNode)_module).WorkingUncompressed.Address; } }
        
        public RelocationManager(ModuleDataNode data)
        {
            _section = data;
            if (_section._manager == null)
            {
                //Initialize
                _linkedCommands = new Dictionary<int, List<RelocationTarget>>();
                _linkedBranches = new Dictionary<int, List<RelocationTarget>>();
                _targetRelocations = new Dictionary<int, RelocationTarget>();
                _tags = new Dictionary<int, List<string>>();
                _commands = new Dictionary<int, RelCommand>();
            }
            else
            {
                //Make a copy
                _linkedCommands = _section._manager._linkedCommands;
                _linkedBranches = _section._manager._linkedBranches;
                _targetRelocations = _section._manager._targetRelocations;
                _tags = _section._manager._tags;
                _commands = _section._manager._commands;
            }
        }
        public uint GetUint(int index)
        {
            return (uint)*((buint*)_section._dataBuffer.Address + index);
        }
        public int GetInt(int index)
        {
            return (int)*((bint*)_section._dataBuffer.Address + index);
        }
        public float GetFloat(int index)
        {
            return (int)*((bint*)_section._dataBuffer.Address + index);
        }
        public Bin32 GetBin(int index)
        {
            return *((Bin32*)_section._dataBuffer.Address + index);
        }
        public PPCOpCode GetCode(int index)
        {
            return (uint)*((buint*)_section._dataBuffer.Address + index);
        }
        public string GetString(int index)
        {
            return new string((sbyte*)_section._dataBuffer.Address + index * 4);
        }
        public void SetUint(int index, uint value)
        {
            *((buint*)_section._dataBuffer.Address + index) = value;
        }
        public void SetInt(int index, int value)
        {
            *((bint*)_section._dataBuffer.Address + index) = value;
        }
        public void SetFloat(int index, float value)
        {
            *((bfloat*)_section._dataBuffer.Address + index) = value;
        }
        public void SetBin(int index, Bin32 value)
        {
            *((Bin32*)_section._dataBuffer.Address + index) = value;
        }
        public void SetCode(int index, PPCOpCode code)
        {
            *((buint*)_section._dataBuffer.Address + index) = (uint)code;
        }
        public void SetString(int index, string value)
        {
            value.Write((sbyte*)_section._dataBuffer.Address + index * 4);
        }

        public RelocationTarget GetTargetRelocation(int index)
        {
            if (_targetRelocations.ContainsKey(index))
                return _targetRelocations[index];
            return null;
        }
        public void SetTargetRelocation(int index, RelocationTarget target)
        {
            if (_targetRelocations.ContainsKey(index))
            {
                if (target != null)
                    _targetRelocations[index] = target;
                else
                    _targetRelocations.Remove(index);
            }
            else if (target != null)
                _targetRelocations.Add(index, target);
        }
        public void ClearTargetRelocation(int index) { SetTargetRelocation(index, null); }
        public RelCommand GetCommand(int index)
        {
            if (_commands.ContainsKey(index))
                return _commands[index];
            return null;
        }
        public void SetCommand(int index, RelCommand cmd)
        {
            if (_commands.ContainsKey(index))
            {
                if (cmd != null)
                    _commands[index] = cmd;
                else
                    _commands.Remove(index);
            }
            else if (cmd != null)
                _commands.Add(index, cmd);
        }
        public void ClearCommand(int index) { SetCommand(index, null); }
        public List<RelocationTarget> GetLinked(int index)
        {
            if (_linkedCommands.ContainsKey(index))
                return _linkedCommands[index];
            return null;
        }
        public void RemoveLinked(int index, RelocationTarget target)
        {
            if (_linkedCommands.ContainsKey(index) &&
                _linkedCommands[index] != null &&
                _linkedCommands[index].Contains(target))
                _linkedCommands[index].Remove(target);
        }
        public void AddLinked(int index, RelocationTarget target)
        {
            if (_linkedCommands.ContainsKey(index))
            {
                if (_linkedCommands[index] == null)
                    _linkedCommands[index] = new List<RelocationTarget>() { target };
                else
                    _linkedCommands[index].Add(target);
            }
            else
                _linkedCommands.Add(index, new List<RelocationTarget>() { target });
        }
        public void ClearLinked(int index)
        {
            if (_linkedCommands.ContainsKey(index))
            {
                _linkedCommands[index].Clear();
                _linkedCommands.Remove(index);
            }
        }
        public List<RelocationTarget> GetBranched(int index)
        {
            if (_linkedBranches.ContainsKey(index))
                return _linkedBranches[index];
            return null;
        }
        public void RemoveBranched(int index, RelocationTarget target)
        {
            if (_linkedBranches.ContainsKey(index) &&
                _linkedBranches[index] != null &&
                _linkedBranches[index].Contains(target))
                _linkedBranches[index].Remove(target);
        }
        public void AddBranched(int index, RelocationTarget target)
        {
            if (_linkedBranches.ContainsKey(index))
            {
                if (_linkedBranches[index] == null)
                    _linkedBranches[index] = new List<RelocationTarget>() { target };
                else
                    _linkedBranches[index].Add(target);
            }
            else
                _linkedBranches.Add(index, new List<RelocationTarget>() { target });
        }
        public void ClearBranched(int index)
        {
            if (_linkedBranches.ContainsKey(index))
            {
                _linkedBranches[index].Clear();
                _linkedBranches.Remove(index);
            }
        }
        public List<string> GetTags(int index)
        {
            if (_tags.ContainsKey(index))
                return _tags[index];
            return null;
        }
        public void RemoveTag(int index, string tag)
        {
            if (_tags.ContainsKey(index) &&
                _tags[index] != null &&
                _tags[index].Contains(tag))
                _tags[index].Remove(tag);
        }
        public void AddTag(int index, string target)
        {
            if (_tags.ContainsKey(index))
            {
                if (_tags[index] == null)
                    _tags[index] = new List<string>() { target };
                else
                    _tags[index].Add(target);
            }
            else
                _tags.Add(index, new List<string>() { target });
        }
        public void ClearTags(int index)
        {
            if (_tags.ContainsKey(index))
            {
                _tags[index].Clear();
                _tags.Remove(index);
            }
        }

        public static Color clrNotRelocated = Color.FromArgb(255, 255, 255);
        public static Color clrRelocated = Color.FromArgb(200, 255, 200);
        public static Color clrBadRelocate = Color.FromArgb(255, 200, 200);
        public static Color clrBlr = Color.FromArgb(255, 255, 100);

        public Color GetStatusColorFromIndex(int index)
        {
            if (GetCode(index) is PPCblr)
                return clrBlr;
            if (!_commands.ContainsKey(index))
                return clrNotRelocated;
            return clrRelocated;
        }

        internal void ClearCommands()
        {
            _commands = new Dictionary<int, RelCommand>();
        }
    }

    public class RelocationTarget
    {
        public RelocationTarget(int moduleID, int sectionID, int index)
        {
            _moduleID = moduleID;
            _sectionID = sectionID;
            _index = index;
        }

        public int _moduleID;
        public int _sectionID;
        public int _index;

        public override string ToString()
        {
            return String.Format("m{0}[{1}] 0x{2}", _moduleID, _sectionID, (_index * 4).ToString("X"));
        }

        public override int GetHashCode()
        {
            return _moduleID ^ _sectionID ^ _index;
        }

        public override bool Equals(object obj)
        {
            if (obj is RelocationTarget)
                return obj.GetHashCode() == GetHashCode();
            return false;
        }
    }
}
