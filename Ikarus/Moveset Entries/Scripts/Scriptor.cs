using Ikarus;
using Ikarus.UI;
using System;
using System.Audio;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BrawlLib.SSBB.ResourceNodes
{
    public class Scriptor
    {
        Script _script;
        public Scriptor(Script s) { _script = s; }

        public int Count { get { return _script.Count; } }
        public Event this[int i]
        {
            get { return _script[i]; }
            set { _script[i] = value; }
        }

        public MovesetFile Root { get { return _script._root; } }
        public ArticleEntry Article { get { return _script._parentArticle; } }

        #region Variables

        public int
            _loopCount = 0,
            _loopStartIndex = -1,
            _loopEndIndex = -1,
            _loopTime = 0,

            _switchStartIndex = -1,
            _switchEndIndex = -1,

            _ifIndex = 0,
            _currentIf = -1,

            _eventIndex = 0,
            _waitFrames = 0,
            _frameIndex = 0;

        public bool
            _looping = false,
            _runEvents = true,
            _return = false;

        public IfInfo _ifInfo;
        public List<int> _ifEndIndices = new List<int>();

        public List<Parameter> _cases = null;
        public List<int> _caseIndices;
        public int _defaultCaseIndex = -1;

        public static int _hurtBoxType = 0;
        public static List<MDL0BoneNode> _boneCollisions = new List<MDL0BoneNode>();

        #endregion

        //These events are read when not executing code in order to set something up
        //Value is only the namespace and id of the event
        public static readonly List<long> _runExceptions = new List<long>()
        {
            0x0005, //Start looping
            0x000A, //If
            0x000F, //End if
            0x000E, //Else
            0x000D, //Else if
            0x0011, //Case
            0x0012, //Default Case
            0x0013, //End Switch
        };

        /// <summary>
        /// Resets all script variables back to their initial values
        /// </summary>
        public void Reset()
        {
            _eventIndex = 0;
            _frameIndex = 0;
            _waitFrames = 0;

            _looping = false;
            _loopCount = 0;
            _loopStartIndex = -1;
            _loopEndIndex = -1;
            _loopTime = 0;

            _switchStartIndex = -1;
            _switchEndIndex = -1;
            _cases = null;
            _defaultCaseIndex = -1;
            _caseIndices = null;

            _ifInfo = null;
            _ifEndIndices = new List<int>();
            _ifIndex = 0;
            _currentIf = -1;
        }

        /// <summary>
        /// Returns true if there is nothing left to execute.
        /// </summary>
        public bool Idling { get { return _eventIndex >= Count; } }

        /// <summary>
        /// Sets how many animation frames have passed.
        /// </summary>
        /// <param name="index"></param>
        public void SetFrame(int index)
        {
            //Check if the script variables need to be reset
            if (index < _frameIndex)
                Reset();

            //Run the script up to the new index only while it's active
            while (_frameIndex <= index && !Idling && RunTime._runningScripts.Contains(_script))
            {
                //Apply the event and increment the frame index afterwards, not before
                FrameAdvance();
                _frameIndex++;
            }
        }

        /// <summary>
        /// Advances the code frame.
        /// </summary>
        internal void FrameAdvance()
        {
            //If the script is idling, there's nothing to run.
            if (Idling)
                return;

            //Wait around for a bit...
            if (_waitFrames > 0)
            {
                _waitFrames--;
                if (_waitFrames > 0)
                {
                    if (_looping)
                        _loopTime++;
                    return;
                }
            }

            //Progress until the next wait event
            while (_waitFrames == 0 && _eventIndex < Count)
            {
                //Start over the loop if it reaches the end
                //When the loop count reaches 0, the loop will terminate.
                if (_looping && _loopEndIndex == _eventIndex)
                {
                    _loopCount--;
                    _eventIndex = _loopStartIndex;
                }

                //Stop the loop if it's done looping or if it's running infinitely with no wait time
                if (_looping && ((_loopCount < 0 && _waitFrames <= 0) || _loopCount == 0))
                {
                    _looping = false;
                    _eventIndex = _loopEndIndex + 1;
                    _loopTime = 0;

                    //Break if at the end of the script
                    if (_eventIndex >= Count)
                        break;
                }

                //Add the effects of the current event to the scene
                RunEvent(_eventIndex++);
            }

            //Increase the loop time if looping
            if (_looping)
                _loopTime++;
        }

        public void RunEvent(int eventIndex)
        {
            //Get the current event and its id
            Event e = this[eventIndex];
            if (e == null)
                return;

            uint eventId = e.EventID;
            byte eNameSpace = e.NameSpace;
            byte eID = e.ID;
            byte eCount = (byte)e.Count;
            byte eUnk = e.Unknown;

            //Get raw parameter list
            int[] p = e.Select(x => x.Data).ToArray();

            //Run event only if allowed or if an exception
            if (!_runEvents && !_runExceptions.Contains((eNameSpace << 8) | eID))
                return;

            //Variables that are used often
            int id;
            Script script;
            Event ev;
            int index;
            ArticleInfo articleInfo;
            HitBox hitbox;
            RequirementInfo reqInfo;
            ActionChangeInfo aChangeInfo;
            SubActionChangeInfo sChangeInfo;

            //Code what to do for each event here!
            switch (eventId)
            {
                case 0x01000000: //Loop Rest 1 for Goto
                    Application.DoEvents();
                    break;
                case 0x00010100: //Synchronous Timer
                    _waitFrames = (int)(e[0].RealValue + 0.5f);
                    break;
                case 0x00020000: //No Operation
                    break;
                case 0x00020100: //Asynchronous Timer
                    _waitFrames = Math.Max((int)(e[0].RealValue + 0.5f) - _frameIndex, 0);
                    break;
                case 0x00040100: //Set loop data
                    _loopCount = p[0];
                    _loopStartIndex = e.Index + 1;
                    _runEvents = false;
                    break;
                case 0x00050000: //Start looping
                    _looping = true;
                    _loopEndIndex = e.Index;
                    _eventIndex = _loopStartIndex;
                    _runEvents = true;
                    break;
                case 0x000A0100: //If
                case 0x000A0200: //If Value
                case 0x000A0300: //If Unk
                case 0x000A0400: //If Comparison
                    if (_runEvents)
                    {
                        _currentIf = _ifIndex++;
                        _runEvents = false;
                        _ifInfo = new IfInfo();

                        index = eventIndex + 1;
                        while (true)
                        {
                            if (index < Count)
                            {
                                ev = this[index];
                                id = (int)ev.EventID;
                                if (id == 0x000B0100 ||
                                    id == 0x000B0200 ||
                                    id == 0x000B0300 ||
                                    id == 0x000B0400)
                                    index++;
                                else
                                    break;
                            }
                            else
                                break;
                        }
                        _ifInfo._reqIndices = new List<int>();
                        _ifInfo._reqIndices.Add(index);

                        _ifEndIndices.Add(0);
                        reqInfo = new RequirementInfo(p[0]);
                        for (int i = 1; i < ((eventId >> 8) & 0xFF); i++)
                            reqInfo._values.Add(e[i]);
                        _ifInfo._requirements = new List<List<RequirementInfo>>();
                        _ifInfo._requirements.Add(new List<RequirementInfo>());
                        _ifInfo._requirements[0].Add(reqInfo);
                    }
                    else
                    {
                        _ifIndex++;
                    }
                    break;
                case 0x000E0000: //Else
                    if (!_runEvents)
                    {
                        if (_ifIndex == _currentIf)
                        {
                            _ifInfo._elseIndex = eventIndex;
                        }
                    }
                    else
                    {
                        if (_ifIndex == _currentIf + 1)
                            _eventIndex = _ifInfo._endIndex;
                    }
                    break;
                case 0x000D0100: //Else If (req)
                case 0x000D0200: //Else If Value (req val)
                case 0x000D0300: //Else If Unk (req val unk)
                case 0x000D0400: //Else If Comparison (req var val var)

                    if (!_runEvents)
                    {
                        if (_ifIndex == _currentIf)
                        {
                            index = eventIndex + 1;
                            while (true)
                            {
                                if (index < Count)
                                {
                                    ev = this[index];
                                    id = (int)ev.EventID;
                                    if (id == 0x000B0100 ||
                                        id == 0x000B0200 ||
                                        id == 0x000B0300 ||
                                        id == 0x000B0400)
                                        index++;
                                    else
                                        break;
                                }
                                else
                                    break;
                            }
                            _ifInfo._reqIndices.Add(index);
                        }
                    }
                    else
                    {
                        if (_ifIndex == _currentIf + 1)
                            _eventIndex = _ifInfo._endIndex;
                    }

                    if (!_runEvents && _ifIndex == _currentIf + 1)
                    {
                        reqInfo = new RequirementInfo(p[0]);
                        for (int i = 1; i < eCount; i++)
                            reqInfo._values.Add(e[i]);
                        _ifInfo._requirements.Add(new List<RequirementInfo>());
                        _ifInfo._requirements[0].Add(reqInfo);
                    }
                    break;
                case 0x000B0100: //And If
                case 0x000B0200: //And If Value
                case 0x000B0300: //And If Unk
                case 0x000B0400: //And If Comparison
                    if (!_runEvents && _ifIndex == _currentIf + 1)
                    {
                        reqInfo = new RequirementInfo(p[0]);
                        for (int i = 1; i < eCount; i++)
                            reqInfo._values.Add(e[i]);
                        _ifInfo._requirements.Add(new List<RequirementInfo>());
                        _ifInfo._requirements[0].Add(reqInfo);
                    }
                    break;
                case 0x000F0000: //End if
                    _ifIndex--;
                    if (!_runEvents)
                    {
                        if (_ifIndex == _currentIf)
                        {
                            _ifInfo._endIndex = _ifEndIndices[_currentIf] = eventIndex + 1;
                            _eventIndex = _ifInfo.Run();
                            _runEvents = true;
                        }
                    }
                    break;
                case 0x00100200: //Switch
                    _cases = new List<Parameter>();
                    _caseIndices = new List<int>();

                    //Turn off events to examine them until end switch
                    //Then the examined data will be evaluated
                    _runEvents = false;

                    _switchStartIndex = eventIndex;
                    break;
                case 0x00110100: //Case
                    if (!_runEvents)
                    {
                        if (_cases != null && _caseIndices != null)
                        {
                            _cases.Add(e[0]);
                            _caseIndices.Add(e.Index);
                        }
                    }
                    else
                    {
                        _eventIndex = _switchEndIndex + 1;
                        _switchEndIndex = -1;
                    }
                    break;
                case 0x00120000: //Default Case
                    _defaultCaseIndex = e.Index;
                    break;
                case 0x00130000: //End Switch
                    _runEvents = true;
                    _switchEndIndex = e.Index;

                    //Apply cases
                    index = 0;
                    if (_switchStartIndex >= 0 && _switchStartIndex < Count)
                    {
                        Parameter Switch = this[_switchStartIndex][1];
                        foreach (Parameter param in _cases)
                        {
                            if (Switch.Compare(param, 2))
                            {
                                _eventIndex = _caseIndices[index] + 1;
                                break;
                            }
                            index++;
                        }
                    }

                    if (_cases != null && index == _cases.Count && _defaultCaseIndex != -1)
                        _eventIndex = _defaultCaseIndex + 1;

                    _defaultCaseIndex = -1;
                    _switchStartIndex = -1;
                    _cases = null;

                    break;
                case 0x00180000: //Break
                    _eventIndex = _switchEndIndex + 1;
                    _switchEndIndex = -1;
                    break;
                case 0x10050200: //Article Visiblity
                    id = p[0];
                    if (id < 0 || id >= RunTime._articles.Length)
                        break;
                    articleInfo = RunTime._articles[id];
                    if (articleInfo != null && articleInfo._model != null)
                        articleInfo._model._visible = p[1] != 0;
                    break;
                case 0x01010000: //Loop Rest
                    _waitFrames = 1;
                    break;
                case 0x06000D00: //Offensive Collison
                case 0x062B0D00: //Thrown Collision
                    hitbox = new HitBox(e, Article != null ? Article.Index : -1);
                    hitbox.HitboxID = (int)(p[0] & 0xFFFF);
                    hitbox.HitboxSize = p[5];
                    RunTime._hitBoxes.Add(hitbox);
                    break;
                case 0x06050100: //Body Collision
                    _hurtBoxType = p[0];
                    break;
                case 0x06080200: //Bone Collision
                    id = p[0];
                    if (Root.Model != null && Root.Model._linker.BoneCache.Length > id && id >= 0)
                    {
                        MDL0BoneNode bone = Root.Model._linker.BoneCache[id] as MDL0BoneNode;
                        switch ((int)p[1])
                        {
                            case 0:
                                bone._nodeColor = Color.Transparent;
                                bone._boneColor = Color.Transparent;
                                break;
                            case 1:
                                bone._nodeColor = bone._boneColor = Color.FromArgb(255, 255, 0);
                                break;
                            default:
                                bone._nodeColor = bone._boneColor = Color.FromArgb(0, 0, 255);
                                break;
                        }
                        _boneCollisions.Add(bone);
                    }
                    break;
                case 0x06060100: //Undo Bone Collision
                    foreach (MDL0BoneNode bone in _boneCollisions)
                        bone._nodeColor = bone._boneColor = Color.Transparent;
                    _boneCollisions = new List<MDL0BoneNode>();
                    break;
                case 0x060A0800: //Catch Collision 1
                case 0x060A0900: //Catch Collision 2
                case 0x060A0A00: //Catch Collision 3
                    hitbox = new HitBox(e, Article != null ? Article.Index : -1);
                    hitbox.HitboxID = p[0];
                    hitbox.HitboxSize = p[2];
                    RunTime._hitBoxes.Add(hitbox);
                    break;
                case 0x060D0000: //Terminate Catch Collisions
                    for (int i = 0; i < RunTime._hitBoxes.Count; i++)
                        if (RunTime._hitBoxes[i].IsCatch())
                            RunTime._hitBoxes.RemoveAt(i--);
                    break;
                case 0x00060000: //Loop break
                    _looping = false;
                    _eventIndex = _loopEndIndex + 1;
                    _loopTime = 0;
                    break;
                case 0x06150F00: //Special Offensive Collison
                    hitbox = new HitBox(e, Article != null ? Article.Index : -1);
                    hitbox.HitboxID = (int)(p[0] & 0xFFFF);
                    hitbox.HitboxSize = p[5];
                    RunTime._hitBoxes.Add(hitbox);
                    break;
                case 0x06040000: //Terminate Collisions
                    for (int i = 0; i < RunTime._hitBoxes.Count; i++)
                        if (RunTime._hitBoxes[i].IsOffensive(true))
                            RunTime._hitBoxes.RemoveAt(i--);
                    break;
                case 0x06030100: //Delete hitbox
                    for (int i = 0; i < RunTime._hitBoxes.Count; i++)
                    {
                        HitBox hbox = RunTime._hitBoxes[i];
                        if (hbox.HitboxID == p[0] && hbox.IsOffensive(true))
                        {
                            RunTime._hitBoxes.RemoveAt(i--);
                            break;
                        }
                    }
                    break;
                case 0x060C0100: //Delete Catch Collision
                    for (int i = 0; i < RunTime._hitBoxes.Count; i++)
                    {
                        HitBox hbox = RunTime._hitBoxes[i];
                        if (hbox.HitboxID == p[0] && hbox.IsCatch())
                        {
                            RunTime._hitBoxes.RemoveAt(i--);
                            break;
                        }
                    }
                    break;
                case 0x061B0500: //Move hitbox
                    foreach (HitBox hbox in RunTime._hitBoxes)
                        if (hbox.HitboxID == p[0] && hbox.IsOffensive(true))
                        {
                            hbox._parameters[1] = p[1];
                            hbox._parameters[6] = p[2];
                            hbox._parameters[7] = p[3];
                            hbox._parameters[8] = p[4];
                            break;
                        }
                    break;
                case 0x04060100: //Set animation frame
                    //if (Article == null)
                    //    RunTime.SetFrame((int)(e[0].RealValue + 0.05f));
                    //else
                    //    RunTime._articles[Article.Index].SetFrame((int)(e[0].RealValue + 0.05f));
                    break;
                case 0x00070100: //Subroutine
                    script = (e[0] as EventOffset)._script;
                    if (script != null && script != _script)
                    {
                        script.Reset();
                        RunTime._runningScripts.Add(script);
                        script.SetFrame(0);
                    }
                    break;
                case 0x00080000: //Return
                    _return = true;
                    _eventIndex = Count;
                    if (RunTime._runningScripts.Contains(_script))
                        RunTime._runningScripts.Remove(_script);
                    break;
                case 0x00090100: //Go to
                    script = (e[0] as EventOffset)._script;
                    if (script != null && script != _script)
                    {
                        RunTime._runningScripts.Remove(_script);
                        script.Reset();
                        RunTime._runningScripts.Add(script);
                        script.SetFrame(0);
                    }
                    break;
                case 0x0A030100: //Stop sound
                    id = p[0];
                    if (RunTime._playingSounds.ContainsKey(id))
                    {
                        List<AudioInfo> aList = RunTime._playingSounds[id];
                        foreach (AudioInfo aInfo in aList)
                            if (aInfo._buffer != null)
                            {
                                aInfo._buffer.Stop();
                                aInfo._buffer.Dispose();
                                aInfo._stream.Dispose();

                            }
                        RunTime._playingSounds.Remove(id);
                    }
                    break;
                case 0x0A000100: //Play sound
                case 0x0A010100:
                case 0x0A020100:
                case 0x0A040100:
                case 0x0A050100:
                case 0x0A060100:
                case 0x0A070100:
                case 0x0A080100:
                case 0x0A090100:
                case 0x0A0A0100:
                case 0x0A0B0100:
                case 0x0A0C0100:
                case 0x0A0D0100:
                case 0x0A0E0100:
                case 0x0A0F0100:

                    if (RunTime._muteSFX)
                        break;

                    id = p[0];
                    if (Manager.SoundArchive != null)
                    {
                        RSARNode node = Manager.SoundArchive;
                        List<RSAREntryNode> sounds = node._infoCache[0];
                        if (sounds != null && id >= 0 && id < sounds.Count)
                        {
                            RSARSoundNode s = sounds[id] as RSARSoundNode;
                            if (s != null)
                            {
                                IAudioStream stream = s.CreateStreams()[0];
                                AudioBuffer b = Manager._audioProvider.CreateBuffer(stream);
                                AudioInfo info = new AudioInfo(b, stream);

                                if (RunTime._playingSounds.ContainsKey(id))
                                    RunTime._playingSounds[id].Add(info);
                                else
                                    RunTime._playingSounds[id] = new List<AudioInfo>() { info };

                                b.Reset();
                                b.Seek(0);
                                b.Play();
                            }
                        }
                    }
                    break;
                case 0x0B000200: //Model Changer 1
                case 0x0B010200: //Model Changer 2

                    ModelVisibility visNode = null;
                    if (Article != null)
                    {
                        //Check if we have data to work with
                        articleInfo = RunTime._articles[Article.Index];

                        if (articleInfo == null ||
                            articleInfo._model == null ||
                            articleInfo._model._objList == null ||
                            articleInfo._article._mdlVis == null ||
                            articleInfo._article._mdlVis.Count == 0) break;

                        visNode = articleInfo._article._mdlVis;
                    }
                    else
                    {
                        //Check if we have data to work with
                        if (Root.Model._objList == null ||
                            Root.Data._modelVis.Count == 0) break;

                        visNode = Root.Data._modelVis;
                    }

                    //Get the target reference
                    ModelVisReference refEntry = Root.Data._modelVis[((int)(eventId >> 16 & 1))];

                    //Check if the reference and switch id is usable
                    if (refEntry.Count == 0 || p[0] < 0 || p[0] >= refEntry.Count) break;

                    //Turn off objects
                    ModelVisBoneSwitch SwitchNode = refEntry[p[0]];
                    foreach (ModelVisGroup grp in SwitchNode)
                        foreach (BoneIndexValue b in grp._bones)
                            if (b.BoneNode != null)
                                foreach (MDL0ObjectNode obj in b.BoneNode._manPolys)
                                    obj._render = false;

                    //Check if the group id is usable
                    if (p[1] > SwitchNode.Count || p[1] < 0) break;

                    //Turn on objects
                    ModelVisGroup group = SwitchNode[p[1]];
                    if (group != null)
                        foreach (BoneIndexValue b in group._bones)
                            if (b.BoneNode != null)
                                foreach (MDL0ObjectNode obj in b.BoneNode._manPolys)
                                    obj._render = true;

                    break;
                case 0x0B020100: //Model visibility
                    if (Article == null)
                        Root.Model._visible = p[0] != 0;
                    else if (Article.Index < RunTime._articles.Length && RunTime._articles[Article.Index]._model != null)
                        RunTime._articles[Article.Index]._model._visible = p[0] != 0;
                    break;
                case 0x0D000200: //Concurrent Infinite Loop
                    index = p[0];
                    EventOffset off = (e[1] as EventOffset);
                    if (off._script != null)
                    {
                        if (RunTime._concurrentLoopScripts.ContainsKey(index))
                            RunTime._concurrentLoopScripts.Remove(index);
                        RunTime._concurrentLoopScripts.Add(index, off._script);
                    }
                    break;
                case 0x0D010100: //Terminate Concurrent Infinite Loop
                    index = p[0];
                    if (RunTime._concurrentLoopScripts.ContainsKey(index))
                        RunTime._concurrentLoopScripts.Remove(index);
                    break;
                case 0x0E000100: //Set Air/Ground
                    RunTime._location = (RunTime.Location)(p[0]);
                    break;
                case 0x10000100: //Generate Article 
                case 0x10000200: //Generate Article 
                case 0x10030100: //Remove Article

                    //These events do a similar job!
                    bool removeArticle = eID == 3;

                    //Make sure we have all the data we need available
                    MainControl main = MainForm.Instance._mainControl;
                    MovesetFile mNode = Manager.Moveset;
                    if (mNode == null)
                        break;
                    DataSection d = mNode.Data;
                    if (d == null)
                        break;

                    //Get the id of the article to be called and check it
                    int aId2 = p[0];
                    if (aId2 < 0 || aId2 >= RunTime._articles.Length)
                        break;

                    //Get the called article from the article list
                    articleInfo = RunTime._articles[aId2];

                    if (articleInfo == null)
                        return;

                    //Remove or add the article
                    if (removeArticle)
                    {
                        if (!articleInfo.Running)
                            return;

                        //Remove the article's model from the scene
                        if (articleInfo._model != null)
                        {
                            main.RemoveTarget(articleInfo._model);
                            articleInfo._model._visible = false;
                        }

                        //This article is no longer available for use
                        articleInfo.Running = false;
                    }
                    else
                    {
                        if (articleInfo.Running)
                            return;

                        //Add the article's model to the scene
                        if (articleInfo._model != null)
                        {
                            main.AddTarget(articleInfo._model);
                            articleInfo._model._visible = true;

                            articleInfo._model._renderBones = RunTime.MainWindow._renderBones;
                            articleInfo._model._renderWireframe = RunTime.MainWindow._renderWireframe;
                            articleInfo._model._renderPolygons = RunTime.MainWindow._renderPolygons;
                            articleInfo._model._renderVertices = RunTime.MainWindow._renderVertices;
                            articleInfo._model._renderBox = RunTime.MainWindow._renderBox;
                            articleInfo._model._renderNormals = RunTime.MainWindow._renderNormals;
                            articleInfo._model._dontRenderOffscreen = RunTime.MainWindow._dontRenderOffscreen;
                        }

                        //This article is now available for use
                        articleInfo.Running = true;
                    }
                    break;
                case 0x10040200: //Set Anchored Article SubAction
                case 0x10070200: //Set Remote Article SubAction
                    id = p[0];
                    int sId = p[1];
                    if (id < 0 || id >= RunTime._articles.Length)
                        break;

                    //Get the called article from the article list
                    articleInfo = RunTime._articles[id];
                    if (articleInfo != null)
                    {
                        articleInfo.SubactionIndex = sId;
                        articleInfo._setAt = _frameIndex;
                    }
                    break;
                case 0x10010200: //Set Ex-Anchored Article Action
                    break;
                case 0x12000200: //Basic Var Set
                case 0x12060200: //Float Var Set
                    e[1].RealValue = e[0].RealValue;
                    break;
                case 0x12010200: //Basic Var Add
                case 0x12070200: //Float Var Add
                    e[1].RealValue = e[1].RealValue + e[0].RealValue;
                    break;
                case 0x12020200: //Basic Var Sub
                case 0x12080200: //Float Var Sub
                    e[1].RealValue = e[1].RealValue - e[0].RealValue;
                    break;
                case 0x12030100: //Basic Var Inc
                    e[0].RealValue = e[0].RealValue + 1.0f;
                    break;
                case 0x12040100: //Basic Var Dec
                    e[0].RealValue = e[0].RealValue - 1.0f;
                    break;
                case 0x120A0100: //Bit Variable Set 
                    e[0].RealValue = 1.0f;
                    break;
                case 0x120B0100: //Bit Variable Clear
                    e[0].RealValue = 0.0f;
                    break;
                case 0x120F0200: //Float Variable Multiply 
                    e[1].RealValue = e[1].RealValue * e[0].RealValue;
                    break;
                case 0x12100200: //Float Variable Divide
                    if (e[0].RealValue != 0)
                        e[1].RealValue = e[1].RealValue / e[0].RealValue;
                    break;
                case 0x64000000: //Allow Interrupt
                    RunTime._allowInterrupt = true;
                    break;
                case 0x02000300: //Change Action Status
                case 0x02000400:
                case 0x02000500:
                case 0x02000600:

                    break;
                case 0x02010200: //Change Action
                case 0x02010300:
                case 0x02010400:
                case 0x02010500:
                    aChangeInfo = new ActionChangeInfo(p[0]);
                    reqInfo = new RequirementInfo(p[1]);
                    for (int i = 2; i < Count; i++)
                        reqInfo._values.Add(e[i]);
                    aChangeInfo._requirements.Add(reqInfo);
                    RunTime.AddActionChangeInfo(aChangeInfo);
                    break;
                case 0x02040100: //Additional Change Action Requirement
                case 0x02040200:
                case 0x02040300:
                case 0x02040400:

                    break;
                case 0x02060100: //Enable Action Status ID
                    break;
                case 0x02080100: //Disable Action Status ID
                    break;
                case 0x02090200: //Invert Action Status ID 
                    break;
                case 0x020A0100: //Allow Specific Interrupt
                    break;
                case 0x020B0100: //Disallow Specific Interrupt
                    break;
                case 0x020C0100: //Unregister Interrupt
                    break;
                case 0x04000100: //Change Subaction
                case 0x04000200:
                    sChangeInfo = new SubActionChangeInfo(p[0], eCount == 2 && p[1] != 0);
                    RunTime.AddSubActionChangeInfo(sChangeInfo);
                    break;
                case 0x04010200: //Change Subaction
                    sChangeInfo = new SubActionChangeInfo(p[0], false);
                    sChangeInfo._requirements.Add(new RequirementInfo(p[1]));
                    RunTime.AddSubActionChangeInfo(sChangeInfo);
                    break;
            }
        }

        /// <summary>
        /// Evaluates a requirement using the info given.
        /// </summary>
        /// <returns>Whether the requirement returned true or false</returns>
        public static bool ApplyRequirement(RequirementInfo info)
        {
            bool not = ((info._requirement >> 31) & 1) == 1;
            long req = info._requirement & 0x7FFFFFFF;
            bool v = false;

            switch (req)
            {
                case 0x03: //Is On Ground
                    v = RunTime._location == RunTime.Location.Ground;
                    break;

                case 0x04: //Is In Air
                    v = RunTime._location == RunTime.Location.Air;
                    break;

                case 0x06: //On a pass-through floor
                    v = false;
                    break;

                case 0x07: //Compare
                    if (info._values.Count == 3)
                        v = info._values[0].Compare(info._values[2], info._values[1].Data);
                    break;

                case 0x08: //Bit is set
                    v = info._values[0].RealValue != 0.0f;
                    break;

                case 0x0C: //Touching something
                    switch (info._values[0].Data)
                    {
                        case 1: //Floor

                            break;
                        case 2: //Left Wall

                            break;
                        case 3: //Ceiling

                            break;
                        case 4: //Right Wall

                            break;
                    }
                    break;

                case 0x15: //Article Exists
                    int aId = (int)info._values[0].RealValue;
                    if (aId < 0 || aId >= RunTime._articles.Length)
                        break;
                    v = RunTime._articles[aId].Running;
                    break;

                case 0x17: //Within distance from floor
                    if (Manager.Moveset != null &&
                        Manager.Moveset.Data != null &&
                        Manager.Moveset.Data._misc != null)
                    {
                        Vector3 p = Manager.Moveset.Data._misc._boneRefs[4].BoneNode._frameMatrix.GetPoint();
                        v = p._y < info._values[0].RealValue;
                    }
                    break;

                case 0x2A: //In Water
                    return false;

                case 0x2B: //Roll a die
                    //int sides = (int)info._values[0].RealValue;

                    break;

                case 0x2C: //Subaction exists

                    break;
                case 0x30: //Button Press

                    break;
                case 0x31: //Button Release

                    break;
                case 0x32: //Button Pressed

                    break;
                case 0x33: //Button Not Pressed

                    break;
                case 0x34: //Stick Dir Pressed

                    break;
                case 0x35: //Stick Dir Not Pressed

                    break;
            }

            return not ? !v : v;
        }
    }
}
