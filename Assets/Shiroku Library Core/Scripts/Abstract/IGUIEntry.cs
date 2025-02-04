using UnityEngine;

namespace ShirokuStudio.Core
{
    public interface IGUIEntry
    {
        string Name { get; }
        Texture Icon { get; }
    }
}