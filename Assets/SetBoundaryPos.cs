using UnityEngine;

public enum Boundary
{
    Floor,
    LeftWall,
    RightWall,
    Ceiling,
}
public class SetBoundaryPos : MonoBehaviour
{
    [SerializeField]
    private Boundary _boundary;
    private BoxCollider2D _col;
    private float _floorOffset = 0.07f;
    private void Start()
    {
        _col = GetComponent<BoxCollider2D>();
        switch (_boundary)
        {
            case Boundary.Floor:
                _col.offset = new Vector2(_col.offset.x, (-Camera.main.orthographicSize - _col.size.y / 2) + _floorOffset);
                break;
            case Boundary.LeftWall:
                _col.offset = new Vector2(-Camera.main.orthographicSize * Camera.main.aspect - _col.size.x / 2, _col.offset.y);
                break;
            case Boundary.RightWall:
                _col.offset = new Vector2(Camera.main.orthographicSize * Camera.main.aspect + _col.size.x / 2, _col.offset.y);
                break;
            case Boundary.Ceiling:
                _col.offset = new Vector2(_col.offset.x, Camera.main.orthographicSize + _col.size.y / 2);
                break;
        }
    }
}
