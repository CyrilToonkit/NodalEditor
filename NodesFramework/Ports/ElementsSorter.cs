using System.Collections.Generic;
using TK.NodalEditor;

public class ElementsSorter : IComparer<PortObj>
{
    List<PortObj> elementsRef = new List<PortObj>();

    /// <summary> 
    /// constructor to set the sort column and sort order. 
    /// </summary> 
    /// <param name="strMemberName"></param> 
    /// <param name="sortingOrder"></param> 
    public ElementsSorter(List<PortObj> inElementsRef)
    {
        elementsRef = inElementsRef;
    }

    public int Compare(PortObj Port1, PortObj Port2)
    {
        int value1 = GetIndex(Port1, elementsRef);
        int value2 = GetIndex(Port2, elementsRef);

        return value1.CompareTo(value2);
    }

    private int GetIndex(PortObj Port1, List<PortObj> portsRef)
    {
        int counter = 0;

        foreach (PortObj port in portsRef)
        {
            if (port.NativeName == Port1.NativeName)
            {
                return counter;
            }
            counter++;
        }

        return -1;
    }
} 

