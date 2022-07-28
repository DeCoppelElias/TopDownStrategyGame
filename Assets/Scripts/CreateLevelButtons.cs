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
            DirectoryInfo info = new DirectoryInfo(Application.dataPath + "/Levels/");
            FileInfo[] fileInfo = info.GetFiles();

            GameObject levelScrollView = GameObject.Find("LevelScrollView");
            GameObject content = levelScrollView.transform.Find("Viewport").Find("Content").gameObject;

            MultiplayerSceneUi manager = GameObject.Find("Canvas").GetComponent<MultiplayerSceneUi>();

            foreach (FileInfo file in fileInfo)
            {
                // Check if txt file
                if (file.Name.EndsWith(".txt"))
                {
                    // Creating level button
                    GameObject LevelButtonGameObject = Instantiate(levelButtonPrefab, content.transform);

                    Button levelButton = LevelButtonGameObject.GetComponent<Button>();

                    levelButton.onClick.AddListener(delegate { manager.selectLevel(file.Name); });

                    GameObject levelNameGameObject = LevelButtonGameObject.transform.Find("LevelName").gameObject;
                    TextMeshProUGUI levelName = levelNameGameObject.GetComponent<TextMeshProUGUI>();
                    levelName.text = file.Name;

                    // Displaying level image
                    // Store file contents
                    StreamReader sr = file.OpenText();
                    List<string> contents = new List<string>();
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        contents.Add(s);
                    }

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
                    foreach(string byteString in contents[counter].Split(' '))
                    {
                        if(byteString.Length > 0)
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
        catch(Exception e)
        {
            Debug.Log(e);
        }
    }
}
