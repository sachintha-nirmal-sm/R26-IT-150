using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElectronicsCircuitBoardManager : MonoBehaviour
{
    public static ElectronicsCircuitBoardManager Instance { get; private set; }

    private GameObject boardRoot;
    private TextMeshProUGUI statusLabel;
    private Image glowOverlay;

    public bool BoardPlaced { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(GameObject board, TextMeshProUGUI status, Image glow)
    {
        boardRoot = board;
        statusLabel = status;
        glowOverlay = glow;
        Refresh();
    }

    public void SetBoardPlaced(bool placed)
    {
        BoardPlaced = placed;
        Refresh();
    }

    public void ResetBoard()
    {
        BoardPlaced = false;
        Refresh();
    }

    public void Refresh()
    {
        if (statusLabel != null)
            statusLabel.text = ElectronicsCircuitConnectionManager.Instance != null
                ? ElectronicsCircuitConnectionManager.Instance.NextHint()
                : "Place the circuit board, then add the battery, switch, diode, bulb and wires.";
        if (glowOverlay != null)
        {
            bool glow = ElectronicsCircuitConnectionManager.Instance != null &&
                        ElectronicsCircuitConnectionManager.Instance.IsCircuitComplete() &&
                        ElectronicsSwitchController.Instance != null &&
                        ElectronicsSwitchController.Instance.IsOn() &&
                        ElectronicsCircuitConnectionManager.Instance.IsForwardBias();
            glowOverlay.enabled = glow;
            glowOverlay.color = glow ? new Color(1f, 0.92f, 0.35f, 0.22f) : Color.clear;
        }
    }
}
