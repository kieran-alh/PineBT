namespace PineBT
{
    public enum State
    {
        NONE,
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
