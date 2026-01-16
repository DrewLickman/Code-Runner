using UnityEngine;

public class RoomMarker : MonoBehaviour
{
    [Tooltip("RoomData asset that defines this room.")]
    public RoomData room;

    private void Start()
    {
        if (room == null) return;

        PlayerProgress progress = GameRoot.Instance != null
            ? GameRoot.Instance.GetComponent<PlayerProgress>()
            : FindFirstObjectByType<PlayerProgress>();

        progress?.DiscoverRoom(room.roomId);
        if (progress != null) progress.Data.currentRoomId = room.roomId;
    }
}

