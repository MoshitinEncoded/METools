using System;

namespace MoshitinEncoded.GraphTools
{
    public class AddParameterMenuAttribute : Attribute
    {
        public string MenuPath = string.Empty;
        public int GroupLevel = 1;
        
        public AddParameterMenuAttribute() {}
    }
}
