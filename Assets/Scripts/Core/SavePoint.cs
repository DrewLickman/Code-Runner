using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SavePoint : MonoBehaviour
{
    [Tooltip("Optional override. If empty, uses current room id.")]
    public string saveRoomId;

    public bool saveOnEnter = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!saveOnEnter) return;
        if (!collision.CompareTag("Player")) return;

        PlayerProgress progress = GameRoot.Instance != null ? GameRoot.Instance.GetComponent<PlayerProgress>() : FindFirstObjectByType<PlayerProgress>();
        SaveSystem save = GameRoot.Instance != null ? GameRoot.Instance.GetComponent<SaveSystem>() : FindFirstObjectByType<SaveSystem>();

        if (progress == null || save == null) return;

        string roomId = !string.IsNullOrWhiteSpace(saveRoomId) ? saveRoomId : progress.Data.currentRoomId;
        progress.Data.lastSaveRoomId = roomId;
        save.Save();
    }
}

