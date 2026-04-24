using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CreditsScroller : MonoBehaviour
{
    [Header("References")]
    public RectTransform creditsContainer;

    [Header("Prefabs")]
    public TextMeshProUGUI headerPrefab;
    public TextMeshProUGUI namePrefab;

    [Header("Scroll Settings")]
    public float autoScrollSpeed = 20f;
    public float fastScrollSpeed = 60f;

    [Header("Spacing")]
    public float headerSpacing = 30f;
    public float nameSpacing = 22f;

    // ── Credits Data ─────────────────────────────
    private struct CreditEntry { public string name, role; }

    private readonly List<CreditEntry> credits = new List<CreditEntry>
    {
        new CreditEntry { name = "Dedicated to Chuck Marschuetz", role = "Loving Father"},
        new CreditEntry { name = "Kai Stewart",            role = "Programming, Design & Art"         },
        new CreditEntry { name = "Zein Al-Bahrani",        role = "Programming"               },
        new CreditEntry { name = "Mark Paul",              role = "Programming, Level Design, Art"},
        new CreditEntry { name = "Tristan Meyer-Odell",    role = "Hardware & Design"         },
        new CreditEntry { name = "Lukas Marschuetz",       role = "Art"                       },
        new CreditEntry { name = "Jordan Davis",           role = "Art"                       },
        new CreditEntry { name = "Justin Valdez",          role = "Programming & Art"         },
        new CreditEntry { name = "Rochelle Suarez-Tapanes",role = "Art"                       },
        new CreditEntry { name = "Connie Hang",            role = "Art (Chud)"                },
        new CreditEntry { name = "Michael Yan",            role = "Art (Chud)"                },
        new CreditEntry { name = "Carl Jeong",             role = "Music & SFX"               },
        new CreditEntry { name = "Michael Barker",         role = "Music"                     },
        new CreditEntry { name = ":3",                     role = "BANANA BRICK!"                     },
    };

    private float totalHeight;
    private float startY;

    void OnEnable() { BuildCredits(); }

    void OnDisable()
    {
        foreach (Transform child in creditsContainer)
            Destroy(child.gameObject);
    }

    void Update()
    {
        if (creditsContainer == null) return;

        float speed = autoScrollSpeed;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            speed = fastScrollSpeed;
        else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            speed = -fastScrollSpeed;

        Vector2 pos = creditsContainer.anchoredPosition;
        pos.y += speed * Time.deltaTime;

        if (pos.y > totalHeight + 300f)
            pos.y = startY;

        creditsContainer.anchoredPosition = pos;
    }

    void BuildCredits()
    {
        foreach (Transform child in creditsContainer)
            Destroy(child.gameObject);

        float yPos = 0f;

        SpawnText(headerPrefab, "TEAM BANANA BRICK PRESENTS", ref yPos, headerSpacing, scale: 0.8f);
        SpawnText(headerPrefab, "ABYSSUM", ref yPos, headerSpacing, scale: 1.4f);

        yPos += 20f; // gap before names

        foreach (var entry in credits)
            SpawnText(namePrefab, $"{entry.name}   <size=70%><color=#aaaaaa>{entry.role}</color></size>", ref yPos, nameSpacing);

        totalHeight = yPos;
        startY = -600f;
        creditsContainer.anchoredPosition = new Vector2(0, startY);
    }

    void SpawnText(TextMeshProUGUI prefab, string text, ref float yPos, float spacing, float scale = 1f)
    {
        var obj = Instantiate(prefab, creditsContainer);
        obj.text = text;
        obj.fontSize *= scale;
        var rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, -yPos);
        yPos += spacing;
    }
}