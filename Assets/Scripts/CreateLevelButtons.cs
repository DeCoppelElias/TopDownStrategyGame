using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreateLevelButtons : MonoBehaviour
{
    [SerializeField]
    private GameObject levelButtonPrefab;

    public Image image;
    private void Start()
    {
        try
        {
            // OFFICIAL LEVELS
            // Finding scrollview
            GameObject officialLevelScrollView = GameObject.Find("OfficialLevelScrollView");

            // Reading data from resources into a dictionary, each entry is one level
            UnityEngine.Object[] officialLevelTxtFiles = Resources.LoadAll("Levels");
            Dictionary<string, string> OfficialLevelData = new Dictionary<string, string>();
            foreach (UnityEngine.Object txtFile in officialLevelTxtFiles)
            {
                TextAsset textAsset = (TextAsset)txtFile;
                OfficialLevelData.Add(textAsset.name, textAsset.text);
            }

            // Creating buttons for each level
            createLevelButtons(officialLevelScrollView, OfficialLevelData);


            // CUSTOM LEVELS
            // Finding scrollview
            GameObject customLevelScrollView = GameObject.Find("CustomLevelScrollView");

            // Reading data from persistent data path into a dictionary, each entry is one level
            Dictionary<string, string> customLevelData = new Dictionary<string, string>();
            DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath + "/Levels/");
            FileInfo[] fileInfo = info.GetFiles();
            foreach (FileInfo file in fileInfo)
            {
                StreamReader sr = file.OpenText();
                customLevelData.Add(file.Name, sr.ReadToEnd());
            }

            // Creating buttons for each level
            createLevelButtons(customLevelScrollView, customLevelData);
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }

    private void createLevelButtons(GameObject levelScrollView, Dictionary<string, string> allLevelData)
    {
        GameObject content = levelScrollView.transform.Find("Viewport").Find("Content").gameObject;

        LevelSelectScene manager = GameObject.Find("Canvas").GetComponent<LevelSelectScene>();

        foreach (KeyValuePair<string,string> levelData in allLevelData)
        {
            string name = levelData.Key;
            string levelString = levelData.Value;

            // Creating level button
            GameObject LevelButtonGameObject = Instantiate(levelButtonPrefab, content.transform);

            Button levelButton = LevelButtonGameObject.GetComponent<Button>();

            levelButton.onClick.AddListener(delegate { manager.selectLevel(name); });

            GameObject levelNameGameObject = LevelButtonGameObject.transform.Find("LevelName").gameObject;
            TextMeshProUGUI levelName = levelNameGameObject.GetComponent<TextMeshProUGUI>();
            levelName.text = name;

            // Displaying level image
            // Store file contents
            string[] s = levelString.Split("\n"[0]);
            List<string> contents = new List<string>(s);

            // Find png image
            int counter = 0;
            while (!contents[counter].StartsWith("PNG"))
            {
                counter++;
            }
            counter++;

            // Find width and height
            int width = int.Parse(contents[counter].Split(' ')[1]);
            counter++;
            int height = int.Parse(contents[counter].Split(' ')[1]);
            counter++;

            // Load bytes
            List<byte> bytesList = new List<byte>();
            foreach (string byteString in contents[counter].Split(' '))
            {
                if (byteString.Length > 0)
                {
                    bytesList.Add(byte.Parse(byteString));
                }
            }
            byte[] bytes = bytesList.ToArray();

            // Load data into the texture and upload it to the GPU.
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.LoadImage(bytes);
            // Upload texture data to the GPU, so the GPU renders the updated texture
            // Note: This method is costly, and you should call it only when you need to
            // If you do not intend to render the updated texture, there is no need to call this method at this point
            tex.Apply();

            // Assign texture to renderer's material.
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            LevelButtonGameObject.transform.Find("LevelImage").GetComponent<Image>().sprite = sprite;
        }
    }
}
