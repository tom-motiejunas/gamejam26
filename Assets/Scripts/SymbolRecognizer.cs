using UnityEngine;

public static class SymbolRecognizer
{
    /// <summary>
    /// Compares a drawn texture against a list of target sprites.
    /// Returns the index of the best match, or -1 if no match exceeds threshold.
    /// </summary>
    public static int GetBestMatch(Texture2D drawing, Sprite[] targets, float threshold = 0.25f)
    {
        if (drawing == null || targets == null || targets.Length == 0) return -1;

        int bestIndex = -1;
        float bestScore = -1f;

        Color[] drawnPixels = drawing.GetPixels();
        Debug.Log($"Drawing Resolution: {drawing.width}x{drawing.height}. Checking against {targets.Length} targets.");

        for (int i = 0; i < targets.Length; i++)
        {
            Sprite target = targets[i];
            if (target == null)
            {
                Debug.LogError($"Target Faction {i} is NULL! Please assign the Sprite in GameController Inspector.");
                continue;
            }
            
            // Debug dimensions
            int targetW = (int)target.textureRect.width;
            int targetH = (int)target.textureRect.height;
            
            // Removed strict dimension check to support variable sprite sizes (29x29 etc)
            // We now scale the comparisons logically using UVs
            
            float score = Compare(drawnPixels, target, drawing.width, drawing.height);
            Debug.Log($"Faction {i} ({target.name}) Score: {score:F2}");

            if (score > bestScore)
            {
                bestScore = score;
                bestIndex = i;
            }
        }

        Debug.Log($"Best Match Index: {bestIndex}, Best Score: {bestScore:F2}, Threshold: {threshold}");

        if (bestScore >= threshold)
        {
            return bestIndex;
        }

        return -1;
    }

    private static float Compare(Color[] drawnPixels, Sprite target, int drawW, int drawH)
    {
        Color[] targetPixels;
        try
        {
            targetPixels = target.texture.GetPixels(
                (int)target.textureRect.x, 
                (int)target.textureRect.y, 
                (int)target.textureRect.width, 
                (int)target.textureRect.height
            );
        }
        catch (UnityException e)
        {
            Debug.LogError($"Cannot read sprite texture for {target.name}. Error: {e.Message}");
            return 0f;
        }

        int targetW = (int)target.textureRect.width;
        int targetH = (int)target.textureRect.height;

        int matches = 0;
        int totalSignificantPixels = 0;

        // Iterate over the TARGET's dimensions as the ground truth
        for (int y = 0; y < targetH; y++)
        {
            for (int x = 0; x < targetW; x++)
            {
                // 1. Get Target Pixel
                Color targetCol = targetPixels[y * targetW + x];
                bool targetActive = targetCol.a > 0.1f;

                // 2. Sample Drawn Pixel at relative position (UV mapping)
                // U = x / targetW, V = y / targetH
                // Map UV to Drawing coordinates
                
                int drawX = (int)((x / (float)targetW) * drawW);
                int drawY = (int)((y / (float)targetH) * drawH);
                
                // Clamp just in case
                drawX = Mathf.Clamp(drawX, 0, drawW - 1);
                drawY = Mathf.Clamp(drawY, 0, drawH - 1);

                Color drawnCol = drawnPixels[drawY * drawW + drawX];
                
                bool drawnActive = drawnCol.a > 0.1f; 
                if (drawnCol.grayscale > 0.1f) drawnActive = true;

                if (drawnActive || targetActive)
                {
                    totalSignificantPixels++;
                    if (drawnActive == targetActive)
                    {
                        matches++;
                    }
                }
            }
        }

        if (totalSignificantPixels == 0) return 0f;
        
        return (float)matches / totalSignificantPixels;
    }
}
