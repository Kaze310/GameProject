using UnityEngine;

[ExecuteAlways]
public class CameraFit : MonoBehaviour
{
    public BoardManager board;
    public float padding = 1f;

    void Update()
    {
        if (!board) return;
        var cam = GetComponent<Camera>();
        float boardW = board.width * board.tileSize;
        float boardH = board.height * board.tileSize;
        float aspect = cam.aspect;
        float size = Mathf.Max(boardH/2f + padding, (boardW/2f + padding)/aspect);
        cam.orthographicSize = size;
    }
}
