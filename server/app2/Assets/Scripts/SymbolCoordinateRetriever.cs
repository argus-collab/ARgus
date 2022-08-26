using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SymbolCoordinateRetriever : MonoBehaviour
{
    private SymbolsRandomPlacement symbolsPlacer;

    public Image symbolX;
    public Image symbolY;

    public Sprite defaultSprite;

    private void Start()
    {
        ResetSprites();
    }

    void Update()
    {
        if (symbolsPlacer != null)
        {
            symbolX.sprite = symbolsPlacer.GetSpriteCoordX();
            symbolY.sprite = symbolsPlacer.GetSpriteCoordY();
        }
        else
        {
            symbolsPlacer = GameObject.FindObjectOfType<SymbolsRandomPlacement>();
            ResetSprites();
        }
    }

    void ResetSprites()
    {
        symbolX.sprite = defaultSprite;
        symbolY.sprite = defaultSprite;
    }
}
