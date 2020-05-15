using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PineBT
{
    public enum State
    {
        FRESH,
        SUCCESS,
        FAILURE,
        RUNNING,
        CANCELLED
    }
    
    public enum UpdateCycle
    {
        UPDATE,
        UPDATE_INCREMENT,
        FIXED_UPDATE,
        BOTH
    }
}
