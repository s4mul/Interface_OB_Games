using System.Collections.Generic;
using UnityEngine;

public static class GlobalHelper
{
    private static int incrementID = 0; 
    public static int GenerateUniqueID()
    {
        return incrementID++;     
    }
}