using System.Text;
using Newtonsoft.Json;

namespace Common;

public static class ObjectSerialize
{
    public static byte[] Serialize(this object obj)
    {
        if (obj == null)
        {
            return null;
        }

        var json = JsonConvert.SerializeObject(obj);
        return Encoding.ASCII.GetBytes(json);
    }
}