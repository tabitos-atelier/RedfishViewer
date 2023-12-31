using Prism.Events;
using RedfishViewer.Models;

namespace RedfishViewer.Events
{
    internal class SaveNodeEvent<T> : PubSubEvent<Node>
    {
    }
}
