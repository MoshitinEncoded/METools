using System;

namespace MoshitinEncoded.GraphTools
{
    public class AddParameterMenuAttribute : Attribute
    {
        public string MenuPath = string.Empty;
        public int GroupLevel = int.MaxValue;
        public string SubMenuPath = string.Empty;

        public AddParameterMenuAttribute()
        {
            var slashIndex = MenuPath.LastIndexOf('/');
            if (slashIndex > 0)
            {
                SubMenuPath = MenuPath[..slashIndex];
            }
        }
    }
}
