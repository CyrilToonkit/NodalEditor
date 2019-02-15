using System.Collections.Generic;
using TK.NodalEditor;

public class PortsSorter : IComparer<Port>
{
    List<Port> portsRef = new List<Port>();

    /// <summary> 
    /// constructor to set the sort column and sort order. 
    /// </summary> 
    /// <param name="strMemberName"></param> 
    /// <param name="sortingOrder"></param> 
    public PortsSorter(List<Port> inPortsRef)
    {
        portsRef = inPortsRef;
    }

    public int Compare(Port Port1, Port Port2)
    {
        int value1 = GetIndex(Port1,portsRef);
        int value2 = GetIndex(Port2, portsRef);

        return value1.CompareTo(value2);
    }

    private int GetIndex(Port Port1, List<Port> portsRef)
    {
        int counter = 0;

        foreach (Port port in portsRef)
        {
            if (port.Name == Port1.Name)
            {
                return counter;
            }
            counter++;
        }

        return -1;
    }
} 

