using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VirusInside
{
    /// <summary>Весь интерфейс строится кодом: меню, HUD, записки, QTE, выбор финала.</summary>
    public class VirusUI : MonoBehaviour
    {
        VirusGame game;
        Canvas canvas;
        Font font;

        GameObject menuRoot, introRoot, hudRoot, noteRoot, qteRoot, choiceRoot;
        GameObject pauseRoot, endingRoot, deathRoot, controlsRoot;

        Text introBody, introCounter, introNextLabel;
        Text noteTitle, noteCounter, noteBody;
        Text healthText, crystalText, noteText, knifeText, objectiveText;
        Text promptText, toastText;
        Image healthFill, promptPanel, toastPanel;
        CanvasGroup toastGroup, promptGroup;

        Text qteCounter, qteTimerText;
        Image qteFill, qteTimerFill;
        RectTransform qteKeyRect;
        float qtePulse;

        Button choice1, choice2;
        Text choice2Title, choice2Desc;

        Text endingTitle, endingBody;
        Text deathBody;

        Image fadeImage, damageImage;
        Color flashColor;
        float flashAmount;
        Coroutine toastRoutine;
        Coroutine fadeRoutine;
        Coroutine introRoutine;

        int lastQtePresses = -1;

        // ================================================================ build
        public void Init(VirusGame g)
        {
            game = g;
            font = VirusArt.Font;
            BuildCanvas();
            BuildVignetteAndLayers();
            BuildMenu();
            BuildIntro();
            BuildHud();
            BuildNote();
            BuildQte();
            BuildChoice();
            BuildPause();
            BuildEnding();
            BuildDeath();
            BuildControls();
            BuildOverlays();
        }

        void BuildCanvas()
        {
            var go = new GameObject("VirusCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                es.transform.SetParent(transform, false);
            }
        }

        GameObject Root(string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return go;
        }

        void BuildVignetteAndLayers()
        {
            var vig = Img(canvas.transform, "Vignette", VirusArt.Vignette, Color.white, false, false);
            Stretch(vig.rectTransform);
            vig.preserveAspect = false;
        }

        void BuildMenu()
        {
            menuRoot = Root("Menu");
            var backdrop = Img(menuRoot.transform, "Backdrop", null, new Color(0.025f, 0.04f, 0.045f, 0.86f), true, false);
            Stretch(backdrop.rectTransform);

            var title = Label(menuRoot.transform, "Title", "ВИРУС ВНУТРИ", 104, VirusArt.Ivory, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 270f), new Vector2(1500f, 140f));
            title.fontStyle = FontStyle.Bold;
            var sh = title.gameObject.AddComponent<Shadow>();
            sh.effectColor = new Color(0.6f, 0.08f, 0.08f, 0.55f);
            sh.effectDistance = new Vector2(3f, -3f);

            Label(menuRoot.transform, "Sub", "ты — то, что ты помнишь", 28, VirusArt.Dim, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 168f), new Vector2(1200f, 46f));

            var line = Img(menuRoot.transform, "Line", null, new Color(0.42f, 0.90f, 0.83f, 0.35f), false, false);
            var lrt = line.rectTransform;
            lrt.anchorMin = lrt.anchorMax = new Vector2(0.5f, 0.5f);
            lrt.pivot = new Vector2(0.5f, 0.5f);
            lrt.anchoredPosition = new Vector2(0f, 120f);
            lrt.sizeDelta = new Vector2(520f, 2f);

            MakeButton(menuRoot.transform, "НАЧАТЬ", new Vector2(0f, 24f), new Vector2(420f, 68f),
                () => { AudioClick(); game.StartNewGame(); }, 30);
            MakeButton(menuRoot.transform, "УПРАВЛЕНИЕ", new Vector2(0f, -64f), new Vector2(420f, 60f),
                () => { AudioClick(); ShowControls(); }, 26);
            MakeButton(menuRoot.transform, "ВЫХОД", new Vector2(0f, -148f), new Vector2(420f, 60f),
                () => { AudioClick(); game.Quit(); }, 26);

            Label(menuRoot.transform, "Footer",
                "WASD — движение   ·   E — взаимодействие   ·   Esc — пауза",
                19, new Color(0.60f, 0.64f, 0.62f, 1f), TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 46f), new Vector2(1400f, 40f));
        }

        void BuildIntro()
        {
            introRoot = Root("Intro");
            var backdrop = Img(introRoot.transform, "Backdrop", null, new Color(0.02f, 0.03f, 0.035f, 0.94f), true, false);
            Stretch(backdrop.rectTransform);

            var panel = Panel(introRoot.transform, "Panel", new Vector2(0.5f, 0.5f), new Vector2(1180f, 650f),
                VirusArt.Panel);
            var inner = panel.transform;

            Label(inner, "Head", "ПРОБУЖДЕНИЕ", 20, VirusArt.Gold, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(48f, -44f), new Vector2(700f, 32f), new Vector2(0f, 1f));
            introCounter = Label(inner, "Counter", "1 / 3", 18, VirusArt.Dim, TextAnchor.MiddleRight,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-48f, -44f), new Vector2(300f, 32f), new Vector2(1f, 1f));

            var accent = Img(inner, "Accent", null, new Color(0.42f, 0.90f, 0.83f, 0.55f), false, false);
            accent.rectTransform.anchorMin = new Vector2(0f, 1f);
            accent.rectTransform.anchorMax = new Vector2(1f, 1f);
            accent.rectTransform.pivot = new Vector2(0.5f, 1f);
            accent.rectTransform.offsetMin = new Vector2(48f, -76f);
            accent.rectTransform.offsetMax = new Vector2(-48f, -73f);

            introBody = Label(inner, "Body", "", 29, VirusArt.Ivory, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(48f, -118f), new Vector2(0f, 360f));
            introBody.rectTransform.offsetMin = new Vector2(48f, 160f);
            introBody.rectTransform.offsetMax = new Vector2(-48f, -118f);
            introBody.lineSpacing = 1.3f;

            var hint = Label(inner, "Hint", "ПРОБЕЛ — продолжить", 18, VirusArt.Dim, TextAnchor.MiddleLeft,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(48f, 48f), new Vector2(600f, 30f), new Vector2(0f, 0f));

            var btn = MakeButton(inner, "ДАЛЕЕ", new Vector2(0f, 0f), new Vector2(220f, 58f),
                () => { AudioClick(); game.AdvanceIntro(); }, 24);
            var rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-48f, 38f);
            introNextLabel = btn.GetComponentInChildren<Text>();
        }

        void BuildHud()
        {
            hudRoot = Root("HUD");

            var hpPanel = Panel(hudRoot.transform, "Health", new Vector2(0f, 1f), new Vector2(392f, 124f),
                new Color(0.05f, 0.08f, 0.085f, 0.78f));
            var hpRt = hpPanel.rectTransform;
            hpRt.anchorMin = hpRt.anchorMax = new Vector2(0f, 1f);
            hpRt.pivot = new Vector2(0f, 1f);
            hpRt.anchoredPosition = new Vector2(30f, -30f);

            Label(hpPanel.transform, "Label", "ЗДОРОВЬЕ", 17, VirusArt.Dim, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(22f, -26f), new Vector2(220f, 24f), new Vector2(0f, 1f));

            var barBg = Img(hpPanel.transform, "BarBg", VirusArt.Rounded, new Color(0.02f, 0.035f, 0.038f, 0.95f), false, true);
            var bgRt = barBg.rectTransform;
            bgRt.anchorMin = bgRt.anchorMax = new Vector2(0f, 1f);
            bgRt.pivot = new Vector2(0f, 1f);
            bgRt.anchoredPosition = new Vector2(22f, -56f);
            bgRt.sizeDelta = new Vector2(300f, 24f);

            healthFill = Img(hpPanel.transform, "BarFill", null, VirusArt.Green, false, false);
            var fRt = healthFill.rectTransform;
            fRt.anchorMin = fRt.anchorMax = new Vector2(0f, 1f);
            fRt.pivot = new Vector2(0f, 1f);
            fRt.anchoredPosition = new Vector2(24f, -58f);
            fRt.sizeDelta = new Vector2(296f, 20f);
            healthFill.type = Image.Type.Filled;
            healthFill.fillMethod = Image.FillMethod.Horizontal;
            healthFill.fillOrigin = 0;

            healthText = Label(hpPanel.transform, "Value", "100 / 100", 17, VirusArt.Ivory, TextAnchor.MiddleRight,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-22f, -26f), new Vector2(160f, 24f), new Vector2(1f, 1f));

            var counters = Panel(hudRoot.transform, "Counters", new Vector2(0f, 1f), new Vector2(300f, 168f),
                new Color(0.05f, 0.08f, 0.085f, 0.78f));
            var cRt = counters.rectTransform;
            cRt.anchorMin = cRt.anchorMax = new Vector2(1f, 1f);
            cRt.pivot = new Vector2(1f, 1f);
            cRt.anchoredPosition = new Vector2(-30f, -30f);

            IconRow(counters.transform, 0f, VirusArt.CrystalIcon, out crystalText, "0 / 10");
            IconRow(counters.transform, -52f, VirusArt.NoteIcon, out noteText, "0 / 10");
            IconRow(counters.transform, -104f, VirusArt.Knife, out knifeText, "НЕТ");

            var objPanel = Panel(hudRoot.transform, "Objective", new Vector2(0f, 1f), new Vector2(760f, 72f),
                new Color(0.05f, 0.08f, 0.085f, 0.55f));
            var oRt = objPanel.rectTransform;
            oRt.anchorMin = oRt.anchorMax = new Vector2(0.5f, 1f);
            oRt.pivot = new Vector2(0.5f, 1f);
            oRt.anchoredPosition = new Vector2(0f, -24f);

            Label(objPanel.transform, "Tag", "ЦЕЛЬ", 15, VirusArt.Gold, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(22f, -22f), new Vector2(100f, 22f), new Vector2(0f, 1f));
            objectiveText = Label(objPanel.transform, "Text", "", 21, VirusArt.Ivory, TextAnchor.MiddleLeft,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(90f, 8f), new Vector2(-18f, -8f));
            objectiveText.verticalOverflow = VerticalWrapMode.Truncate;

            promptPanel = Img(hudRoot.transform, "PromptPanel", VirusArt.Rounded,
                new Color(0.04f, 0.07f, 0.075f, 0.92f), false, true);
            var pRt = promptPanel.rectTransform;
            pRt.anchorMin = pRt.anchorMax = new Vector2(0.5f, 0f);
            pRt.pivot = new Vector2(0.5f, 0f);
            pRt.anchoredPosition = new Vector2(0f, 96f);
            pRt.sizeDelta = new Vector2(860f, 56f);
            promptText = Label(promptPanel.transform, "Text", "", 22, VirusArt.Cyan, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, new Vector2(20f, 0f), new Vector2(-20f, 0f));
            promptGroup = promptPanel.gameObject.AddComponent<CanvasGroup>();
            promptPanel.gameObject.SetActive(false);

            toastPanel = Img(hudRoot.transform, "ToastPanel", VirusArt.Rounded,
                new Color(0.03f, 0.05f, 0.055f, 0.86f), false, true);
            var tRt = toastPanel.rectTransform;
            tRt.anchorMin = tRt.anchorMax = new Vector2(0.5f, 0f);
            tRt.pivot = new Vector2(0.5f, 0f);
            tRt.anchoredPosition = new Vector2(0f, 170f);
            tRt.sizeDelta = new Vector2(980f, 54f);
            toastText = Label(toastPanel.transform, "Text", "", 21, VirusArt.Gold, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, new Vector2(18f, 0f), new Vector2(-18f, 0f));
            toastGroup = toastPanel.gameObject.AddComponent<CanvasGroup>();
            toastPanel.gameObject.SetActive(false);
        }

        void IconRow(Transform parent, float y, Sprite icon, out Text text, string initial)
        {
            var img = Img(parent, "Icon", icon, Color.white, false, false);
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(20f, -18f + y);
            rt.sizeDelta = new Vector2(34f, 34f);
            img.preserveAspect = true;

            text = Label(parent, "Count", initial, 21, VirusArt.Ivory, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(66f, -18f + y), new Vector2(220f, 34f), new Vector2(0f, 1f));
        }

        void BuildNote()
        {
            noteRoot = Root("Note");
            var backdrop = Img(noteRoot.transform, "Backdrop", null, new Color(0.01f, 0.015f, 0.018f, 0.90f), true, false);
            Stretch(backdrop.rectTransform);

            var panel = Panel(noteRoot.transform, "Panel", new Vector2(0.5f, 0.5f), new Vector2(1140f, 730f),
                new Color(0.055f, 0.075f, 0.08f, 0.98f));
            var inner = panel.transform;

            Label(inner, "Tag", "АРХИВ ПАМЯТИ", 18, VirusArt.Gold, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(48f, -40f), new Vector2(500f, 26f), new Vector2(0f, 1f));
            noteCounter = Label(inner, "Counter", "03 / 10", 18, VirusArt.Dim, TextAnchor.MiddleRight,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-48f, -40f), new Vector2(300f, 26f), new Vector2(1f, 1f));

            var line = Img(inner, "Line", null, new Color(0.42f, 0.90f, 0.83f, 0.4f), false, false);
            line.rectTransform.anchorMin = new Vector2(0f, 1f);
            line.rectTransform.anchorMax = new Vector2(1f, 1f);
            line.rectTransform.pivot = new Vector2(0.5f, 1f);
            line.rectTransform.offsetMin = new Vector2(48f, -70f);
            line.rectTransform.offsetMax = new Vector2(-48f, -68f);

            noteTitle = Label(inner, "Title", "", 38, VirusArt.Ivory, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(48f, -116f), new Vector2(-48f, -80f));
            noteTitle.fontStyle = FontStyle.Bold;

            noteBody = Label(inner, "Body", "", 25, new Color(0.86f, 0.85f, 0.80f, 1f), TextAnchor.UpperLeft,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(48f, 76f), new Vector2(-48f, -164f));
            noteBody.lineSpacing = 1.22f;
            noteBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            noteBody.verticalOverflow = VerticalWrapMode.Overflow;

            Label(inner, "Hint", "ПРОБЕЛ / E — закрыть", 18, VirusArt.Dim, TextAnchor.MiddleLeft,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(48f, 30f), new Vector2(600f, 28f), new Vector2(0f, 0f));
        }

        void BuildQte()
        {
            qteRoot = Root("QTE");
            var backdrop = Img(qteRoot.transform, "Backdrop", null, new Color(0.16f, 0.01f, 0.015f, 0.82f), true, false);
            Stretch(backdrop.rectTransform);

            var panel = Panel(qteRoot.transform, "Panel", new Vector2(0.5f, 0.5f), new Vector2(920f, 540f),
                new Color(0.10f, 0.03f, 0.035f, 0.97f));
            var inner = panel.transform;

            var border = Img(inner, "Border", VirusArt.Rounded, new Color(0.8f, 0.22f, 0.22f, 0.35f), false, true);
            Stretch(border.rectTransform, 8f, 8f, 8f, 8f);

            Label(inner, "Title", "ТЕБЯ СХВАТИЛИ ЗА ГОРЛО", 40, new Color(1f, 0.42f, 0.38f, 1f),
                TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -70f), new Vector2(840f, 60f), new Vector2(0.5f, 1f));
            Label(inner, "Sub", "ЖМИ, ЧТОБЫ ВЫРВАТЬСЯ", 20, new Color(0.85f, 0.70f, 0.68f, 1f),
                TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -120f), new Vector2(700f, 34f), new Vector2(0.5f, 1f));

            var key = Panel(inner, "Key", new Vector2(0.5f, 0.5f), new Vector2(270f, 90f),
                new Color(0.16f, 0.06f, 0.065f, 1f));
            qteKeyRect = key.rectTransform;
            qteKeyRect.anchoredPosition = new Vector2(0f, 34f);
            Label(key.transform, "KeyText", "ПРОБЕЛ", 36, VirusArt.Ivory, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            qteCounter = Label(inner, "Counter", "0 / 12", 30, VirusArt.Ivory, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -54f), new Vector2(500f, 44f));

            var barBg = Img(inner, "BarBg", VirusArt.Rounded, new Color(0.03f, 0.02f, 0.02f, 1f), false, true);
            barBg.rectTransform.anchorMin = barBg.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            barBg.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            barBg.rectTransform.anchoredPosition = new Vector2(0f, -110f);
            barBg.rectTransform.sizeDelta = new Vector2(640f, 26f);

            qteFill = Img(inner, "Fill", null, new Color(1f, 0.42f, 0.38f, 1f), false, false);
            qteFill.rectTransform.anchorMin = qteFill.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            qteFill.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            qteFill.rectTransform.anchoredPosition = new Vector2(0f, -110f);
            qteFill.rectTransform.sizeDelta = new Vector2(632f, 18f);
            qteFill.type = Image.Type.Filled;
            qteFill.fillMethod = Image.FillMethod.Horizontal;
            qteFill.fillOrigin = 0;

            var timerBg = Img(inner, "TimerBg", null, new Color(0.03f, 0.02f, 0.02f, 1f), false, false);
            timerBg.rectTransform.anchorMin = timerBg.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            timerBg.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            timerBg.rectTransform.anchoredPosition = new Vector2(0f, -150f);
            timerBg.rectTransform.sizeDelta = new Vector2(640f, 8f);

            qteTimerFill = Img(inner, "TimerFill", null, new Color(0.85f, 0.65f, 0.35f, 1f), false, false);
            qteTimerFill.rectTransform.anchorMin = qteTimerFill.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            qteTimerFill.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            qteTimerFill.rectTransform.anchoredPosition = new Vector2(0f, -150f);
            qteTimerFill.rectTransform.sizeDelta = new Vector2(640f, 8f);
            qteTimerFill.type = Image.Type.Filled;
            qteTimerFill.fillMethod = Image.FillMethod.Horizontal;
            qteTimerFill.fillOrigin = 0;

            qteTimerText = Label(inner, "Timer", "", 17, VirusArt.Dim, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(700f, 26f), new Vector2(0.5f, 0f));
        }

        void BuildChoice()
        {
            choiceRoot = Root("Choice");
            var backdrop = Img(choiceRoot.transform, "Backdrop", null, new Color(0.01f, 0.02f, 0.022f, 0.92f), true, false);
            Stretch(backdrop.rectTransform);

            var panel = Panel(choiceRoot.transform, "Panel", new Vector2(0.5f, 0.5f), new Vector2(1220f, 780f),
                new Color(0.045f, 0.065f, 0.07f, 0.98f));
            var inner = panel.transform;

            Label(inner, "Tag", "АЛТАРЬ ПРОБУДИЛСЯ", 18, VirusArt.Gold, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(700f, 28f), new Vector2(0.5f, 1f));
            Label(inner, "Title", "ВЫБЕРИ, КЕМ ТЫ СТАНЕШЬ", 38, VirusArt.Ivory, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -98f), new Vector2(1100f, 52f), new Vector2(0.5f, 1f));
            Label(inner, "Body",
                "Кристаллы дали тебе силу. Записки дали тебе знание. Осталось решить, что делать с этим знанием.",
                22, VirusArt.Dim, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(1060f, 60f), new Vector2(0.5f, 1f));

            choice1 = MakeChoice(inner, new Vector2(0f, -30f), "1 · ЗАНЯТЬ ТЕЛО СТРАЖА",
                "Убить монстрика и войти в его оболочку. Вирус продолжит жить.", VirusArt.Gold,
                () => { AudioClick(); game.ChooseEnding(0); });
            choice2 = MakeChoice(inner, new Vector2(0f, -200f), "2 · УНИЧТОЖИТЬ СЕБЯ",
                "Взять ритуальный нож и стереть вирус. Мальчик проснётся.", VirusArt.Cyan,
                () => { AudioClick(); game.ChooseEnding(1); });
            choice2Title = choice2.transform.Find("Title")?.GetComponent<Text>();
            choice2Desc = choice2.transform.Find("Desc")?.GetComponent<Text>();

            Label(inner, "Hint", "1 / 2 — выбрать   ·   Esc — отойти от алтаря", 17, VirusArt.Dim,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 26f), new Vector2(900f, 28f), new Vector2(0.5f, 0f));
        }

        Button MakeChoice(Transform parent, Vector2 pos, string title, string desc, Color accent, UnityAction action)
        {
            var img = Img(parent, "Choice_" + title, VirusArt.Rounded, new Color(0.08f, 0.11f, 0.115f, 1f), true, true);
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(1030f, 150f);

            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.78f, 0.92f, 0.90f, 1f);
            cb.pressedColor = new Color(0.62f, 0.78f, 0.76f, 1f);
            cb.disabledColor = new Color(0.35f, 0.35f, 0.35f, 0.6f);
            btn.colors = cb;
            btn.navigation = new Navigation { mode = Navigation.Mode.None };
            btn.onClick.AddListener(action);

            var t = Label(img.transform, "Title", title, 29, accent, TextAnchor.MiddleLeft,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(38f, 52f), new Vector2(-38f, -12f));
            t.fontStyle = FontStyle.Bold;

            var d = Label(img.transform, "Desc", desc, 19, new Color(0.72f, 0.74f, 0.71f, 1f), TextAnchor.MiddleLeft,
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(38f, 16f), new Vector2(-38f, 46f));
            return btn;
        }

        void BuildPause()
        {
            pauseRoot = Root("Pause");
            var backdrop = Img(pauseRoot.transform, "Backdrop", null, new Color(0.01f, 0.02f, 0.022f, 0.86f), true, false);
            Stretch(backdrop.rectTransform);

            var panel = Panel(pauseRoot.transform, "Panel", new Vector2(0.5f, 0.5f), new Vector2(840f, 680f),
                new Color(0.05f, 0.07f, 0.075f, 0.98f));
            var inner = panel.transform;

            Label(inner, "Title", "ПАУЗА", 46, VirusArt.Ivory, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -62f), new Vector2(600f, 56f), new Vector2(0.5f, 1f));
            var body = Label(inner, "Controls", NoteData.ControlsText, 22, VirusArt.Dim, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(90f, -140f), new Vector2(-90f, -320f));
            body.lineSpacing = 1.35f;

            MakeButton(inner, "ПРОДОЛЖИТЬ", new Vector2(0f, -50f), new Vector2(360f, 58f),
                () => { AudioClick(); game.Resume(); }, 24);
            MakeButton(inner, "ЗАНОВО", new Vector2(0f, -128f), new Vector2(360f, 58f),
                () => { AudioClick(); game.RestartRun(); }, 24);
            MakeButton(inner, "В ГЛАВНОЕ МЕНЮ", new Vector2(0f, -206f), new Vector2(360f, 58f),
                () => { AudioClick(); game.ReturnToMenu(); }, 22);
        }

        void BuildEnding()
        {
            endingRoot = Root("Ending");
            var backdrop = Img(endingRoot.transform, "Backdrop", null, new Color(0.02f, 0.01f, 0.012f, 0.98f), true, false);
            Stretch(backdrop.rectTransform);

            endingTitle = Label(endingRoot.transform, "Title", "", 58, VirusArt.Gold, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 300f), new Vector2(1400f, 90f));
            endingTitle.fontStyle = FontStyle.Bold;

            var line = Img(endingRoot.transform, "Line", null, new Color(0.42f, 0.90f, 0.83f, 0.45f), false, false);
            line.rectTransform.anchorMin = line.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            line.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            line.rectTransform.anchoredPosition = new Vector2(0f, 246f);
            line.rectTransform.sizeDelta = new Vector2(700f, 2f);

            endingBody = Label(endingRoot.transform, "Body", "", 25, VirusArt.Ivory, TextAnchor.UpperCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(1160f, 380f));
            endingBody.lineSpacing = 1.3f;

            MakeButton(endingRoot.transform, "ЗАНОВО", new Vector2(-160f, -320f), new Vector2(280f, 60f),
                () => { AudioClick(); game.RestartRun(); }, 24);
            MakeButton(endingRoot.transform, "В МЕНЮ", new Vector2(160f, -320f), new Vector2(280f, 60f),
                () => { AudioClick(); game.ReturnToMenu(); }, 24);
        }

        void BuildDeath()
        {
            deathRoot = Root("Death");
            var backdrop = Img(deathRoot.transform, "Backdrop", null, new Color(0.09f, 0.01f, 0.012f, 0.98f), true, false);
            Stretch(backdrop.rectTransform);

            var title = Label(deathRoot.transform, "Title", "ТЫ УНИЧТОЖЕН", 66, new Color(0.90f, 0.20f, 0.20f, 1f),
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 180f), new Vector2(1300f, 100f));
            title.fontStyle = FontStyle.Bold;

            deathBody = Label(deathRoot.transform, "Body", NoteData.DeathText, 25, new Color(0.84f, 0.78f, 0.75f, 1f),
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 30f), new Vector2(1000f, 200f));
            deathBody.lineSpacing = 1.3f;

            MakeButton(deathRoot.transform, "ЗАНОВО", new Vector2(-160f, -260f), new Vector2(280f, 60f),
                () => { AudioClick(); game.RestartRun(); }, 24);
            MakeButton(deathRoot.transform, "В МЕНЮ", new Vector2(160f, -260f), new Vector2(280f, 60f),
                () => { AudioClick(); game.ReturnToMenu(); }, 24);
        }

        void BuildControls()
        {
            controlsRoot = Root("Controls");
            var backdrop = Img(controlsRoot.transform, "Backdrop", null, new Color(0.01f, 0.02f, 0.022f, 0.93f), true, false);
            Stretch(backdrop.rectTransform);

            var panel = Panel(controlsRoot.transform, "Panel", new Vector2(0.5f, 0.5f), new Vector2(960f, 720f),
                new Color(0.05f, 0.07f, 0.075f, 0.98f));
            var inner = panel.transform;

            Label(inner, "Title", "УПРАВЛЕНИЕ", 38, VirusArt.Ivory, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -52f), new Vector2(700f, 44f), new Vector2(0.5f, 1f));

            var body = Label(inner, "Controls", NoteData.ControlsText, 24, VirusArt.Dim, TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(90f, -130f), new Vector2(-90f, -300f));
            body.lineSpacing = 1.3f;

            Label(inner, "Mechanics",
                "Здоровье: 100. Хватка монстра отнимает треть, неудачная попытка — ещё треть.\n" +
                "Аптечки лечат две трети здоровья. Ищите их по всей пустоши.\n" +
                "Кристаллы и записки спрятаны в пяти домах. В каждом доме — своя 3D-комната.\n" +
                "Соберите 10 кристаллов, чтобы разбудить алтарь. Все 10 записок откроют второй финал.",
                20, new Color(0.72f, 0.74f, 0.71f, 1f), TextAnchor.UpperLeft,
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(90f, 120f), new Vector2(-90f, 240f));

            MakeButton(inner, "НАЗАД", new Vector2(0f, 0f), new Vector2(260f, 56f),
                () => { AudioClick(); HideControls(); }, 22).GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 42f);
        }

        void BuildOverlays()
        {
            damageImage = Img(canvas.transform, "Damage", null, new Color(0.7f, 0.05f, 0.05f, 0f), false, false);
            Stretch(damageImage.rectTransform);

            fadeImage = Img(canvas.transform, "Fade", null, new Color(0f, 0f, 0f, 0f), false, false);
            Stretch(fadeImage.rectTransform);
            fadeImage.raycastTarget = false;
        }

        // ================================================================ widgets
        Image Img(Transform parent, string name, Sprite sprite, Color color, bool raycast, bool sliced)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = raycast;
            if (sliced && sprite != null) img.type = Image.Type.Sliced;
            return img;
        }

        Image Panel(Transform parent, string name, Vector2 anchor, Vector2 size, Color color)
        {
            var img = Img(parent, name, VirusArt.Rounded, color, false, true);
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = size;
            var outline = img.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.35f, 0.75f, 0.70f, 0.22f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            return img;
        }

        Text Label(Transform parent, string name, string value, int size, Color color, TextAnchor align,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Vector2? pivot = null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.font = font;
            t.text = value;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            var rt = t.rectTransform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            if (pivot.HasValue) rt.pivot = pivot.Value;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            return t;
        }

        void Stretch(RectTransform rt, float left = 0f, float bottom = 0f, float right = 0f, float top = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        Button MakeButton(Transform parent, string label, Vector2 pos, Vector2 size, UnityAction action, int fontSize)
        {
            var img = Img(parent, "Btn_" + label, VirusArt.Rounded, new Color(0.09f, 0.145f, 0.145f, 0.97f), true, true);
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var outline = img.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.42f, 0.90f, 0.83f, 0.30f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.70f, 0.90f, 0.88f, 1f);
            cb.pressedColor = new Color(0.55f, 0.72f, 0.70f, 1f);
            btn.colors = cb;
            btn.navigation = new Navigation { mode = Navigation.Mode.None };
            btn.onClick.AddListener(action);

            var text = Label(img.transform, "Text", label, fontSize, VirusArt.Ivory, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, new Vector2(10f, 0f), new Vector2(-10f, 0f));
            text.fontStyle = FontStyle.Bold;
            return btn;
        }

        void AudioClick()
        {
            if (game != null && game.Audio != null) game.Audio.Click();
        }

        // ================================================================ public api
        public void ShowMenu()
        {
            HideAllRoots();
            menuRoot.SetActive(true);
            hudRoot.SetActive(false);
        }

        public void HideMenu() { menuRoot.SetActive(false); }

        public void ShowIntro(int page)
        {
            HideAllRoots();
            introRoot.SetActive(true);
            hudRoot.SetActive(false);
            introBody.text = NoteData.IntroPages[Mathf.Clamp(page, 0, NoteData.IntroPages.Length - 1)];
            introCounter.text = (page + 1) + " / " + NoteData.IntroPages.Length;
            introNextLabel.text = page >= NoteData.IntroPages.Length - 1 ? "НАЧАТЬ" : "ДАЛЕЕ";
        }

        public void HideIntro() { introRoot.SetActive(false); }

        public void SetHudVisible(bool visible) { hudRoot.SetActive(visible); }

        public void UpdateHealth(float health)
        {
            if (healthFill == null) return;
            float ratio = Mathf.Clamp01(health / VirusGame.MaxHealth);
            healthFill.fillAmount = ratio;
            healthFill.color = Color.Lerp(new Color(0.85f, 0.20f, 0.18f), new Color(0.35f, 0.85f, 0.48f), ratio);
            if (healthText != null) healthText.text = Mathf.CeilToInt(health) + " / 100";
        }

        public void UpdateCounters(float health, int crystals, int notes, bool knife)
        {
            if (crystalText != null) crystalText.text = crystals + " / 10";
            if (noteText != null) noteText.text = notes + " / 10";
            if (knifeText != null)
            {
                knifeText.text = knife ? "ЕСТЬ" : "НЕТ";
                knifeText.color = knife ? VirusArt.Cyan : VirusArt.Dim;
            }
            UpdateHealth(health);
        }

        public void SetObjective(string text)
        {
            if (objectiveText != null) objectiveText.text = text;
        }

        public void SetPrompt(string text)
        {
            if (promptPanel == null) return;
            if (string.IsNullOrEmpty(text))
            {
                promptPanel.gameObject.SetActive(false);
                return;
            }
            promptPanel.gameObject.SetActive(true);
            promptText.text = text;
        }

        public void Toast(string text)
        {
            if (toastPanel == null) return;
            if (string.IsNullOrEmpty(text)) return;
            toastText.text = text;
            toastPanel.gameObject.SetActive(true);
            toastGroup.alpha = 1f;
            if (toastRoutine != null) StopCoroutine(toastRoutine);
            toastRoutine = StartCoroutine(ToastRoutine());
        }

        IEnumerator ToastRoutine()
        {
            yield return new WaitForSecondsRealtime(2.6f);
            float a = 1f;
            while (a > 0.01f)
            {
                a -= Time.unscaledDeltaTime * 2.4f;
                toastGroup.alpha = Mathf.Clamp01(a);
                yield return null;
            }
            toastPanel.gameObject.SetActive(false);
        }

        public void ShowNote(int index)
        {
            index = Mathf.Clamp(index, 0, NoteData.NoteCount - 1);
            noteRoot.SetActive(true);
            noteTitle.text = NoteData.Titles[index];
            noteBody.text = NoteData.Bodies[index];
            noteCounter.text = (index + 1).ToString("00") + " / 10";
            noteBody.rectTransform.anchoredPosition = Vector2.zero;
        }

        public void HideNote() { noteRoot.SetActive(false); }

        public void ShowQTE(int need)
        {
            qteRoot.SetActive(true);
            qteFill.fillAmount = 0f;
            qteTimerFill.fillAmount = 1f;
            qteCounter.text = "0 / " + need;
            qteTimerText.text = "";
            lastQtePresses = -1;
            qtePulse = 0f;
        }

        public void UpdateQTE(int presses, int need)
        {
            if (qteCounter != null) qteCounter.text = presses + " / " + need;
            if (qteFill != null) qteFill.fillAmount = Mathf.Clamp01((float)presses / Mathf.Max(1, need));
            if (presses != lastQtePresses)
            {
                lastQtePresses = presses;
                qtePulse = 1f;
            }
        }

        public void UpdateQTETimer(float remaining)
        {
            if (qteTimerFill != null) qteTimerFill.fillAmount = Mathf.Clamp01(remaining);
            if (qteTimerText != null)
                qteTimerText.text = remaining > 0.01f ? "ОСТАЛОСЬ " + Mathf.CeilToInt(remaining * 6f) + " СЕК" : "ПОЗДНО...";
        }

        public void HideQTE() { qteRoot.SetActive(false); }

        public void ShowChoices(bool canSacrifice, int notes, bool knife)
        {
            choiceRoot.SetActive(true);
            if (choice2 != null)
            {
                choice2.interactable = canSacrifice;
                var cb = choice2.colors;
                cb.normalColor = canSacrifice ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.7f);
                choice2.colors = cb;
            }
            if (choice2Title != null)
                choice2Title.color = canSacrifice ? VirusArt.Cyan : new Color(0.5f, 0.5f, 0.5f, 1f);
            if (choice2Desc != null)
            {
                if (canSacrifice)
                    choice2Desc.text = "Взять ритуальный нож и стереть вирус. Мальчик проснётся.";
                else
                    choice2Desc.text = "ЗАКРЫТО · нужно " + notes + "/10 записок" + (knife ? "" : " и ритуальный нож");
                choice2Desc.color = canSacrifice ? new Color(0.72f, 0.74f, 0.71f, 1f)
                                                 : new Color(0.55f, 0.35f, 0.35f, 1f);
            }
        }

        public void HideChoices() { choiceRoot.SetActive(false); }

        public void ShowPause() { pauseRoot.SetActive(true); }
        public void HidePause() { pauseRoot.SetActive(false); }

        public void ShowControls()
        {
            controlsRoot.SetActive(true);
            menuRoot.SetActive(false);
        }

        public void HideControls()
        {
            controlsRoot.SetActive(false);
            menuRoot.SetActive(true);
        }

        public void ShowEnding(string title, string body, bool good)
        {
            HideAllRoots();
            endingRoot.SetActive(true);
            hudRoot.SetActive(false);
            endingTitle.text = title;
            endingTitle.color = good ? VirusArt.Cyan : new Color(0.85f, 0.28f, 0.22f, 1f);
            endingBody.text = body;
        }

        public void ShowDeath()
        {
            HideAllRoots();
            hudRoot.SetActive(false);
            qteRoot.SetActive(false);
            deathRoot.SetActive(true);
            deathBody.text = NoteData.DeathText;
        }

        public void HideAllModals()
        {
            noteRoot.SetActive(false);
            qteRoot.SetActive(false);
            choiceRoot.SetActive(false);
            pauseRoot.SetActive(false);
        }

        void HideAllRoots()
        {
            menuRoot.SetActive(false);
            introRoot.SetActive(false);
            hudRoot.SetActive(false);
            noteRoot.SetActive(false);
            qteRoot.SetActive(false);
            choiceRoot.SetActive(false);
            pauseRoot.SetActive(false);
            endingRoot.SetActive(false);
            deathRoot.SetActive(false);
            controlsRoot.SetActive(false);
        }

        public void FadeTo(float alpha, float duration)
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeRoutine(alpha, duration));
        }

        IEnumerator FadeRoutine(float target, float duration)
        {
            float start = fadeImage.color.a;
            float t = 0f;
            if (duration <= 0.001f)
            {
                fadeImage.color = new Color(0f, 0f, 0f, target);
                yield break;
            }
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float a = Mathf.Lerp(start, target, Mathf.Clamp01(t / duration));
                fadeImage.color = new Color(0f, 0f, 0f, a);
                yield return null;
            }
            fadeImage.color = new Color(0f, 0f, 0f, target);
        }

        public void FlashDamage()
        {
            flashColor = new Color(0.75f, 0.03f, 0.03f);
            flashAmount = 1f;
        }

        public void FlashHeal()
        {
            flashColor = new Color(0.10f, 0.65f, 0.30f);
            flashAmount = 0.55f;
        }

        void Update()
        {
            if (flashAmount > 0f)
            {
                flashAmount = Mathf.Max(0f, flashAmount - Time.unscaledDeltaTime * 2.4f);
                damageImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, flashAmount * 0.42f);
            }

            if (qtePulse > 0f)
            {
                qtePulse = Mathf.Max(0f, qtePulse - Time.unscaledDeltaTime * 5f);
                if (qteKeyRect != null)
                    qteKeyRect.localScale = Vector3.one * (1f + qtePulse * 0.14f);
            }
        }
    }
}
