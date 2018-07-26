using System;
using System.Dynamic;

namespace TK.NodalEditor
{
    public class Sharpy : DynamicObject
    {
        public Sharpy()
        {

        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            switch (binder.Name)
            {
                case "addNode":
                    result = (Func<string, string, int, int, string>)((string a, string b, int c, int d) => NodalDirector.AddNode(a, b, c, d));
                    return true;
                case "deleteNode":
                    result = (Func<string, bool>)((string a) => NodalDirector.DeleteNode(a));
                    return true;
            }
            return false;
        }
    }
}
