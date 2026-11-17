namespace Lowering.Primitive;

public class FieldsLowering
{
    #region Code1
    public int A { get; private set; }
    public int B { get; }
    public int C { get; init; }
    private int _d;
    public int D
    {
        get => _d;
        set => _d = value;
    }
    #endregion
    
    
    #region Lower1
    private int _la;
    private readonly int _lb;
    private readonly int _lc;
    private int _ld;

    public int LA
    {
        get
        {
            return _la;
        }
        private set
        {
            _la = value;
        }
    }

    public int LB
    {
        get
        {
            return _lb;
        }
    }

    public int LC
    {
        get
        {
            return _lc;
        }
        init
        {
            _lc = value;
        }
    }

    public int LD
    {
        get
        {
            return _d;
        }
        set
        {
            _d = value;
        }
    }
    #endregion
}