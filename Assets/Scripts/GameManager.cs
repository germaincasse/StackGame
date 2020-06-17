using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    public GameObject blockPrefab;
    public GameObject debritPrefab;
    public GameObject baseBlock;
    public GameObject cameraAnchor;
    private CameraMovement camMovement;
    private List<GameObject> stack;

    private GameObject newBlock;
    private int height = 0;
    private float swingStartTime;
    private float speed = 0.2f;
    
    public List<Color> colors;
    private Color currentColor;
    
    public float placementTolerance = 0.1f;

    public Text text;

    void Start()
    {
        newBlock = Instantiate(blockPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        currentColor = colors[0];
        ChangeBlockColor(newBlock);

        swingStartTime = Time.time;
        stack = new List<GameObject>();
        stack.Add(baseBlock);

        camMovement = cameraAnchor.GetComponent<CameraMovement>();
    }

    private bool gameOver = false;
    private bool swingsAlongX = true;

    void Update()
    {
        if (gameOver)
            return;

        if (Input.GetMouseButtonDown(0)) {
            PlaceBlock();
            camMovement.Move();
            text.text = height.ToString();
        }
        // horizontal movement
        float offset = Mathf.Cos((Time.time - swingStartTime) * (2 * Mathf.PI) * speed);
        if (swingsAlongX)
            newBlock.transform.position = new Vector3(offset * 6, height, newBlock.transform.position.z);
        else
            newBlock.transform.position = new Vector3(newBlock.transform.position.x, height, offset * 6);
    }


    void PlaceBlock() {
        swingStartTime = Time.time;
        GameObject top = stack[stack.Count - 1];

        // tolerance
        if (Mathf.Abs((top.transform.position + new Vector3(0, 1, 0)
            - newBlock.transform.position).magnitude) < placementTolerance)
            newBlock.transform.position = top.transform.position + new Vector3(0, 1, 0);

        Vector3 pos = newBlock.transform.position;      // position of the new block
        Vector3 scale = top.transform.localScale;       // scale of the top stack block = scale of the new block
        Vector3 topPos = top.transform.position;        // position of the top stack block

        float error;
        float newBlockScale;
        if (swingsAlongX) {
            error = pos.x - topPos.x;
            newBlockScale = scale.x - Mathf.Abs(error);
        } else {
            error = pos.z - topPos.z;
            newBlockScale = scale.z - Mathf.Abs(error);
        }
        if (newBlockScale <= 0)
            gameOver = true;
        
        CreateDebrit(pos, topPos, scale);

        if (gameOver) {
            Destroy(newBlock);
            return;
        }

        PlaceCutBlock(pos, topPos, scale, newBlockScale);
        AddBlockToStack();
        CreateNewBlock();
    }


    void CreateDebrit(Vector3 pos, Vector3 topPos, Vector3 scale) {

        if (topPos + new Vector3(0, 1, 0) == pos)
            return;     // perfect placement -> no debrit

        if (swingsAlongX) {
            float debritPosition = (pos.x + topPos.x + scale.x) / 2;
            if (pos.x < topPos.x)
                debritPosition -= scale.x;
            if (gameOver)
                debritPosition = pos.x;
            GameObject debrit = Instantiate(debritPrefab, new Vector3(debritPosition, height, pos.z), Quaternion.identity);
            float debritScale = Mathf.Abs(pos.x - topPos.x);
            if (gameOver)
                debritScale = scale.x;
            debrit.transform.localScale = new Vector3(debritScale, 1, scale.z);
            ChangeBlockColor(debrit);
        } else {
            float debritPosition = (pos.z + topPos.z + scale.z) / 2;
            if (pos.z < topPos.z)
                debritPosition -= scale.z;
            if (gameOver)
                debritPosition = pos.z;
            GameObject debrit = Instantiate(debritPrefab, new Vector3(pos.x, height, debritPosition), Quaternion.identity);
            float debritScale = Mathf.Abs(pos.z - topPos.z);
            if (gameOver)
                debritScale = scale.z;
            debrit.transform.localScale = new Vector3(scale.x, 1, debritScale);
            ChangeBlockColor(debrit);
        }
    }

    void PlaceCutBlock(Vector3 pos, Vector3 topPos, Vector3 scale, float newBlockScale) {

            if (swingsAlongX)
                newBlock.transform.localScale = new Vector3(newBlockScale, 1, scale.z);
            else
                newBlock.transform.localScale = new Vector3(scale.x, 1, newBlockScale);
            newBlock.transform.position = (pos + topPos + new Vector3(0, 1, 0)) / 2;
    }
    void AddBlockToStack() {

        stack.Add(newBlock);
        height++;
        if (stack.Count > 20) {
            Destroy(stack[0]);
            stack.RemoveAt(0);
        }
    }

    void ChangeColor() {
        int steps = 4;
        int index = (int)((float)(height + 1) / (float)steps) % colors.Count;
        int secondIndex = (index + 1) % colors.Count;
        float lerpValue = (float)((height + 1) % steps) / (float)steps;
        text.text = "index: " + index + " sIndex: " + secondIndex + " lerp: " + lerpValue; 
        currentColor = Color.Lerp(colors[index], colors[secondIndex], lerpValue);
    }

    void ChangeBlockColor(GameObject block) {
        block.GetComponent<Renderer>().material.color = currentColor;
    }

    void CreateNewBlock() {
        swingsAlongX = !swingsAlongX;
        if (swingsAlongX) {
            newBlock = Instantiate(blockPrefab, new Vector3(6, height, newBlock.transform.position.z), Quaternion.identity);
        } else {
            newBlock = Instantiate(blockPrefab, new Vector3(newBlock.transform.position.x, height, 6), Quaternion.identity);
        }
        newBlock.transform.localScale = stack[stack.Count - 1].transform.localScale;
        ChangeColor();
        ChangeBlockColor(newBlock);
    }

    void GameOver() {

    }
}
