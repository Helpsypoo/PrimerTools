using System.Collections.Generic;
using System.Linq;
using Primer;
using Primer.Animation;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class FruitTree : MonoBehaviour
{
    public Transform fruitPrefab;
    public static float xAngleMax = 5f;
    public static float yAngleMax = 360f;
    public static float zAngleMax = 5f;
    public Rng rng;

    [HideInInspector] public bool skipAnimations = false;
    
    
    public bool hasFruit => flowers.Any(x => x.childCount > 0);
    public Transform[] fruits => flowers.Where(x => x.childCount > 0).Select(x => x.GetChild(0)).ToArray();
    
    [Title("Flowers")]
    public List<Transform> flowers;

    public void Reset()
    {
        flowers.GetChildren().Dispose();
    }
    
    public Tween GrowFruit(int index)
    {
        Transform fruit;
        if (flowers[index].childCount == 0)
        {
            RandomlyRotateFlower(index);
            fruit = Instantiate(fruitPrefab, flowers[index]);
            fruit.localScale = Vector3.zero;
        }
        else
        {
            fruit = flowers[index].GetChild(0);
        }

        return fruit.ScaleTo(1) with {duration = skipAnimations ? 0 : 0.5f};
    }
    public Tween GrowRandomFruitsUpToTotal(int total, float delayRange = 0)
    {
        if (total > flowers.Count)
        {
            total = flowers.Count;
            Debug.LogWarning($"Cannot grow {total} fruit, only {flowers.Count} flowers available");
        }

        // Get indices where there is already a fruit
        var existingFruitIndices = Enumerable.Range(0, flowers.Count)
            .Where(i => flowers[i].childCount > 0).ToArray();
        
        // Choose random indices where there's not already a fruit
        var newFruitIndices = Enumerable.Range(0, flowers.Count)
            .Where(i => flowers[i].childCount == 0)
            .Shuffle(rng: rng)
            .Take(total - existingFruitIndices.Length);
        
        return GrowSpecificFruits(newFruitIndices.Concat(existingFruitIndices).ToArray(), delayRange);
    }

    public Tween GrowSpecificFruits(int[] indices, float delayRange = 0)
    {
        // Create tweens, giving each a random delay between 0 and delayRange
        return indices
            .Select((index, i) => GrowFruit(index) with {delay = skipAnimations ? 0 : rng.Range(delayRange)})
            .RunInParallel();
    }

    private void RandomlyRotateFlower(int index)
    {
        flowers[index].localRotation = Quaternion.Euler(rng.Range(xAngleMax), rng.Range(yAngleMax), rng.Range(zAngleMax));
    }

    public Transform HarvestFruit(Component closestTo = null)
    {
        var candidates = flowers.Where(x => x.childCount > 0);

        if (closestTo is not null) {
            var target = closestTo.transform.position;
            candidates = candidates.OrderBy(x => Vector3.Distance(x.position, target));
        }

        var flower = candidates.FirstOrDefault();
        var fruit = flower != null ? flower.GetChild(0) : null;

        if (fruit is null)
            return null;

        fruit.SetParent(null, worldPositionStays: true);
        return fruit;
    }
}
