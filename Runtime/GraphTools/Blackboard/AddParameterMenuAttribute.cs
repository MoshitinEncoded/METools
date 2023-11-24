using System;

namespace MoshitinEncoded.GraphTools
{
    public class AddParameterMenuAttribute : Attribute
    {
        public const int UNSORTED_GROUP = int.MaxValue;
        private readonly string _MenuPath = string.Empty;
        private readonly string _SubMenuPath = string.Empty;
        private readonly int _GroupLevel = UNSORTED_GROUP;

        public string MenuPath => _MenuPath;

        public string SubMenuPath => _SubMenuPath;

        public int GroupLevel => _GroupLevel;

        public AddParameterMenuAttribute(string menuPath, int groupLevel = UNSORTED_GROUP)
        {
            _MenuPath = menuPath;
            _GroupLevel = groupLevel;
            var slashIndex = menuPath.LastIndexOf('/');
            if (slashIndex > 0)
            {
                _SubMenuPath = menuPath[..slashIndex];
            }
        }
    }
}
