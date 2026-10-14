using UnityEngine;

public class Connection1Manager : MonoBehaviour
{
    public static Connection1Manager Instance { get; private set; }
    public ConnectionConfiguration Config { get; private set; }

    private void Awake()
    {
        Instance = this;
        Config = ConnectionConfiguration.SeriesAiding();
    }

    public ConnectionType ExpectedType => ConnectionType.SeriesAiding;
}
