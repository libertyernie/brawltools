using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using BrawlLib.SSBB.ResourceNodes;
using Ikarus;

namespace BrawlLib.SSBB.ResourceNodes
{
    public class DropDownListBonesMDef : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            MDL0Node model = (context.Instance as MovesetEntry).Model;
            if (model != null)
                return new StandardValuesCollection(model._linker.BoneCache.Select(n => n.ToString()).ToList());
            return null;
        }
    }

    public class DropDownListRequirementsMDef : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            string[] values = Manager.iRequirements;
            if (values != null)
                return new StandardValuesCollection(values);
            return null;
        }
    }

    public class DropDownListGFXFilesMDef : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            string[] values = Manager.iGFXFiles;
            if (values != null)
                return new StandardValuesCollection(values);
            return null;
        }
    }

    public class DropDownListExtNodesMDef : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            ExternalEntry[] values = (context.Instance as MovesetEntry)._root.ReferenceList.ToArray();
            if (values != null)
                return new StandardValuesCollection(values.Select(n => n.ToString()).ToList());
            return null;
        }
    }

    public class DropDownListEnumMDef : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            string[] values = (context.Instance as EventEnumValue).Enums;
            if (values != null)
                return new StandardValuesCollection(values);
            return null;
        }
    }
}
