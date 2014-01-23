using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrawlLib.SSBB.ResourceNodes;

namespace BrawlLib.SSBB.ResourceNodes
{
    public class ObjectParser
    {
        private ModuleSectionNode _objectSection;

        public Dictionary<Relocation, RELType> _types = new Dictionary<Relocation, RELType>();
        public List<RELObjectNode> _objects = new List<RELObjectNode>();

        public ObjectParser(ModuleSectionNode section)
        {
            _objects = new List<RELObjectNode>();
            _types = new Dictionary<Relocation, RELType>();
            _objectSection = section;
        }

        public unsafe void Parse()
        {
            if (_objectSection == null)
                return;

            for (Relocation rel = _objectSection[0]; rel != null; rel = rel.Next)
                ParseDeclaration(rel);

            for (Relocation rel = _objectSection[0]; rel != null; rel = rel.Next)
                ParseObject(ref rel);
        }

        private unsafe RELType ParseDeclaration(Relocation rel)
        {
            RELType type = null;
            if (_types.TryGetValue(rel, out type))
                return type;

            if (rel.Command == null || rel.Command._targetRelocation._section != _objectSection)
                return null;

            string name = new string((sbyte*)(rel._section.BaseAddress + rel.RelOffset));

            if (String.IsNullOrEmpty(name))
                return null;

            type = new RELType(name);

            //Get inheritances, if any.
            if (rel.Next.Command != null)
                for (Relocation r = rel.Next.Command._targetRelocation;
                     r != null && r.Command != null;
                     r = r.NextAt(2))
                {
                    RELType inheritance = ParseDeclaration(r.Command._targetRelocation);
                    if (inheritance != null)
                    {
                        type.Inheritance.Add(new InheritanceItemNode(inheritance, r.Next.RawValue));
                        inheritance.Inherited = true;
                    }
                    else break;
                }

            rel.Tags.Add(type.FormalName + " Declaration");
            rel.Next.Tags.Add(type.FormalName + "->Inheritances");

            _types.Add(rel, type);

            return type;
        }

        private unsafe RELObjectNode ParseObject(ref Relocation rel)
        {
            if (rel.Command == null || rel.Command._targetRelocation._section != _objectSection)
                return null;

            RELType declaration = null;

            if (!_types.TryGetValue(rel.Command.TargetRelocation, out declaration) || declaration.Inherited)
                return null;

            RELObjectNode obj = null;
            foreach (RELObjectNode node in _objects)
                if (node._name == declaration.FullName)
                {
                    obj = node;
                    break;
                }

            if (obj == null)
            {
                obj = new RELObjectNode(declaration);
                obj._parent = _objectSection;
                _objectSection._children.Add(obj);
                new RELGroupNode() { _name = "Inheritance" }.Parent = obj;
                foreach (InheritanceItemNode n in declaration.Inheritance)
                    n.Parent = obj.Children[0];
                new RELGroupNode() { _name = "Functions" }.Parent = obj;
            }

            Relocation baseRel = rel;

            int methodIndex = 0;
            int setIndex = 0;

            // Read object methods.
            while (rel.Command != null && (rel.Command._targetRelocation._section != _objectSection || rel.SectionOffset == baseRel.SectionOffset))
            {
                if (rel.SectionOffset != baseRel.SectionOffset)
                {
                    new RELMethodNode() { _name = String.Format("Function[{0}][{1}]", setIndex, methodIndex) }.Initialize(obj.Children[1], rel.Target.Address, 0);
                    methodIndex++;
                }
                else
                {
                    if (rel.Next.RawValue != 0)
                        setIndex++;

                    methodIndex = 0;
                    rel = rel.Next;
                }
                rel = rel.Next;
            }

            baseRel.Tags.Add(obj.Type.FullName);

            _objects.Add(obj);
            return obj;
        }

        public void Populate()
        {
            foreach (RELObjectNode obj in _objects)
            {
                obj._parent = null;
                _objectSection._children.Remove(obj);
                obj.Parent = _objectSection;
            }
        }
    }
}
