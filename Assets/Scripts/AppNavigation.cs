using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FestiveSpaces
{
    /// <summary>Builds the complete lightweight UI at runtime so the generated scene needs no hand-authored canvases.</summary>
    public class AppNavigation : MonoBehaviour
    {
        public ARPlacementController placement;
        public DecorationManipulator manipulator;
        public ScreenshotService screenshots;
        Canvas canvas;
        RectTransform body;
        Text statusText;
        Festival festival;
        DecorationDefinition[] items;
        int currentItem;
        GameObject currentTemplate;
        readonly List<string> saved = new List<string>();
        readonly Color ink = new Color(.12f,.12f,.22f);
        readonly Color cream = new Color(1f,.97f,.91f);
        GraphicRaycaster raycaster;
        Sprite roundedSprite;
        public bool IsARMode { get; private set; }

        void Start()
        {
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.Portrait;
            BuildCanvas();
            if(screenshots!=null) screenshots.uiCanvas=canvas;
            string screenshotDirectory=System.IO.Path.Combine(Application.persistentDataPath,"Screenshots");
            if(System.IO.Directory.Exists(screenshotDirectory))
            {
                saved.AddRange(System.IO.Directory.GetFiles(screenshotDirectory,"*.png"));
                saved.Sort((a,b)=>System.IO.File.GetLastWriteTime(b).CompareTo(System.IO.File.GetLastWriteTime(a)));
            }
            ShowHome();
            if (screenshots != null) screenshots.Completed += OnScreenshot;
        }

        void BuildCanvas()
        {
            var go = new GameObject("Festive Spaces UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20; raycaster = go.GetComponent<GraphicRaycaster>();
            roundedSprite = MakeRoundedSprite();
            var scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920); scaler.matchWidthOrHeight = .5f;
            body = MakeRect("Body", canvas.transform, new Vector2(0,0), new Vector2(1,1));
        }

        public bool IsPointerOverUI(Vector2 position)
        {
            if (raycaster == null || EventSystem.current == null) return false;
            var data = new PointerEventData(EventSystem.current) { position = position };
            var results = new List<RaycastResult>(); raycaster.Raycast(data, results); return results.Count > 0;
        }

        public void SetStatus(string text) { if (statusText != null) statusText.text = text; }
        void Clear() { for (int i=body.childCount-1;i>=0;i--) Destroy(body.GetChild(i).gameObject); statusText = null; }

        void ShowHome()
        {
            IsARMode=false;
            Clear(); Backdrop(new Color(.16f,.12f,.27f));
            Label("Festive Spaces", .075f, new Vector2(.5f,.71f), cream, FontStyle.Bold);
            Label("Transform your space,\ncelebrate every season.", .038f, new Vector2(.5f,.62f), new Color(1f,.8f,.55f));
            Label("✦  ✧  ✦  ✧  ✦", .07f, new Vector2(.5f,.83f), new Color(1f,.62f,.2f));
            Button("Start Decorating", new Vector2(.5f,.43f), new Vector2(.78f,.075f), new Color(1f,.55f,.17f), ShowFestivals);
            Button("Saved Designs", new Vector2(.5f,.32f), new Vector2(.68f,.065f), new Color(.36f,.28f,.53f), ShowSaved);
        }

        void ShowFestivals()
        {
            IsARMode=false;
            Clear(); Backdrop(cream); Label("Choose your celebration", .05f, new Vector2(.5f,.88f), ink, FontStyle.Bold);
            Festival[] choices = { Festival.Diwali, Festival.Christmas, Festival.Halloween };
            string[] copy = { "Warm lights & rangoli", "Evergreen & sparkle", "Pumpkins & friendly fright" };
            for (int i=0;i<choices.Length;i++)
            {
                Festival f = choices[i]; float y=.69f-i*.22f;
                ButtonCard(FestivalCatalog.Title(f)+"\n"+copy[i], new Vector2(.5f,y), FestivalCatalog.Accent(f), ()=>OpenCatalog(f));
            }
            Button("Back", new Vector2(.5f,.12f), new Vector2(.36f,.055f), ink, ShowHome);
        }

        void OpenCatalog(Festival selectedFestival)
        {
            if (placement != null) placement.ClearPlacedObjects();
            if (currentTemplate != null) Destroy(currentTemplate);
            currentTemplate=null;
            if (placement != null) placement.SetSelectedObject(null);
            festival = selectedFestival; items = FestivalCatalog.Items(festival); currentItem=0;
            ShowCatalog();
        }

        void ShowCatalog()
        {
            IsARMode=false;
            Clear(); Backdrop(cream); Label(FestivalCatalog.Title(festival)+" decorations", .05f, new Vector2(.5f,.9f), ink, FontStyle.Bold);
            Label("Choose an item, then enter your space", .027f, new Vector2(.5f,.84f), ink);
            var scrollGo = new GameObject("Decoration list", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scrollGo.transform.SetParent(body,false); var sr=scrollGo.GetComponent<RectTransform>(); sr.anchorMin=new Vector2(.08f,.22f); sr.anchorMax=new Vector2(.92f,.8f); sr.offsetMin=sr.offsetMax=Vector2.zero;
            scrollGo.GetComponent<Image>().color = new Color(1,1,1,.32f);
            var viewport=MakeRect("Viewport",scrollGo.transform,new Vector2(0,0),new Vector2(1,1)); viewport.gameObject.AddComponent<RectMask2D>();
            var contentGo=new GameObject("Content",typeof(RectTransform),typeof(VerticalLayoutGroup),typeof(ContentSizeFitter)); contentGo.transform.SetParent(viewport,false);
            var content=contentGo.GetComponent<RectTransform>(); content.anchorMin=new Vector2(0,1); content.anchorMax=new Vector2(1,1); content.pivot=new Vector2(.5f,1); content.sizeDelta=new Vector2(0,0);
            var layout=contentGo.GetComponent<VerticalLayoutGroup>(); layout.padding=new RectOffset(14,14,14,14); layout.spacing=12; layout.childForceExpandHeight=false; layout.childForceExpandWidth=true;
            contentGo.GetComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            sr.GetComponent<ScrollRect>().viewport=viewport; sr.GetComponent<ScrollRect>().content=content; sr.GetComponent<ScrollRect>().vertical=true; sr.GetComponent<ScrollRect>().horizontal=false;
            for (int i=0;i<items.Length;i++)
            {
                int index=i; var row=Button(items[i].displayName, content, new Vector2(.5f,.5f), new Vector2(0,100), FestivalCatalog.Accent(festival), ()=>SelectItem(index));
                row.GetComponent<LayoutElement>().preferredHeight=108;
                var iconRect=MakeRect("Preview swatch",row.transform,new Vector2(.07f,.5f),new Vector2(.07f,.5f)); iconRect.sizeDelta=new Vector2(54,54);
                var icon=iconRect.gameObject.AddComponent<Image>(); icon.sprite=roundedSprite; icon.type=Image.Type.Sliced; icon.color=items[i].color;
            }
            statusText=Label(currentTemplate==null?"Select a decoration to use in AR.":"Selected: "+items[currentItem].displayName,.022f,new Vector2(.5f,.185f),ink);
            Button("Back", new Vector2(.27f,.12f), new Vector2(.37f,.06f), ink, ShowFestivals);
            Button("Enter AR", new Vector2(.72f,.12f), new Vector2(.43f,.06f), FestivalCatalog.Accent(festival), EnterAR);
        }

        void SelectItem(int index)
        {
            currentItem=index;
            if (currentTemplate != null) Destroy(currentTemplate);
            currentTemplate=FestivalCatalog.CreateDecoration(items[index]); currentTemplate.SetActive(false);
            currentTemplate.hideFlags=HideFlags.HideAndDontSave;
            if (placement != null) placement.SetSelectedObject(currentTemplate);
            SetStatus(items[index].displayName+" selected");
        }

        void EnterAR()
        {
            IsARMode=true;
            if (items == null || items.Length==0) return;
            if (placement != null) { if (!placement.HasSelectedObject) SelectItem(currentItem); placement.placementEnabled=true; }
            if (EventSystem.current == null) gameObject.AddComponent<EventSystem>();
            Clear();
            Label("Scan a surface", .04f, new Vector2(.5f,.94f), Color.white, FontStyle.Bold);
            statusText=Label("Move your phone slowly to scan your surroundings.", .024f, new Vector2(.5f,.885f), Color.white);
            Button("Switch decor", new Vector2(.5f,.13f), new Vector2(.42f,.06f), FestivalCatalog.Accent(festival), CycleItem);
            Button("↻ Rotate", new Vector2(.22f,.22f), new Vector2(.34f,.06f), ink, ()=>manipulator.RotateSelected());
            Button("＋ Resize", new Vector2(.60f,.22f), new Vector2(.34f,.06f), ink, ()=>manipulator.ResizeSelected());
            Button("Delete", new Vector2(.22f,.31f), new Vector2(.34f,.06f), new Color(.72f,.2f,.22f), ()=>manipulator.DeleteSelected());
            Button("Done", new Vector2(.70f,.31f), new Vector2(.34f,.06f), new Color(.15f,.57f,.36f), ShowPreview);
        }

        void CycleItem()
        {
            currentItem=(currentItem+1)%items.Length;
            if (currentTemplate != null) Destroy(currentTemplate);
            currentTemplate=FestivalCatalog.CreateDecoration(items[currentItem]); currentTemplate.SetActive(false);
            currentTemplate.hideFlags=HideFlags.HideAndDontSave;
            if (placement != null) placement.SetSelectedObject(currentTemplate);
            SetStatus(items[currentItem].displayName+" selected. Tap a scanned surface to place it.");
        }

        void ShowPreview()
        {
            IsARMode=false;
            if (placement != null) { placement.placementEnabled=false; placement.ClearMarker(); }
            Clear();
            Label("Your festive space",.05f,new Vector2(.5f,.94f),Color.white,FontStyle.Bold);
            Label("Tap capture to save a photo to your gallery.",.026f,new Vector2(.5f,.88f),Color.white);
            Button("Capture screenshot",new Vector2(.5f,.17f),new Vector2(.72f,.065f),FestivalCatalog.Accent(festival),()=>screenshots.Capture());
            Button("Choose another festival",new Vector2(.5f,.085f),new Vector2(.72f,.06f),ink,ShowFestivals);
            statusText=Label("",.022f,new Vector2(.5f,.25f),Color.white);
        }

        void OnScreenshot(string location)
        {
            if(statusText!=null) statusText.text="Saved to your device.";
            string directory=System.IO.Path.Combine(Application.persistentDataPath,"Screenshots");
            if(System.IO.Directory.Exists(directory)) { saved.Clear(); saved.AddRange(System.IO.Directory.GetFiles(directory,"*.png")); saved.Sort((a,b)=>System.IO.File.GetLastWriteTime(b).CompareTo(System.IO.File.GetLastWriteTime(a))); }
        }

        void ShowSaved()
        {
            IsARMode=false;
            Clear(); Backdrop(cream); Label("Saved designs",.055f,new Vector2(.5f,.88f),ink,FontStyle.Bold);
            Label(saved.Count==0?"Your captured designs will appear here.":"Saved on this device: "+saved.Count,.03f,new Vector2(.5f,.77f),ink);
            for(int i=0;i<Mathf.Min(saved.Count,7);i++)
                Label(System.IO.Path.GetFileName(saved[i]),.019f,new Vector2(.5f,.68f-i*.055f),ink);
            Button("Back",new Vector2(.5f,.14f),new Vector2(.38f,.06f),ink,ShowHome);
        }

        void Backdrop(Color color)
        {
            var bg=MakeRect("Background",body,new Vector2(0,0),new Vector2(1,1)); var image=bg.gameObject.AddComponent<Image>(); image.color=color; bg.SetAsFirstSibling();
        }

        Text Label(string value,float normalizedSize,Vector2 anchor,Color color,FontStyle style=FontStyle.Normal)
        {
            var rect=MakeRect(value,body,anchor,anchor); rect.sizeDelta=new Vector2(960,220); rect.anchoredPosition=Vector2.zero;
            var t=rect.gameObject.AddComponent<Text>(); t.text=value; t.font=Resources.GetBuiltinResource<Font>("Arial.ttf"); t.fontSize=Mathf.RoundToInt(Screen.height*normalizedSize); t.alignment=TextAnchor.MiddleCenter; t.color=color; t.fontStyle=style; t.horizontalOverflow=HorizontalWrapMode.Wrap; t.verticalOverflow=VerticalWrapMode.Overflow; return t;
        }

        void Button(string label,Vector2 anchor,Vector2 size,Color color,UnityEngine.Events.UnityAction action) => Button(label,body,anchor,size,color,action);
        Button Button(string label,Transform parent,Vector2 anchor,Vector2 size,Color color,UnityEngine.Events.UnityAction action)
        {
            var go=new GameObject(label.Replace(" "," "),typeof(RectTransform),typeof(Image),typeof(Button),typeof(LayoutElement)); go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>(); rect.anchorMin=rect.anchorMax=anchor; rect.sizeDelta=size; rect.anchoredPosition=Vector2.zero;
            var image=go.GetComponent<Image>(); image.sprite=roundedSprite; image.type=Image.Type.Sliced; image.color=color; var button=go.GetComponent<Button>(); button.targetGraphic=image; button.onClick.AddListener(action);
            var textGo=new GameObject("Label",typeof(RectTransform),typeof(Text)); textGo.transform.SetParent(go.transform,false); var tr=textGo.GetComponent<RectTransform>(); tr.anchorMin=Vector2.zero;tr.anchorMax=Vector2.one;tr.offsetMin=tr.offsetMax=Vector2.zero;
            var text=textGo.GetComponent<Text>(); text.text=label;text.font=Resources.GetBuiltinResource<Font>("Arial.ttf");text.fontSize=Mathf.RoundToInt(Screen.height*.027f);text.alignment=TextAnchor.MiddleCenter;text.color=Color.white;text.fontStyle=FontStyle.Bold;
            return button;
        }

        void ButtonCard(string label,Vector2 anchor,Color color,UnityEngine.Events.UnityAction action)
        {
            var button=Button(label,anchor,new Vector2(.82f,.17f),color,action);
        }

        Sprite MakeRoundedSprite()
        {
            const int size=64, radius=15;
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,false); texture.name="Rounded UI sprite"; texture.wrapMode=TextureWrapMode.Clamp;
            for(int y=0;y<size;y++) for(int x=0;x<size;x++)
            {
                int cx=x<radius?radius-1-x:(x>=size-radius?x-(size-radius):0);
                int cy=y<radius?radius-1-y:(y>=size-radius?y-(size-radius):0);
                bool corner=(x<radius||x>=size-radius)&&(y<radius||y>=size-radius);
                texture.SetPixel(x,y,!corner||(cx*cx+cy*cy<=radius*radius)?Color.white:new Color(1,1,1,0));
            }
            texture.Apply(); return Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(20,20,20,20));
        }
    }
}
