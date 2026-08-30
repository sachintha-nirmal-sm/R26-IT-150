using UnityEngine;

public class BatteryCellController : MonoBehaviour
{
    public static BatteryCellController Cell1 { get; private set; }
    public static BatteryCellController Cell2 { get; private set; }

    [SerializeField] private float cellVoltage = 1.5f;
    [SerializeField] private string cellID = "Cell1";
    [SerializeField] private ElectricalComponent component;

    public float CellVoltage => cellVoltage;
    public string CellID => cellID;
    public ElectricalComponent Component => component;
    public ElectricalTerminal PositiveTerminal => component != null ? component.GetTerminal("+") : null;
    public ElectricalTerminal NegativeTerminal => component != null ? component.GetTerminal("-") : null;
    public bool Flipped => component != null && component.Flipped;

    public void Bind(ElectricalComponent source, string id, float voltage)
    {
        component = source;
        cellID = id;
        cellVoltage = voltage;
        if (id == "Cell2") Cell2 = this;
        else Cell1 = this;
    }

    public void RotateCell()
    {
        component?.ToggleFlip();
    }
}
