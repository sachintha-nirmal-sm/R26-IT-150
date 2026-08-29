using UnityEngine;

public class Connection3Manager : MonoBehaviour
{
    public static Connection3Manager Instance { get; private set; }
    public ConnectionConfiguration Config { get; private set; }

    private void Awake()
    {
        Instance = this;
        Config = ConnectionConfiguration.SeriesOpposing();
    }

    public ConnectionType ExpectedType => ConnectionType.SeriesOpposing;
}
