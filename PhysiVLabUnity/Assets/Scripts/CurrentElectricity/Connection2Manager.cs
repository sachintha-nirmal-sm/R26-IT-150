using UnityEngine;

public class Connection2Manager : MonoBehaviour
{
    public static Connection2Manager Instance { get; private set; }
    public ConnectionConfiguration Config { get; private set; }

    private void Awake()
    {
        Instance = this;
        Config = ConnectionConfiguration.Parallel();
    }

    public ConnectionType ExpectedType => ConnectionType.Parallel;
}
