using UnityEngine;

[System.Serializable]
public class ConnectionConfiguration
{
    [SerializeField] private ConnectionType connectionType = ConnectionType.SeriesAiding;
    [SerializeField] private string title = "CONNECTION 1";
    [SerializeField] private string instruction = "Connect the two dry cells in series aiding as shown in the textbook reference.";
    [SerializeField] private string arrangementName = "Series aiding";

    public ConnectionType Type => connectionType;
    public string Title => title;
    public string Instruction => instruction;
    public string ArrangementName => arrangementName;

    public static ConnectionConfiguration SeriesAiding()
    {
        return new ConnectionConfiguration
        {
            connectionType = ConnectionType.SeriesAiding,
            title = "CONNECTION 1",
            instruction = "Connect the two dry cells in series aiding as shown in the textbook reference. The positive of one cell must join the negative of the other so the voltages add.",
            arrangementName = "Series aiding"
        };
    }

    public static ConnectionConfiguration Parallel()
    {
        return new ConnectionConfiguration
        {
            connectionType = ConnectionType.Parallel,
            title = "CONNECTION 2",
            instruction = "Connect the two dry cells in parallel as shown in the textbook reference. Both positive terminals must join together and both negative terminals must join together.",
            arrangementName = "Parallel"
        };
    }

    public static ConnectionConfiguration SeriesOpposing()
    {
        return new ConnectionConfiguration
        {
            connectionType = ConnectionType.SeriesOpposing,
            title = "CONNECTION 3",
            instruction = "Connect the two dry cells in series opposing as shown in the textbook reference. Arrange the polarities so the cell voltages oppose each other.",
            arrangementName = "Series opposing"
        };
    }
}
