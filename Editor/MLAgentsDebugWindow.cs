#if UNITY_EDITOR

using System;
using System.Linq;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class MLAgentsDebugWindow : EditorWindow
{
    #region ### Classes ###

    /// <summary>
    /// スタイルの定義
    /// </summary>
    private static class Styles
    {
        /// <summary>
        /// タブ用のトグル
        /// </summary>
        private static GUIContent[] tabToggles = null;

        public static GUIContent[] TabToggles
        {
            get
            {
                if (tabToggles == null)
                {
                    tabToggles = System.Enum.GetNames(typeof(Tab)).Select(x => new GUIContent(x)).ToArray();
                }
                return tabToggles;
            }
        }

        public static readonly GUIStyle TabButtonStyle = "LargeButton";

        /// <summary>
        /// タブボタンの大きさ
        /// </summary>
        public static readonly GUI.ToolbarButtonSize TabButtonSize = GUI.ToolbarButtonSize.Fixed;
    }

    public static class CustomUI
    {
        public static bool Foldout(string title, bool display)
        {
            var style = new GUIStyle("ShurikenModuleTitle");
            style.font = new GUIStyle(EditorStyles.label).font;
            style.border = new RectOffset(15, 7, 4, 4);
            style.fixedHeight = 22;
            style.contentOffset = new Vector2(20f, -2f);

            var rect = GUILayoutUtility.GetRect(16f, 22f, style);
            GUI.Box(rect, title, style);

            var e = Event.current;

            var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            if (e.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
            }

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                display = !display;
                e.Use();
            }

            return display;
        }
    }

    #endregion ### Classes ###

    #region ### Enums ###

    enum Tab
    {
        Setup,
        TFModel,
        Status,
        Manual,
    }

    #endregion ### Enums ###

    #region ### Parameters ###

    [Header("GUI")]
    static Tab tab = Tab.Setup;
    static Vector2 scrollPos = Vector2.zero;
    static float lineSpace = 10;
    static string str;
    static bool isOpenTfModelPath = true;
    static bool isOpenTfmodelList = true;
    static bool isOpenLink = false;
    static bool isOpenHowTo = false;

    GUIStyle styleLabel;

    [Header("Path")]
    static string pathTfModelDirectory = "C:/Users/CS1/Documents/ml-agents/results";
    static string[] tfModelFiles;
    static string[] tfModelDirectories;

    #endregion ### Parameters ###

    #region ### Methods ###

    private void Awake()
    {
        styleLabel = new GUIStyle(EditorStyles.label);
        styleLabel.wordWrap = true;
    }

    #region ### Methods / MenuItems ###

    [MenuItem("Tools/ML-Agents/Open Debug Window")]
    private static void OpenSetupWindow()
    {
        GetWindow<MLAgentsDebugWindow>("ML-Agents");
    }

    #endregion ### Methods / MenuItems ###

    #region ### GUI ###

    void OnGUI()
    {
        // タブの描画
        tab = (Tab)GUILayout.Toolbar((int)tab, Styles.TabToggles, Styles.TabButtonStyle, Styles.TabButtonSize, GUILayout.Height(32));

        // 仕切り
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(2));
        
        // ここから下の要素をスクロール対象にする
        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        // タブごとに表示内容を変える
        switch (tab)
        {
            case Tab.Setup:
                ShowSetUp();
                break;
            case Tab.TFModel:
                ShowTFModel();
                break;
            case Tab.Status:
                
                break;
            case Tab.Manual:
                ShowManual();
                break;
            

        }

        // ここまでの要素をスクロール対象にする
        GUILayout.EndScrollView();

    }

    #endregion ### GUI ###

    void ShowSetUp()
    {

    }

    void ShowTFModel()
    {
        isOpenTfModelPath = CustomUI.Foldout("Path", isOpenTfModelPath);
        if (isOpenTfModelPath)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label($"path:");
                GUILayout.Box($"{pathTfModelDirectory}", GUILayout.ExpandWidth(true));

                // TFModelが保存されているパスを開く
                if (GUILayout.Button("Open"))
                {
                    System.Diagnostics.Process.Start(pathTfModelDirectory);
                }
            }
            GUILayout.EndHorizontal();
        }

        isOpenTfmodelList = CustomUI.Foldout("TF Models", isOpenTfmodelList);
        if (isOpenTfmodelList)
        {
            tfModelDirectories = System.IO.Directory.GetDirectories(pathTfModelDirectory, "*", System.IO.SearchOption.TopDirectoryOnly);

            // ヘッダー
            GUILayout.BeginHorizontal("box");
            {
                Color bufcolor = GUI.color;
                GUI.color = Color.gray;

                GUILayout.Label("Onnx File", GUILayout.Width(120 - 4), GUILayout.Height(20));

                // 仕切り
                GUILayout.Box("", GUILayout.Width(1), GUILayout.Height(20));

                GUILayout.Label("Path", GUILayout.Width(180), GUILayout.Height(20));

                // 仕切り
                GUILayout.Box("", GUILayout.Width(1), GUILayout.Height(20));

                GUILayout.Label(" ", GUILayout.Width(50), GUILayout.Height(20));

                GUI.color = bufcolor;
            }
            GUILayout.EndHorizontal();

            for (int i = 0; i < tfModelDirectories.Length; i++)
            {
                GUILayout.BeginHorizontal();
                {
                    // onnxファイルを表示
                    string[] files = System.IO.Directory.GetFileSystemEntries(tfModelDirectories[i], @"*.onnx");

                    if (files.Length > 0)
                    {
                        foreach (string s in files)
                        {
                            GUILayout.Label($"{System.IO.Path.GetFileNameWithoutExtension(s)}", GUILayout.Width(120), GUILayout.Height(20));
                        }
                    }
                    else
                    {
                        GUILayout.Label($"--", GUILayout.Width(120), GUILayout.Height(20));
                    }

                    // 仕切り
                    GUILayout.Box("", GUILayout.Width(1), GUILayout.Height(20));

                    // ディレクトリを表示
                    GUILayout.Label($"{System.IO.Path.GetFileName(tfModelDirectories[i])}{System.IO.Path.GetExtension(tfModelDirectories[i])}", GUILayout.Width(180), GUILayout.Height(20));

                    // 仕切り
                    GUILayout.Box("", GUILayout.Width(1), GUILayout.Height(20));

                    // TFModelが保存されているパスを開く
                    if (GUILayout.Button("Open", GUILayout.MaxWidth(50), GUILayout.Height(20)))
                    {
                        System.Diagnostics.Process.Start(tfModelDirectories[i]);
                    }
                }
                GUILayout.EndHorizontal();
                
            }

        }

    }

    void ShowManual()
    {
        // 公式ドキュメント等のリンクの表示
        isOpenLink = CustomUI.Foldout("Link", isOpenLink);
        if (isOpenLink)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("APIドキュメント");
                if (GUILayout.Button("開く", GUILayout.MaxWidth(60)))
                {
                    Application.OpenURL("https://docs.unity3d.com/Packages/com.unity.ml-agents@1.4/api/Unity.MLAgents.Actuators.ActionBuffers.html");
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Github");
                if (GUILayout.Button("開く", GUILayout.MaxWidth(60)))
                {
                    Application.OpenURL("https://github.com/Unity-Technologies/ml-agents/blob/release_12_docs/docs/Installation.md");
                }
            }
            GUILayout.EndHorizontal();
        }

        // 使い方の表示
        isOpenHowTo = CustomUI.Foldout("How to", isOpenHowTo);
        if (isOpenHowTo)
        {
            GUILayout.Box("学習を行う", GUILayout.ExpandWidth(true));

            str = "１．AnacondaPromptを開く\n";
            str += "\n";
            str += "２．「Activate ml-agents」と入力し、ML-Agentsを起動\n";
            str += "\n";
            str += "３．「cd」コマンドで「ml-agents-master」フォルダに移動\n";
            str += "　　・Unityプロジェクトファイルの\n";
            str += "　　　一つ上のディレクトリにある\n";
            str += "\n";
            str += "４．「mlagents-learn config/ppo/〇〇.yaml \n";
            str += "　　--run-id=<<任意のID名>>」と入力\n";
            str += "　　・任意のID名：例）3DBallRunFirst\n";
            str += "\n";
            str += "５．Unityエディタ側でPlayボタンを押して、学習を開始する\n";

            GUILayout.Label(str);

            // 仕切り
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(2));
            GUILayout.Space(lineSpace);


            GUILayout.Box("学習済みモデルの適用", GUILayout.ExpandWidth(true));
            
            str = "";

            str += "１．「ml-agents/results」を開く\n";
            str += "　　・学習後に生成される\n";
            str += "　　　推論モデルの形式が変更\n";
            str += "　　　＜従来＞nnファイル　→　＜現在＞onnxファイル\n";
            str += "\n";
            str += "２．BehaviorParameterで設定した\n";
            str += "　　BehaviorNameと同じ名前のnnファイルを探す\n";
            str += "　　・デフォルトでは「firstRun-0」という\n";
            str += "　　　フォルダに入っている\n";
            str += "\n";
            str += "３．nnファイルをUnityプロジェクトファイル\n";
            str += "　　（ml-agents-master/UnitySDK/Assets）内の\n";
            str += "　　どこかに入れる\n";
            str += "　　・コピーがおすすめ";
            str += "\n";
            str += "４．Inspectorタブ内でnnモデルファイルを\n";
            str += "　　BehaviorParameterの「Model」パラメータに入れる\n";
            str += "\n";
            str += "５．Unityエディタ側でPlayボタンを押して\n";
            str += "　　実行する";

            GUILayout.Label(str);
            

            // 仕切り
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(2));
            GUILayout.Space(lineSpace);


            GUILayout.Box("学習状況の確認", GUILayout.ExpandWidth(true));

            str = "";



            GUILayout.Label(str);
        }
        
    }

    void ShowSettings()
    {

    }

    #endregion ### Methods ###

}

#endif
