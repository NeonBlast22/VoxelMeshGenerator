using UnityEngine;

public class PlayerT : MonoBehaviour
{
    [SerializeField] float normalSpeed;
    [SerializeField] float superSpeed;
    [SerializeField] float sensitivty;
    [SerializeField] LayerMask terrainLayer;

    [SerializeField] Voxel air;
    [SerializeField] Voxel stone;

    MapBuilder map;
    float yLook = 0f;

    private void Awake()
    {
        map = FindObjectOfType<MapBuilder>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        float speed = (Input.GetKey(KeyCode.LeftControl)) ?  superSpeed : normalSpeed;

        Vector3 input = Vector3.zero;
        input.x = Input.GetAxisRaw("Horizontal");
        input.z = Input.GetAxisRaw("Vertical");
        input.y = Input.GetAxisRaw("UpDown");

        transform.position += Vector3.up * input.y * speed * Time.deltaTime;
        transform.position += transform.forward * input.z * speed * Time.deltaTime;
        transform.position += transform.right * input.x * speed * Time.deltaTime;

        yLook -= Input.GetAxis("Mouse Y") * sensitivty;
        yLook = Mathf.Clamp(yLook, -90f, 90f);
        transform.localEulerAngles = new Vector3(yLook, transform.localEulerAngles.y, transform.localEulerAngles.z);

        float horizontal = Input.GetAxis("Mouse X");
        transform.Rotate(Vector3.up * horizontal * sensitivty, Space.World);

        if (Input.GetMouseButtonDown(0)) Break();
        if (Input.GetMouseButtonDown(1)) Place();
    }

    void Break()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo ,10f, terrainLayer))
        {
            Vector3 targetPos = hitInfo.point - (hitInfo.normal * 0.5f);
            Vector3Int voxelPos = new Vector3Int(Mathf.RoundToInt(targetPos.x), Mathf.RoundToInt(targetPos.y), Mathf.RoundToInt(targetPos.z));
            map.SetVoxel(voxelPos, air);
        }
    }
    void Place()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo, 10f, terrainLayer))
        {
            Vector3 targetPos = hitInfo.point + (hitInfo.normal * 0.5f);
            Vector3Int voxelPos = new Vector3Int(Mathf.RoundToInt(targetPos.x), Mathf.RoundToInt(targetPos.y), Mathf.RoundToInt(targetPos.z));
            map.SetVoxel(voxelPos, stone);
        }
    }
}
