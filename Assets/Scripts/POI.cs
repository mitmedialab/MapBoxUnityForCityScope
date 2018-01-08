using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class POI : MonoBehaviour
{
    // http://www.theappguruz.com/blog/unity-csv-parsing-unity

    public TextAsset csvFile; // Reference of CSV file
    /*
    public InputField rollNoInputField;// Reference of rollno input field
    public InputField nameInputField; // Reference of name input filed
    */
    public Text contentArea; // Reference of contentArea where records are displayed

    private char lineSeperater = '\n'; // It defines line seperate character
    private char fieldSeperator = ','; // It defines field seperate chracter

    // RZ
    public float unityToLatLon = 37383.18379871f;
    public float latCenter = 42.508637f;
    public float lonCenter = 1.535879f;
    public float fTextLabelHeight = 100f;
    public GameObject map;
    public Sprite mySprite;
    public int latColumn = 1;
    public int lonColumn = 2;
    public int imgLnkColumn = 10;

    void Start()
    {
        
        StartCoroutine(LateStart(0.1f)); //second
        
    }

    IEnumerator LateStart(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        // add mesh colliders for the terrain panels
        for (int i = 0; i < map.transform.childCount; i++)
        {
            Transform child = map.transform.GetChild(i);
            MeshCollider mc = child.gameObject.AddComponent<MeshCollider>();
        }

        // read data to UI text
        readData();

        // read data and creat UI in right location
        string[] records = csvFile.text.Split(lineSeperater);
        foreach (string record in records)
        {
            // read each field of each line
            string[] fields = record.Split(fieldSeperator);

            // skip the first line
            if (fields[0] != "Name")
            {

                // first get right unity x,z location according to lat,lon
                float lat = float.Parse(fields[latColumn]);
                float lon = float.Parse(fields[lonColumn]);
                Debug.Log("lat: " + lat.ToString() + "; lon: " + lon.ToString() + "; ");

                float x = (lon - lonCenter) * unityToLatLon;
                float z = (lat - latCenter) * unityToLatLon;
                Debug.Log("x = " + x.ToString() + "; z = " + z.ToString() + "; ");

                float y = 5000f;
                RaycastHit hit;
                Debug.DrawRay(new Vector3(x,y,z), Vector3.up * -10000, Color.green);
                if (Physics.Raycast(new Vector3(x, y, z), -Vector3.up, out hit, 10000))
                {
                    Debug.LogWarning("HIT");
                    y = hit.point.y;
                }
                else
                {
                    y = 0f;
                }

                GameObject g = new GameObject();
                Canvas canvas = g.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                CanvasScaler cs = g.AddComponent<CanvasScaler>();
                cs.scaleFactor = 10.0f;
                cs.dynamicPixelsPerUnit = 10.0f;
                GraphicRaycaster gr = g.AddComponent<GraphicRaycaster>();
                g.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100.0f);
                g.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100.0f);
                GameObject g2 = new GameObject();
                g2.name = "Text";
                g2.transform.parent = g.transform;
                Text t = g2.AddComponent<Text>();
                g2.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100.0f);
                g2.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100.0f);
                t.alignment = TextAnchor.MiddleCenter;
                t.horizontalOverflow = HorizontalWrapMode.Overflow;
                t.verticalOverflow = VerticalWrapMode.Overflow;
                Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
                t.font = ArialFont;
                t.fontSize = 24;
                t.text = fields[0] + "\n|\n|\n|\n|\n|\n|";
                t.enabled = true;
                t.color = Color.white;

                StartCoroutine(AddImage(fields[imgLnkColumn], g, x, y, z, transform));

            }
        }
    }

    // Read data from CSV file
    private void readData()
    {
        string[] records = csvFile.text.Split(lineSeperater);
        foreach (string record in records)
        {
            string[] fields = record.Split(fieldSeperator);
            foreach (string field in fields)
            {
                contentArea.text += field + "\t";
            }
            contentArea.text += '\n';
        }
    }

    IEnumerator AddImage(string url, GameObject g, float x, float y, float z, Transform transformThis)
    {
        // load images from url
        using (WWW www = new WWW(url))
        {
            // Wait for download to complete
            yield return www;

            Texture2D tex = www.texture;
            mySprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        }

        // add image to POI GO
        GameObject g3 = new GameObject();
        g3.name = "Image";
        g3.transform.parent = g.transform;
        Image img = g3.AddComponent<Image>();
        g3.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 150.0f);
        g3.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100.0f);
        g3.GetComponent<RectTransform>().SetPositionAndRotation(new Vector3(1f, 15f, 1f), transform.rotation);
        img.sprite = mySprite;
        
        g.name = "Text Label";
        bool bWorldPosition = false;
        g.GetComponent<RectTransform>().SetParent(transformThis, bWorldPosition);

        g.transform.localPosition = new Vector3(x, y + fTextLabelHeight, z);
        g.transform.localScale = new Vector3(1.0f / transformThis.localScale.x * 1.0f, 1.0f / transformThis.localScale.y * 1.0f, 1.0f / transformThis.localScale.z * 1.0f);
        g.AddComponent<CameraFacingBillboard>();
    }

    /*
    // Add data to CSV file
    public void addData()
    {
        // Following line adds data to CSV file
        File.AppendAllText(getPath() + "/Assets/StudentData.csv", lineSeperater + rollNoInputField.text + fieldSeperator + nameInputField.text);
        // Following lines refresh the edotor and print data
        rollNoInputField.text = "";
        nameInputField.text = "";
        contentArea.text = "";
        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        #endif
        readData();
    }

    // Get path for given CSV file
    private static string getPath()
    {
    #if UNITY_EDITOR
        return Application.dataPath;
    #elif UNITY_ANDROID
    return Application.persistentDataPath;// +fileName;
    #elif UNITY_IPHONE
    return GetiPhoneDocumentsPath();// +"/"+fileName;
    #else
    return Application.dataPath;// +"/"+ fileName;
    #endif
    }
    // Get the path in iOS device
    private static string GetiPhoneDocumentsPath()
    {
        string path = Application.dataPath.Substring(0, Application.dataPath.Length - 5);
        path = path.Substring(0, path.LastIndexOf('/'));
        return path + "/Documents";
    }
    */

}
