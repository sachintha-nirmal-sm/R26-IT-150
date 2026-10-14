using UnityEngine;

public class ElectronicsCircuitComponent : MonoBehaviour
{
    [SerializeField] private string componentId;
    [SerializeField] private string inputTerminal = "In";
    [SerializeField] private string outputTerminal = "Out";
    [SerializeField] private bool placed;

    public string ComponentId => componentId;
    public string InputTerminal => inputTerminal;
    public string OutputTerminal => outputTerminal;
    public bool IsPlaced => placed;

    public void Configure(string id, string input, string output)
    {
        componentId = id;
        inputTerminal = input;
        outputTerminal = output;
        placed = true;
    }

    public void SetPlaced(bool value) => placed = value;
}
