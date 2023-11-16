using System;

namespace MoshitinEncoded.GraphTools
{
    public class AddParameterMenuAttribute : Attribute
    {
        public string MenuPath = string.Empty;
        
        public AddParameterMenuAttribute() {}
    }
}
