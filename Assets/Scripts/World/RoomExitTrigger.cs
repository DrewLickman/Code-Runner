using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RoomExitTrigger : MonoBehaviour
{
    [Header("Destination")]
    public string toRoomId;
    public string sceneNameOverride;

    [Header("Gates")]
    public string requiredAbilityId;
    public string requiredKeyfileId;

    private RoomLoader loader;
    private PlayerProgress progress;

    private void Awake()
    {
        loader = GameRoot.Instance != null ? GameRoot.Instance.GetComponent<RoomLoader>() : FindFirstObjectByType<RoomLoader>();
        progress = GameRoot.Instance != null ? GameRoot.Instance.GetComponent<PlayerProgress>() : FindFirstObjectByType<PlayerProgress>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (loader == null || progress == null) return;

        RoomExit exit = new RoomExit
        {
            toRoomId = toRoomId,
            requiredAbilityId = requiredAbilityId,
            requiredKeyfileId = requiredKeyfileId,
        };

        if (!GateRules.CanTraverse(exit, progress)) return;

        if (!string.IsNullOrWhiteSpace(sceneNameOverride))
        {
            loader.LoadRoomSceneName(sceneNameOverride, toRoomId);
        }
        else if (!string.IsNullOrWhiteSpace(toRoomId))
        {
            loader.LoadRoomById(toRoomId);
        }
    }
}

